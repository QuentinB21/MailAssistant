import { type FormEvent, useEffect, useMemo, useState } from "react";
import { api } from "../api/apiClient";
import type { GmailAccount, Organization, Project } from "../api/types";
import type { AuthSession } from "../auth/auth";
import { Dashboard } from "./Dashboard";
import { MatchingTester } from "./MatchingTester";
import { MailAccountsPanel } from "./MailAccountsPanel";
import { ProjectManager } from "./ProjectManager";
import { SettingsPanel } from "./SettingsPanel";
import { StatusMessage } from "./StatusMessage";

type View = "dashboard" | "projects" | "accounts" | "matching" | "settings";

interface WorkspaceProps {
  session: AuthSession;
}

const viewLabels: Record<View, string> = {
  dashboard: "Tableau de bord",
  projects: "Projets",
  accounts: "Comptes email",
  matching: "Tester un sujet",
  settings: "Paramètres",
};

export function Workspace({ session }: WorkspaceProps) {
  const [organizations, setOrganizations] = useState<Organization[]>([]);
  const [selectedOrganizationId, setSelectedOrganizationId] = useState<
    string | null
  >(null);
  const [projects, setProjects] = useState<Project[]>([]);
  const [gmailAccounts, setGmailAccounts] = useState<GmailAccount[]>([]);
  const [view, setView] = useState<View>("dashboard");
  const [organizationName, setOrganizationName] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const selectedOrganization = useMemo(
    () =>
      organizations.find(
        (organization) => organization.id === selectedOrganizationId,
      ) ?? null,
    [organizations, selectedOrganizationId],
  );
  const canManage =
    selectedOrganization?.role === "Owner" ||
    selectedOrganization?.role === "Admin";

  useEffect(() => {
    let active = true;
    api
      .listOrganizations(session)
      .then((results) => {
        if (!active) {
          return;
        }

        setOrganizations(results);
        const remembered = window.localStorage.getItem(
          "mailassistant.organization",
        );
        const initial =
          results.find((organization) => organization.id === remembered) ??
          results[0] ??
          null;
        setSelectedOrganizationId(initial?.id ?? null);
      })
      .catch((requestError: unknown) => {
        if (active) {
          setError(
            requestError instanceof Error
              ? requestError.message
              : "Impossible de charger les organisations.",
          );
        }
      })
      .finally(() => {
        if (active) {
          setLoading(false);
        }
      });

    return () => {
      active = false;
    };
  }, [session]);

  useEffect(() => {
    if (!selectedOrganizationId) {
      setProjects([]);
      setGmailAccounts([]);
      return;
    }

    window.localStorage.setItem(
      "mailassistant.organization",
      selectedOrganizationId,
    );
    setError(null);
    Promise.all([
      api.listProjects(session, selectedOrganizationId),
      api.listGmailAccounts(session, selectedOrganizationId),
    ])
      .then(([projectResults, gmailResults]) => {
        setProjects(projectResults);
        setGmailAccounts(gmailResults);
      })
      .catch((requestError: unknown) =>
        setError(
          requestError instanceof Error
            ? requestError.message
            : "Impossible de charger les projets.",
        ),
      );
  }, [selectedOrganizationId, session]);

  const createOrganization = async (event: FormEvent) => {
    event.preventDefault();
    if (!organizationName.trim()) {
      setError("Saisissez un nom d’organisation.");
      return;
    }

    setError(null);
    try {
      const created = await api.createOrganization(
        session,
        organizationName.trim(),
      );
      setOrganizations([...organizations, created]);
      setSelectedOrganizationId(created.id);
      setOrganizationName("");
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Impossible de créer l’organisation.",
      );
    }
  };

  if (loading) {
    return (
      <main className="loading-screen">
        <div className="brand-mark">M</div>
        <p>Chargement de votre espace…</p>
      </main>
    );
  }

  if (organizations.length === 0) {
    return (
      <main className="onboarding-shell">
        <section className="onboarding-card">
          <div className="brand-lockup">
            <span className="brand-mark">M</span>
            <strong>MailAssistant</strong>
          </div>
          <p className="eyebrow">Premier espace</p>
          <h1>Créez votre organisation</h1>
          <p>
            Les projets, règles et comptes email seront cloisonnés dans cet
            espace.
          </p>
          <form onSubmit={createOrganization}>
            <label htmlFor="organization-name">Nom de l’organisation</label>
            <input
              id="organization-name"
              value={organizationName}
              onChange={(event) => setOrganizationName(event.target.value)}
              placeholder="Ex. Acme France"
              maxLength={200}
              autoFocus
            />
            {error && <StatusMessage kind="error">{error}</StatusMessage>}
            <button className="primary-action" type="submit">
              Créer mon espace
            </button>
          </form>
        </section>
      </main>
    );
  }

  if (!selectedOrganization) {
    return (
      <main className="loading-screen">
        <StatusMessage kind="error">
          L’organisation sélectionnée est introuvable.
        </StatusMessage>
      </main>
    );
  }

  return (
    <div className="workspace">
      <aside className="sidebar">
        <div className="brand-lockup">
          <span className="brand-mark">M</span>
          <strong>MailAssistant</strong>
        </div>

        <label className="organization-picker">
          <span>Organisation</span>
          <select
            value={selectedOrganization.id}
            onChange={(event) => {
              setSelectedOrganizationId(event.target.value);
              setView("dashboard");
            }}
          >
            {organizations.map((organization) => (
              <option value={organization.id} key={organization.id}>
                {organization.name}
              </option>
            ))}
          </select>
        </label>

        <nav aria-label="Navigation principale">
          {(Object.keys(viewLabels) as View[]).map((item) => (
            <button
              className={view === item ? "active" : ""}
              type="button"
              key={item}
              onClick={() => setView(item)}
            >
              <span className="nav-icon" aria-hidden="true">
                {item === "dashboard"
                  ? "⌂"
                  : item === "projects"
                    ? "▣"
                  : item === "accounts"
                    ? "@"
                    : item === "matching"
                      ? "✓"
                      : "⚙"}
              </span>
              {viewLabels[item]}
            </button>
          ))}
        </nav>

        <div className="sidebar-footer">
          <div className="user-summary">
            <span className="avatar">
              {(session.displayName ?? "U").charAt(0).toUpperCase()}
            </span>
            <span>
              <strong>{session.displayName ?? "Utilisateur"}</strong>
              <small>{selectedOrganization.role}</small>
            </span>
          </div>
          <button
            className="logout-button"
            type="button"
            onClick={session.logout}
          >
            Se déconnecter
          </button>
        </div>
      </aside>

      <main className="workspace-main">
        {error && <StatusMessage kind="error">{error}</StatusMessage>}
        {view === "dashboard" && (
          <Dashboard
            organization={selectedOrganization}
            projects={projects}
            gmailAccounts={gmailAccounts}
          />
        )}
        {view === "accounts" && (
          <MailAccountsPanel
            session={session}
            organizationId={selectedOrganization.id}
            accounts={gmailAccounts}
            canManage={canManage}
            onAccountsChange={setGmailAccounts}
          />
        )}
        {view === "projects" && (
          <ProjectManager
            session={session}
            organizationId={selectedOrganization.id}
            projects={projects}
            canManage={canManage}
            onProjectsChange={setProjects}
          />
        )}
        {view === "matching" && (
          <MatchingTester
            session={session}
            organizationId={selectedOrganization.id}
          />
        )}
        {view === "settings" && (
          <SettingsPanel
            session={session}
            organizationId={selectedOrganization.id}
            canManage={canManage}
          />
        )}
      </main>
    </div>
  );
}
