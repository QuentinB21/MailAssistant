import { run, start, stop, waitForHttp } from "./process.mjs";

run("docker", [
  "compose",
  "up",
  "-d",
  "postgres",
  "rabbitmq",
  "keycloak",
]);
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

const api = start("dotnet", [
  "run",
  "--project",
  "src/MailAssistant.Api",
  "--configuration",
  "Release",
]);
const worker = start("dotnet", [
  "run",
  "--project",
  "src/MailAssistant.Worker",
  "--configuration",
  "Release",
], {
  env: {
    ...process.env,
    DOTNET_ENVIRONMENT: "Development",
  },
});
const frontend = start("npm", [
  "run",
  "dev",
  "--prefix",
  "frontend",
  "--",
  "--host",
  "0.0.0.0",
]);

let stopping = false;
const shutdown = async () => {
  if (stopping) {
    return;
  }

  stopping = true;
  await Promise.all([stop(frontend), stop(worker), stop(api)]);
};

process.on("SIGINT", shutdown);
process.on("SIGTERM", shutdown);

try {
  await Promise.all([
    waitForHttp("http://localhost:5080/health", { timeoutMs: 60_000 }),
    waitForHttp("http://localhost:5173", { timeoutMs: 60_000 }),
  ]);
  console.log("MailAssistant is available at http://localhost:5173");

  if (process.argv.includes("--smoke")) {
    await shutdown();
    console.log("Development startup smoke test passed.");
    process.exitCode = 0;
  } else {
    await Promise.race([
      new Promise((resolve) => api.once("exit", resolve)),
      new Promise((resolve) => worker.once("exit", resolve)),
      new Promise((resolve) => frontend.once("exit", resolve)),
    ]);
  }
} finally {
  await shutdown();
}
