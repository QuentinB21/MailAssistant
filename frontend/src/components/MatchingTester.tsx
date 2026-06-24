import { type FormEvent, useState } from "react";
import { api } from "../api/apiClient";
import type { MatchingResult } from "../api/types";
import type { AuthSession } from "../auth/auth";
import { StatusMessage } from "./StatusMessage";

interface MatchingTesterProps {
  session: AuthSession;
  organizationId: string;
}

const outcomeLabels: Record<MatchingResult["outcome"], string> = {
  Matched: "Projet détecté",
  NoMatch: "Aucun projet détecté",
  Conflict: "Conflit entre plusieurs projets",
};

export function MatchingTester({
  session,
  organizationId,
}: MatchingTesterProps) {
  const [subject, setSubject] = useState("");
  const [result, setResult] = useState<MatchingResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    if (!subject.trim()) {
      setError("Saisissez un objet d’email à analyser.");
      return;
    }

    setSubmitting(true);
    setError(null);
    try {
      setResult(await api.testSubject(session, organizationId, subject.trim()));
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Le test du sujet a échoué.",
      );
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <section aria-labelledby="matching-title">
      <header className="page-header">
        <div>
          <p className="eyebrow">Banc d’essai</p>
          <h2 id="matching-title">Tester un objet d’email</h2>
          <p>
            Vérifiez le résultat et la règle appliquée sans déplacer de message.
          </p>
        </div>
      </header>

      <div className="two-column-layout">
        <form className="content-card form-card" onSubmit={submit}>
          <label htmlFor="subject">Objet de l’email</label>
          <textarea
            id="subject"
            rows={5}
            value={subject}
            onChange={(event) => setSubject(event.target.value)}
            placeholder="RE: Compte rendu Projet Apollo"
          />
          <p className="field-help">
            Les préfixes RE, FW, FWD et TR, la casse, les accents et la
            ponctuation sont normalisés.
          </p>
          {error && <StatusMessage kind="error">{error}</StatusMessage>}
          <button className="primary-action" type="submit" disabled={submitting}>
            {submitting ? "Analyse…" : "Analyser le sujet"}
          </button>
        </form>

        <article className="content-card result-card" aria-live="polite">
          {!result ? (
            <div className="empty-state compact">
              <span className="empty-icon">Aa</span>
              <h3>Résultat du matching</h3>
              <p>L’explication apparaîtra ici après l’analyse.</p>
            </div>
          ) : (
            <>
              <span
                className={`outcome-badge outcome-${result.outcome.toLowerCase()}`}
              >
                {outcomeLabels[result.outcome]}
              </span>
              <h3>{outcomeLabels[result.outcome]}</h3>
              <dl className="result-details">
                <div>
                  <dt>Sujet normalisé</dt>
                  <dd>{result.normalizedSubject || "Sujet vide"}</dd>
                </div>
                <div>
                  <dt>Décision</dt>
                  <dd>
                    {result.outcome === "Matched"
                      ? "Le projet unique peut être classé automatiquement."
                      : result.outcome === "Conflict"
                        ? "Aucune action automatique : une décision est requise."
                        : "Le message restera inchangé."}
                  </dd>
                </div>
              </dl>
              {result.matches.length > 0 && (
                <ul className="match-list">
                  {result.matches.map((match) => (
                    <li key={match.projectId}>
                      <strong>{match.projectName}</strong>
                      <span>
                        via {match.source === "Alias" ? "l’alias" : "le nom"} «
                        {match.matchedValue} »
                      </span>
                    </li>
                  ))}
                </ul>
              )}
            </>
          )}
        </article>
      </div>
    </section>
  );
}
