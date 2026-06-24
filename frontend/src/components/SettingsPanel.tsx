import { type FormEvent, useEffect, useState } from "react";
import { api } from "../api/apiClient";
import type { OrganizationSettings } from "../api/types";
import type { AuthSession } from "../auth/auth";
import { StatusMessage } from "./StatusMessage";

interface SettingsPanelProps {
  session: AuthSession;
  organizationId: string;
  canManage: boolean;
}

export function SettingsPanel({
  session,
  organizationId,
  canManage,
}: SettingsPanelProps) {
  const [settings, setSettings] = useState<OrganizationSettings | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    setSettings(null);
    setError(null);
    api
      .getSettings(session, organizationId)
      .then(setSettings)
      .catch((requestError: unknown) =>
        setError(
          requestError instanceof Error
            ? requestError.message
            : "Impossible de charger les paramètres.",
        ),
      );
  }, [organizationId, session]);

  const save = async (event: FormEvent) => {
    event.preventDefault();
    if (!settings || !canManage) {
      return;
    }

    setError(null);
    setSaved(false);
    try {
      setSettings(await api.updateSettings(session, organizationId, settings));
      setSaved(true);
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Impossible d’enregistrer les paramètres.",
      );
    }
  };

  return (
    <section aria-labelledby="settings-title">
      <header className="page-header">
        <div>
          <p className="eyebrow">Comportement</p>
          <h2 id="settings-title">Paramètres de classement</h2>
          <p>Configurez les décisions sûres appliquées par défaut.</p>
        </div>
      </header>

      <form className="content-card settings-form" onSubmit={save}>
        {!settings && !error && <p>Chargement des paramètres…</p>}
        {error && <StatusMessage kind="error">{error}</StatusMessage>}
        {saved && (
          <StatusMessage kind="success">
            Les paramètres ont été enregistrés.
          </StatusMessage>
        )}
        {settings && (
          <>
            <fieldset disabled={!canManage}>
              <legend>En cas de plusieurs projets détectés</legend>
              <label className="choice-card">
                <input type="radio" checked readOnly />
                <span>
                  <strong>Signaler un conflit, sans action</strong>
                  <small>
                    Empêche un déplacement arbitraire vers le mauvais projet.
                  </small>
                </span>
              </label>
            </fieldset>

            <fieldset disabled={!canManage}>
              <legend>Si aucun projet n’est détecté</legend>
              <label className="choice-card">
                <input type="radio" checked readOnly />
                <span>
                  <strong>Ignorer le message</strong>
                  <small>Le message reste dans son emplacement actuel.</small>
                </span>
              </label>
            </fieldset>

            <fieldset disabled={!canManage}>
              <legend>Comportement Gmail</legend>
              <label className="choice-card">
                <input
                  type="checkbox"
                  checked={settings.archiveGmailAfterClassification}
                  onChange={(event) =>
                    setSettings({
                      ...settings,
                      archiveGmailAfterClassification: event.target.checked,
                    })
                  }
                />
                <span>
                  <strong>Archiver après l’ajout du label</strong>
                  <small>
                    Retire le message de la boîte de réception une fois classé.
                  </small>
                </span>
              </label>
            </fieldset>

            {canManage ? (
              <button className="primary-action" type="submit">
                Enregistrer les paramètres
              </button>
            ) : (
              <StatusMessage kind="info">
                Votre rôle autorise la consultation, mais pas la modification.
              </StatusMessage>
            )}
          </>
        )}
      </form>
    </section>
  );
}
