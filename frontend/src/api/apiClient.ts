import type { AuthSession } from "../auth/auth";
import type {
  MatchingResult,
  GmailAccount,
  GmailAuthorization,
  GmailManualClassification,
  Organization,
  OrganizationSettings,
  Project,
  ProjectAlias,
  ProjectInput,
  ProjectUpdate,
} from "./types";

const apiUrl =
  import.meta.env.VITE_API_URL ??
  `${window.location.protocol}//${window.location.hostname}:5080`;

interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
}

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

async function request<T>(
  session: AuthSession,
  path: string,
  init?: RequestInit,
): Promise<T> {
  const accessToken = await session.getAccessToken();
  const headers = new Headers(init?.headers);
  headers.set("Authorization", `Bearer ${accessToken}`);

  if (init?.body && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(`${apiUrl}${path}`, {
    ...init,
    headers,
  });

  if (!response.ok) {
    let problem: ProblemDetails | null = null;
    try {
      problem = (await response.json()) as ProblemDetails;
    } catch {
      // The API did not return problem details.
    }

    throw new ApiError(
      problem?.detail ?? problem?.title ?? `Erreur HTTP ${response.status}`,
      response.status,
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

const jsonBody = (value: unknown): RequestInit => ({
  method: "POST",
  body: JSON.stringify(value),
});

export const api = {
  listOrganizations: (session: AuthSession) =>
    request<Organization[]>(session, "/api/organizations"),

  createOrganization: (session: AuthSession, name: string) =>
    request<Organization>(session, "/api/organizations", jsonBody({ name })),

  listProjects: (session: AuthSession, organizationId: string) =>
    request<Project[]>(session, `/api/organizations/${organizationId}/projects`),

  createProject: (
    session: AuthSession,
    organizationId: string,
    input: ProjectInput,
  ) =>
    request<Project>(
      session,
      `/api/organizations/${organizationId}/projects`,
      jsonBody(input),
    ),

  updateProject: (
    session: AuthSession,
    organizationId: string,
    projectId: string,
    input: ProjectUpdate,
  ) =>
    request<Project>(
      session,
      `/api/organizations/${organizationId}/projects/${projectId}`,
      {
        method: "PUT",
        body: JSON.stringify(input),
      },
    ),

  deleteProject: (
    session: AuthSession,
    organizationId: string,
    projectId: string,
  ) =>
    request<void>(
      session,
      `/api/organizations/${organizationId}/projects/${projectId}`,
      { method: "DELETE" },
    ),

  addAlias: (
    session: AuthSession,
    organizationId: string,
    projectId: string,
    value: string,
  ) =>
    request<ProjectAlias>(
      session,
      `/api/organizations/${organizationId}/projects/${projectId}/aliases`,
      jsonBody({ value }),
    ),

  updateAlias: (
    session: AuthSession,
    organizationId: string,
    projectId: string,
    alias: Pick<ProjectAlias, "id" | "value" | "isActive">,
  ) =>
    request<ProjectAlias>(
      session,
      `/api/organizations/${organizationId}/projects/${projectId}/aliases/${alias.id}`,
      {
        method: "PUT",
        body: JSON.stringify({
          value: alias.value,
          isActive: alias.isActive,
        }),
      },
    ),

  deleteAlias: (
    session: AuthSession,
    organizationId: string,
    projectId: string,
    aliasId: string,
  ) =>
    request<void>(
      session,
      `/api/organizations/${organizationId}/projects/${projectId}/aliases/${aliasId}`,
      { method: "DELETE" },
    ),

  testSubject: (
    session: AuthSession,
    organizationId: string,
    subject: string,
  ) =>
    request<MatchingResult>(
      session,
      `/api/organizations/${organizationId}/matching-tests`,
      jsonBody({ subject }),
    ),

  getSettings: (session: AuthSession, organizationId: string) =>
    request<OrganizationSettings>(
      session,
      `/api/organizations/${organizationId}/settings`,
    ),

  updateSettings: (
    session: AuthSession,
    organizationId: string,
    settings: OrganizationSettings,
  ) =>
    request<OrganizationSettings>(
      session,
      `/api/organizations/${organizationId}/settings`,
      {
        method: "PUT",
        body: JSON.stringify(settings),
      },
    ),

  listGmailAccounts: (session: AuthSession, organizationId: string) =>
    request<GmailAccount[]>(
      session,
      `/api/organizations/${organizationId}/mail-accounts/gmail`,
    ),

  createGmailAuthorization: (
    session: AuthSession,
    organizationId: string,
  ) =>
    request<GmailAuthorization>(
      session,
      `/api/organizations/${organizationId}/mail-accounts/gmail/authorization`,
      { method: "POST" },
    ),

  updateGmailAccount: (
    session: AuthSession,
    organizationId: string,
    mailAccountId: string,
    isAutomaticClassificationEnabled: boolean,
  ) =>
    request<GmailAccount>(
      session,
      `/api/organizations/${organizationId}/mail-accounts/gmail/${mailAccountId}`,
      {
        method: "PUT",
        body: JSON.stringify({ isAutomaticClassificationEnabled }),
      },
    ),

  disconnectGmailAccount: (
    session: AuthSession,
    organizationId: string,
    mailAccountId: string,
  ) =>
    request<void>(
      session,
      `/api/organizations/${organizationId}/mail-accounts/gmail/${mailAccountId}`,
      { method: "DELETE" },
    ),

  classifyGmailMessage: (
    session: AuthSession,
    organizationId: string,
    mailAccountId: string,
    messageId: string,
  ) =>
    request<GmailManualClassification>(
      session,
      `/api/organizations/${organizationId}/mail-accounts/gmail/${mailAccountId}/manual-classifications`,
      jsonBody({ messageId }),
    ),
};
