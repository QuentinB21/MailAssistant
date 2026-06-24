import type { Organization, Project } from "../api/types";

interface DashboardProps {
  organization: Organization;
  projects: Project[];
}

export function Dashboard({ organization, projects }: DashboardProps) {
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
          <strong>0</strong>
          <small>Gmail et Outlook arrivent aux prochaines itérations</small>
        </article>
        <article className="metric-card">
          <span>Tri automatique</span>
          <strong className="metric-text">En attente</strong>
          <small>Le moteur de règles est prêt</small>
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
          <li>
            <span>3</span>
            Un compte Gmail ou Outlook connecté
          </li>
        </ul>
      </article>
    </section>
  );
}
