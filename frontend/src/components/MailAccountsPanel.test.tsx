import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import type { AuthSession } from "../auth/auth";
import { MailAccountsPanel } from "./MailAccountsPanel";

const session: AuthSession = {
  initialized: true,
  authenticated: true,
  displayName: "Quentin",
  login: vi.fn(async () => undefined),
  logout: vi.fn(async () => undefined),
  getAccessToken: vi.fn(async () => "test-token"),
};

describe("MailAccountsPanel", () => {
  it("affiche un compte Gmail et permet de lancer un classement manuel", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValueOnce(
      new Response(
        JSON.stringify({
          outcome: "Matched",
          normalizedSubject: "suivi apollo",
          projectId: "project-1",
          projectName: "Apollo",
          labelName: "Projet Apollo",
          labelApplied: true,
          archived: false,
        }),
        {
          status: 200,
          headers: { "Content-Type": "application/json" },
        },
      ),
    );

    render(
      <MailAccountsPanel
        session={session}
        organizationId="org-1"
        accounts={[
          {
            id: "gmail-1",
            organizationId: "org-1",
            emailAddress: "owner@example.test",
            isAutomaticClassificationEnabled: false,
            createdAt: "2026-06-24T00:00:00Z",
            updatedAt: "2026-06-24T00:00:00Z",
          },
        ]}
        canManage
        onAccountsChange={vi.fn()}
      />,
    );

    fireEvent.change(screen.getByLabelText("Identifiant du message"), {
      target: { value: "message-123" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Classer ce message" }));

    expect(
      await screen.findByText("Label « Projet Apollo » appliqué"),
    ).toBeInTheDocument();
  });
});
