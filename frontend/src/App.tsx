import type { AuthSession } from "./auth/auth";

interface AppProps {
  session: AuthSession;
}

export function App({ session }: AppProps) {
  if (!session.initialized) {
    return (
      <main className="app-shell">
        <p>Initialisation de la session…</p>
      </main>
    );
  }

  if (!session.authenticated) {
    return (
      <main className="app-shell">
        <section className="hero" aria-labelledby="page-title">
          <p className="eyebrow">Classement automatique</p>
          <h1 id="page-title">MailAssistant</h1>
          <p>Connectez-vous pour accéder aux projets de votre organisation.</p>
          <button className="primary-action" type="button" onClick={session.login}>
            Se connecter avec Keycloak
          </button>
        </section>
      </main>
    );
  }

  return (
    <main className="app-shell">
      <section className="hero" aria-labelledby="page-title">
        <p className="eyebrow">Session sécurisée</p>
        <h1 id="page-title">MailAssistant</h1>
        <p>
          Connecté en tant que {session.displayName ?? "utilisateur"}.
        </p>
        <button className="secondary-action" type="button" onClick={session.logout}>
          Se déconnecter
        </button>
      </section>
    </main>
  );
}
