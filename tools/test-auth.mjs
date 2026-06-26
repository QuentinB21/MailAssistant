import { run, start, stop, waitForHttp } from "./process.mjs";
import { resolve } from "node:path";

const apiBaseUrl = "http://127.0.0.1:5090";
const tokenUrl =
  "http://localhost:8080/realms/mailassistant/protocol/openid-connect/token";
const apiOutputDirectory = resolve(
  "src",
  "MailAssistant.Api",
  "bin",
  "Release",
  "net10.0",
);

async function getAccessToken(username, password) {
  const body = new URLSearchParams({
    client_id: "mailassistant-tests",
    grant_type: "password",
    username,
    password,
    scope: "openid profile email",
  });
  const response = await fetch(tokenUrl, {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body,
  });

  if (!response.ok) {
    throw new Error(`Token request failed for ${username}: ${response.status}`);
  }

  return (await response.json()).access_token;
}

async function apiRequest(method, path, token, body) {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method,
    headers: {
      Authorization: `Bearer ${token}`,
      ...(body ? { "Content-Type": "application/json" } : {}),
    },
    body: body ? JSON.stringify(body) : undefined,
  });

  return response;
}

run("docker", ["compose", "up", "-d", "postgres", "keycloak"]);
await waitForHttp(
  "http://localhost:8080/realms/mailassistant/.well-known/openid-configuration",
  { timeoutMs: 60_000 },
);
run("dotnet", ["tool", "restore"]);
run("dotnet", [
  "ef",
  "database",
  "update",
  "--project",
  "src/MailAssistant.Infrastructure",
  "--startup-project",
  "src/MailAssistant.Api",
]);
run("dotnet", [
  "build",
  "MailAssistant.sln",
  "--configuration",
  "Release",
]);

const api = start(
  "dotnet",
  [resolve(apiOutputDirectory, "MailAssistant.Api.dll")],
  {
    cwd: apiOutputDirectory,
    env: {
      ...process.env,
      ASPNETCORE_URLS: apiBaseUrl,
      ASPNETCORE_ENVIRONMENT: "Development",
    },
    stdio: ["ignore", "pipe", "pipe"],
  },
);

const apiLogs = [];
const captureApiLog = (chunk) => {
  apiLogs.push(chunk.toString());
  if (apiLogs.length > 100) {
    apiLogs.shift();
  }
};
api.stdout.on("data", captureApiLog);
api.stderr.on("data", captureApiLog);

try {
  await waitForHttp(`${apiBaseUrl}/health`, { timeoutMs: 30_000 });

  const owner = await getAccessToken("owner", "Owner-local-2026!");
  const admin = await getAccessToken("admin", "Admin-local-2026!");
  const member = await getAccessToken("member", "Member-local-2026!");
  const outsider = await getAccessToken("outsider", "Outsider-local-2026!");

  for (const token of [owner, admin, member, outsider]) {
    const response = await apiRequest("GET", "/api/me", token);
    if (!response.ok) {
      throw new Error(`User synchronization failed: ${response.status}`);
    }
  }

  const unauthenticated = await fetch(`${apiBaseUrl}/api/organizations`);
  const suffix = crypto.randomUUID().slice(0, 8);
  const createOrganization = await apiRequest(
    "POST",
    "/api/organizations",
    owner,
    { name: `Authorization test ${suffix}` },
  );
  const organization = await createOrganization.json();
  const basePath = `/api/organizations/${organization.id}`;

  for (const membership of [
    { email: "admin@local.test", role: "Admin" },
    { email: "member@local.test", role: "Member" },
  ]) {
    const response = await apiRequest(
      "PUT",
      `${basePath}/members/by-email`,
      owner,
      membership,
    );
    if (!response.ok) {
      throw new Error(`Membership setup failed: ${response.status}`);
    }
  }

  const adminWrite = await apiRequest(
    "POST",
    `${basePath}/projects`,
    admin,
    {
      name: "Admin project",
      classificationTargetName: "Admin project",
      description: null,
    },
  );
  const memberRead = await apiRequest("GET", `${basePath}/projects`, member);
  const memberWrite = await apiRequest(
    "POST",
    `${basePath}/projects`,
    member,
    {
      name: "Forbidden",
      classificationTargetName: "Forbidden",
      description: null,
    },
  );
  const outsiderRead = await apiRequest(
    "GET",
    `${basePath}/projects`,
    outsider,
  );
  const memberGmailRead = await apiRequest(
    "GET",
    `${basePath}/mail-accounts/gmail`,
    member,
  );
  const memberGmailAuthorization = await apiRequest(
    "POST",
    `${basePath}/mail-accounts/gmail/authorization`,
    member,
  );
  const ownerGmailAuthorization = await apiRequest(
    "POST",
    `${basePath}/mail-accounts/gmail/authorization`,
    owner,
  );

  const checks = [
    [unauthenticated.status, 401, "unauthenticated request"],
    [adminWrite.status, 201, "admin write"],
    [memberRead.status, 200, "member read"],
    [memberWrite.status, 403, "member write"],
    [outsiderRead.status, 403, "outsider read"],
    [memberGmailRead.status, 200, "member Gmail read"],
    [memberGmailAuthorization.status, 403, "member Gmail authorization"],
    [ownerGmailAuthorization.status, 503, "unconfigured Gmail authorization"],
  ];

  for (const [actual, expected, label] of checks) {
    if (actual !== expected) {
      throw new Error(`${label}: expected ${expected}, received ${actual}`);
    }
  }

  console.log("Authentication and tenant authorization checks passed.");
} catch (error) {
  console.error(apiLogs.join(""));
  throw error;
} finally {
  await stop(api);
}
