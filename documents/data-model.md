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

Le MVP utilise `MarkAsConflict` pour plusieurs projets et `Ignore` lorsqu’aucun
projet n’est détecté.

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
