# Authentification et autorisation

## Responsabilités

Keycloak gère :

- la connexion OpenID Connect ;
- les sessions et la MFA lorsqu’elle est activée ;
- l’émission et la signature des jetons JWT ;
- l’identité externe stable via la claim `sub`.

MailAssistant gère :

- la copie locale minimale de l’utilisateur ;
- les appartenances aux organisations ;
- les rôles propres à chaque organisation ;
- toutes les décisions d’autorisation métier.

Un rôle Keycloak global ne donne donc pas accès à une organisation.

## Clients Keycloak locaux

- `mailassistant-web` : client public, authorization code avec PKCE S256.
- `mailassistant-api` : audience attendue par l’API.
- `mailassistant-tests` : password grant réservé aux tests locaux automatisés.

Le client `mailassistant-tests` ne doit pas exister en production.

## Rôles d’organisation

- `Owner` : toutes les opérations et gestion des membres.
- `Admin` : lecture et gestion des projets/alias, consultation des membres.
- `Member` : lecture des projets et test manuel du matching.

Un utilisateur sans appartenance reçoit HTTP 403, même s’il connaît
l’identifiant de l’organisation.

## Comptes locaux

Le realm de développement fournit :

| Utilisateur | Email | Mot de passe local |
|---|---|---|
| `owner` | `owner@local.test` | `Owner-local-2026!` |
| `admin` | `admin@local.test` | `Admin-local-2026!` |
| `member` | `member@local.test` | `Member-local-2026!` |
| `outsider` | `outsider@local.test` | `Outsider-local-2026!` |

Ces identifiants sont publics, strictement locaux et interdits en production.

## Synchronisation

À la première requête authentifiée, l’application crée une référence locale
avec :

- identifiant interne ;
- `sub` Keycloak ;
- email ;
- nom affiché ;
- dates de création et modification.

Une synchronisation ultérieure ne déclenche une écriture que si les claims
utiles ont changé.

## Frontend

Le frontend utilise `keycloak-js`, le flux authorization code avec PKCE et
conserve le token uniquement en mémoire. Aucun token n’est écrit dans
`localStorage` ou `sessionStorage`.

## Test reproductible

Avec Docker actif :

```powershell
./scripts/test-auth.ps1
```

Le script vérifie les réponses 401/403 et les droits Owner/Admin/Member sur un
tenant réel.
