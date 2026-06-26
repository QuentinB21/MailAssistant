# Modèle de données — itération 1

## Organization

Tenant métier qui porte les projets et les règles de classement.

- `Id`
- `Name`
- `CreatedAt`
- `UpdatedAt`

## ApplicationUser

Référence locale minimale d’une identité Keycloak.

- `Id`
- `Subject` unique, issu de la claim `sub`
- `Email`
- `DisplayName`
- `CreatedAt`
- `UpdatedAt`

## OrganizationMembership

Association d’un utilisateur à une organisation.

- `OrganizationId`
- `UserId`
- `Role` : `Owner`, `Admin` ou `Member`
- `CreatedAt`
- `UpdatedAt`

## OrganizationSettings

Configuration de comportement du tenant.

- `OrganizationId`
- `MultipleMatchBehavior`
- `NoMatchBehavior`
- `ArchiveGmailAfterClassification`

Le MVP utilise `MarkAsConflict` pour plusieurs projets et `Ignore` lorsqu’aucun
projet n’est détecté. L’archivage Gmail après classement est désactivé par
défaut et configurable par organisation.

## Project

Projet détectable dans le sujet d’un email.

- `Id`
- `OrganizationId`
- `Name`
- `Description`
- `IsActive`
- `ClassificationTargetName`
- `CreatedAt`
- `UpdatedAt`

Le nom est unique dans une organisation. Toutes les requêtes de projet portent
explicitement l’identifiant de l’organisation.

## ProjectAlias

Terme alternatif permettant de détecter un projet.

- `Id`
- `ProjectId`
- `Value`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

Un alias est unique dans son projet. Le même alias peut être présent dans deux
projets : le moteur retourne alors un conflit explicite.

## Suppression

L’itération 1 utilise une suppression physique, car aucun historique de
traitement ne référence encore les projets. Une suppression logique sera
introduite avant que l’historique rende ces références durables.

## MailAccount

Compte de messagerie connecté à une organisation.

- `Id`
- `OrganizationId`
- `ConnectedByUserId`
- `Provider`
- `EmailAddress`
- `IsAutomaticClassificationEnabled`
- `CreatedAt`
- `UpdatedAt`

Une adresse Gmail ne peut être connectée qu’une fois par organisation.

## OAuthCredential

Credential fournisseur associé à un compte mail.

- `MailAccountId`
- `EncryptedRefreshToken`
- `GrantedScopes`
- `CreatedAt`
- `UpdatedAt`

Le token d’accès n’est pas persisté. Le refresh token est chiffré avant toute
écriture en base.

## ProviderClassificationTarget

Correspondance entre un projet interne et une cible propre à un compte.

- `Id`
- `MailAccountId`
- `ProjectId`
- `ExternalTargetId`
- `ExternalTargetName`
- `CreatedAt`
- `UpdatedAt`

Pour Gmail, la cible externe est un label.
