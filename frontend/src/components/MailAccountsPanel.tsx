import { type FormEvent, useEffect, useState } from "react";
import { api } from "../api/apiClient";
import type {
  GmailAccount,
  GmailManualClassification,
} from "../api/types";
import type { AuthSession } from "../auth/auth";
import { StatusMessage } from "./StatusMessage";

interface MailAccountsPanelProps {
  session: AuthSession;
  organizationId: string;
  accounts: GmailAccount[];
  canManage: boolean;
  onAccountsChange: (accounts: GmailAccount[]) => void;
}

export function MailAccountsPanel({
  session,
  organizationId,
  accounts,
  canManage,
  onAccountsChange,
}: MailAccountsPanelProps) {
  const [messageId, setMessageId] = useState("");
  const [selectedAccountId, setSelectedAccountId] = useState(
    accounts[0]?.id ?? "",
  );
  const [result, setResult] = useState<GmailManualClassification | null>(null);
  const [status, setStatus] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    if (!accounts.some((account) => account.id === selectedAccountId)) {
      setSelectedAccountId(accounts[0]?.id ?? "");
    }
  }, [accounts, selectedAccountId]);

  useEffect(() => {
    const parameters = new URLSearchParams(window.location.search);
    if (parameters.has("gmailConnected")) {
      setStatus("Le compte Gmail a été connecté.");
    }
    if (parameters.has("gmailError")) {
      setError(
        `La connexion Gmail a échoué : ${parameters.get("gmailError") ?? "erreur inconnue"}.`,
      );
    }
    if (parameters.has("gmailConnected") || parameters.has("gmailError")) {
      window.history.replaceState({}, "", window.location.pathname);
    }
  }, []);

  const connect = async () => {
    setBusy(true);
    setError(null);
    try {
      const authorization = await api.createGmailAuthorization(
        session,
        organizationId,
      );
      window.location.assign(authorization.authorizationUrl);
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Impossible de démarrer la connexion Gmail.",
      );
      setBusy(false);
    }
  };

  const toggleAutomatic = async (account: GmailAccount) => {
    setError(null);
    try {
      const updated = await api.updateGmailAccount(
        session,
        organizationId,
        account.id,
        !account.isAutomaticClassificationEnabled,
      );
      onAccountsChange(
        accounts.map((current) => (current.id === updated.id ? updated : current)),
      );
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Impossible de modifier le compte Gmail.",
      );
    }
  };

  const disconnect = async (account: GmailAccount) => {
    if (!window.confirm(`Déconnecter ${account.emailAddress} ?`)) {
      return;
    }

    setError(null);
    try {
      await api.disconnectGmailAccount(session, organizationId, account.id);
      onAccountsChange(accounts.filter((current) => current.id !== account.id));
      setStatus("Le compte Gmail a été déconnecté et son token supprimé.");
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Impossible de déconnecter le compte Gmail.",
      );
    }
  };

  const classify = async (event: FormEvent) => {
    event.preventDefault();
    if (!selectedAccountId || !messageId.trim()) {
      setError("Sélectionnez un compte et saisissez un identifiant de message.");
      return;
    }

    setBusy(true);
    setError(null);
    setResult(null);
    try {
      setResult(
        await api.classifyGmailMessage(
          session,
          organizationId,
          selectedAccountId,
          messageId.trim(),
        ),
      );
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Le classement Gmail a échoué.",
      );
    } finally {
      setBusy(false);
    }
  };

  return (
    <section aria-labelledby="mail-accounts-title">
      <header className="page-header">
        <div>
          <p className="eyebrow">Intégrations</p>
          <h2 id="mail-accounts-title">Comptes email</h2>
          <p>Connectez Gmail sans partager votre mot de passe avec MailAssistant.</p>
        </div>
        {canManage && (
          <button
            className="primary-action compact"
            type="button"
            onClick={connect}
            disabled={busy}
          >
            Connecter Gmail
          </button>
        )}
      </header>

      {status && <StatusMessage kind="success">{status}</StatusMessage>}
      {error && <StatusMessage kind="error">{error}</StatusMessage>}

      <div className="mail-account-list">
        {accounts.length === 0 ? (
          <article className="content-card empty-integration">
            <span className="provider-mark">G</span>
            <div>
              <h3>Aucun compte Gmail connecté</h3>
              <p>
                Un Owner ou Admin peut lancer le consentement OAuth Google.
              </p>
            </div>
          </article>
        ) : (
          accounts.map((account) => (
            <article className="content-card mail-account-card" key={account.id}>
              <div className="account-identity">
                <span className="provider-mark">G</span>
                <div>
                  <h3>{account.emailAddress}</h3>
                  <p>Gmail · autorisation active</p>
                </div>
              </div>
              <div className="account-actions">
                <span
                  className={
                    account.isAutomaticClassificationEnabled
                      ? "status-pill active"
                      : "status-pill"
                  }
                >
                  {account.isAutomaticClassificationEnabled
                    ? "Tri automatique actif"
                    : "Tri automatique inactif"}
                </span>
                {canManage && (
                  <>
                    <button
                      type="button"
                      onClick={() => toggleAutomatic(account)}
                    >
                      {account.isAutomaticClassificationEnabled
                        ? "Désactiver"
                        : "Activer"}
                    </button>
                    <button
                      className="danger-action"
                      type="button"
                      onClick={() => disconnect(account)}
                    >
                      Déconnecter
                    </button>
                  </>
                )}
              </div>
            </article>
          ))
        )}
      </div>

      {canManage && accounts.length > 0 && (
        <article className="content-card manual-gmail-card">
          <div className="card-heading">
            <div>
              <h3>Classement manuel de validation</h3>
              <p>
                Saisissez l’identifiant Gmail d’un message pour appliquer la
                règle et le label attendus.
              </p>
            </div>
          </div>
          <form className="gmail-test-form" onSubmit={classify}>
            <label>
              Compte Gmail
              <select
                value={selectedAccountId}
                onChange={(event) => setSelectedAccountId(event.target.value)}
              >
                {accounts.map((account) => (
                  <option value={account.id} key={account.id}>
                    {account.emailAddress}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Identifiant du message
              <input
                value={messageId}
                onChange={(event) => setMessageId(event.target.value)}
                placeholder="Ex. 18f2a3b4c5d6e7f8"
              />
            </label>
            <button className="primary-action" type="submit" disabled={busy}>
              Classer ce message
            </button>
          </form>

          {result && (
            <div className="gmail-result" role="status">
              <strong>
                {result.labelApplied
                  ? `Label « ${result.labelName} » appliqué`
                  : result.outcome === "Conflict"
                    ? "Conflit détecté, aucune action"
                    : "Aucun projet détecté"}
              </strong>
              <span>Sujet normalisé : {result.normalizedSubject || "vide"}</span>
              {result.archived && <span>Le message a aussi été archivé.</span>}
            </div>
          )}
        </article>
      )}
    </section>
  );
}
