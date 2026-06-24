# ADR 0005 — identité Keycloak et rôles tenant locaux

- Statut : accepté
- Date : 2026-06-24

## Contexte

Keycloak authentifie les utilisateurs, mais les droits Owner/Admin/Member
dépendent de l’organisation ciblée. Un même utilisateur peut avoir des rôles
différents selon le tenant.

## Décision

- La claim Keycloak `sub` identifie l’utilisateur externe.
- Une référence utilisateur minimale est synchronisée en PostgreSQL.
- Les appartenances et rôles sont stockés localement par organisation.
- Les services applicatifs vérifient le rôle avant toute lecture ou mutation.
- Les routes restent tenantées, mais `organizationId` seul ne donne aucun
  accès.

## Conséquences

- L’autorisation reste correcte pour plusieurs organisations.
- Les décisions métier ne dépendent pas de rôles Keycloak globaux.
- Chaque requête protégée nécessite un JWT valide et une appartenance locale.
