import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { App } from "./App";
import type { AuthSession } from "./auth/auth";

const session: AuthSession = {
  initialized: true,
  authenticated: true,
  displayName: "Quentin",
  login: async () => undefined,
  logout: async () => undefined,
  getAccessToken: async () => "test-token",
};

describe("App", () => {
  it("affiche le nom du produit", () => {
    render(<App session={session} />);

    expect(
      screen.getByRole("heading", { name: "MailAssistant" }),
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
