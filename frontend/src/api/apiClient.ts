import type { AuthSession } from "../auth/auth";

const apiUrl = import.meta.env.VITE_API_URL ?? "http://localhost:5080";

export async function apiFetch(
  session: AuthSession,
  path: string,
  init?: RequestInit,
) {
  const accessToken = await session.getAccessToken();
  const headers = new Headers(init?.headers);
  headers.set("Authorization", `Bearer ${accessToken}`);

  if (init?.body && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  return fetch(`${apiUrl}${path}`, {
    ...init,
    headers,
  });
}
