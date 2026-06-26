import type { GmailAccount, Organization, Project } from "../api/types";

interface DashboardProps {
  organization: Organization;
  projects: Project[];
  gmailAccounts: GmailAccount[];
}

export function Dashboard({
  organization,
  projects,
  gmailAccounts,
}: DashboardProps) {
  const activeProjects = projects.filter((project) => project.isActive);
  const aliasCount = projects.reduce(
    (total, project) =>
      total + project.aliases.filter((alias) => alias.isActive).length,
    0,
  );

  return (
    <section aria-labelledby="dashboard-title">
      <header className="page-header">
        <div>
          <p className="eyebrow">Vue d’ensemble</p>
          <h2 id="dashboard-title">{organization.name}</h2>
          <p>
            Suivez l’état des règles de classement avant de connecter vos
            comptes email.
          </p>
        </div>
        <span className={`role-badge role-${organization.role.toLowerCase()}`}>
          {organization.role}
        </span>
      </header>

      <div className="metric-grid">
        <article className="metric-card">
          <span>Projets actifs</span>
          <strong>{activeProjects.length}</strong>
          <small>{projects.length} projet(s) configuré(s)</small>
        </article>
        <article className="metric-card">
          <span>Alias actifs</span>
          <strong>{aliasCount}</strong>
          <small>Termes alternatifs surveillés</small>
        </article>
        <article className="metric-card">
          <span>Comptes connectés</span>
          <strong>{gmailAccounts.length}</strong>
          <small>Compte(s) Gmail autorisé(s)</small>
        </article>
        <article className="metric-card">
          <span>Tri automatique</span>
          <strong className="metric-text">
            {gmailAccounts.some(
              (account) => account.isAutomaticClassificationEnabled,
            )
              ? "Actif"
              : "En attente"}
          </strong>
          <small>Activation configurée par compte</small>
        </article>
      </div>

      <article className="content-card">
        <div className="card-heading">
          <div>
            <h3>Préparation du classement</h3>
            <p>Les éléments nécessaires avant la connexion d’un fournisseur.</p>
          </div>
        </div>
        <ul className="readiness-list">
          <li className={activeProjects.length > 0 ? "complete" : ""}>
            <span>1</span>
            Au moins un projet actif
          </li>
          <li className={aliasCount > 0 ? "complete" : ""}>
            <span>2</span>
            Des alias pour les sujets non standardisés
          </li>
          <li className={gmailAccounts.length > 0 ? "complete" : ""}>
            <span>3</span>
            Un compte Gmail ou Outlook connecté
          </li>
        </ul>
      </article>
    </section>
  );
}
