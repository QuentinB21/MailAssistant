import { render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { App } from "./App";
import type { AuthSession } from "./auth/auth";

const session: AuthSession = {
  initialized: true,
  authenticated: true,
  displayName: "Quentin",
  login: vi.fn(async () => undefined),
  logout: vi.fn(async () => undefined),
  getAccessToken: vi.fn(async () => "test-token"),
};

const jsonResponse = (body: unknown) =>
  Promise.resolve(
    new Response(JSON.stringify(body), {
      status: 200,
      headers: { "Content-Type": "application/json" },
    }),
  );

afterEach(() => {
  vi.restoreAllMocks();
  window.localStorage.clear();
});

describe("App", () => {
  it("affiche l’espace authentifié et son tableau de bord", async () => {
    vi.spyOn(globalThis, "fetch")
      .mockImplementationOnce(() =>
        jsonResponse([
          {
            id: "org-1",
            name: "Acme",
            role: "Owner",
            createdAt: "2026-06-24T00:00:00Z",
            updatedAt: "2026-06-24T00:00:00Z",
          },
        ]),
      )
      .mockImplementationOnce(() =>
        jsonResponse([
          {
            id: "project-1",
            organizationId: "org-1",
            name: "Apollo",
            description: null,
            isActive: true,
            classificationTargetName: "Apollo",
            createdAt: "2026-06-24T00:00:00Z",
            updatedAt: "2026-06-24T00:00:00Z",
            aliases: [],
          },
        ]),
      );

    render(<App session={session} />);

    expect(await screen.findByText("MailAssistant")).toBeInTheDocument();
    expect(
      await screen.findByRole("heading", { name: "Acme" }),
    ).toBeInTheDocument();
    expect(screen.getByText("1 projet(s) configuré(s)")).toBeInTheDocument();
  });

  it("propose la création d’une organisation pour un nouveau compte", async () => {
    vi.spyOn(globalThis, "fetch").mockImplementationOnce(() => jsonResponse([]));

    render(<App session={session} />);

    expect(
      await screen.findByRole("heading", { name: "Créez votre organisation" }),
    ).toBeInTheDocument();
  });

  it("propose une connexion lorsque la session est absente", () => {
    render(
      <App
        session={{
          ...session,
          authenticated: false,
          displayName: null,
        }}
      />,
    );

    expect(
      screen.getByRole("button", { name: "Se connecter avec Keycloak" }),
    ).toBeInTheDocument();
  });
});
