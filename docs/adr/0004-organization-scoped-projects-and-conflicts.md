# ADR 0004 — projets par organisation et conflits sans action

- Statut : accepté
- Date : 2026-06-24

## Contexte

Le cahier des charges laisse ouverte la portée exacte des projets et le
comportement lorsqu’un sujet correspond à plusieurs projets.

## Décision

- Les projets, alias et règles sont portés par une organisation.
- Un message ne sélectionne qu’un projet dans le MVP.
- Plusieurs projets détectés produisent le statut `Conflict` sans action
  automatique.
- Les routes de l’itération 1 contiennent explicitement `organizationId`.
  L’itération Keycloak ajoutera la vérification d’appartenance et de rôle.

## Conséquences

- Le cloisonnement est présent dès le modèle et les contraintes de requête.
- Un classement arbitraire ne peut pas déplacer un message vers le mauvais
  projet.
- Les conflits restent observables et pourront recevoir une politique
  configurable ultérieurement.
