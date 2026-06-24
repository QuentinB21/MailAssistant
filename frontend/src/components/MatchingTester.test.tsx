import { fireEvent, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import type { AuthSession } from "../auth/auth";
import { MatchingTester } from "./MatchingTester";

const session: AuthSession = {
  initialized: true,
  authenticated: true,
  displayName: "Quentin",
  login: vi.fn(async () => undefined),
  logout: vi.fn(async () => undefined),
  getAccessToken: vi.fn(async () => "test-token"),
};

afterEach(() => {
  vi.restoreAllMocks();
});

describe("MatchingTester", () => {
  it("explique le projet détecté", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValueOnce(
      new Response(
        JSON.stringify({
          outcome: "Matched",
          normalizedSubject: "compte rendu apollo",
          selectedProjectId: "project-1",
          matches: [
            {
              projectId: "project-1",
              projectName: "Apollo",
              matchedValue: "Apollo",
              source: "ProjectName",
            },
          ],
        }),
        {
          status: 200,
          headers: { "Content-Type": "application/json" },
        },
      ),
    );

    render(<MatchingTester session={session} organizationId="org-1" />);

    fireEvent.change(screen.getByLabelText("Objet de l’email"), {
      target: { value: "RE: Compte rendu Apollo" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Analyser le sujet" }));

    expect(await screen.findByText("compte rendu apollo")).toBeInTheDocument();
    expect(screen.getByText("via le nom «Apollo »")).toBeInTheDocument();
  });
});
