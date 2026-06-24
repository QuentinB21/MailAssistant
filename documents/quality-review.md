# Revue périodique de qualité

## Cadence

Une passe dédiée de cleanup/refactoring est réalisée après chaque groupe de
trois itérations fonctionnelles :

- après les itérations 0 à 2 ;
- après les itérations 3 à 5 ;
- après les itérations 6 à 8 ;
- avant la livraison finale.

Cette passe ne doit pas ajouter de fonctionnalité produit. Elle vise à réduire
le coût des itérations suivantes.

## Checklist

### Architecture

- frontières `Domain`, `Application`, `Infrastructure`, `Api` et `Worker` ;
- absence de dépendance fournisseur dans le domaine ;
- services applicatifs avec une responsabilité identifiable ;
- autorisations vérifiées côté backend ;
- requêtes tenantées et absence de fuite inter-organisations.

### Code

- noms explicites et méthodes de taille raisonnable ;
- invariants métier portés par le domaine ;
- absence de duplication significative ;
- comparaisons, culture, dates et identifiants traités explicitement ;
- erreurs externes assainies ;
- aucun secret ou token dans le code ou les logs.

### Données

- migration cohérente avec le modèle ;
- index et contraintes uniques adaptés ;
- minimisation des données personnelles ;
- stratégie de suppression et de rétention documentée.

### Tests

- build sans avertissement ;
- tests unitaires et d’intégration ;
- migrations sans changement de modèle en attente ;
- lint, build et audit frontend ;
- scénarios négatifs de sécurité ;
- commandes locales reproductibles.

### Documentation et exploitation

- README à jour ;
- documents regroupés dans `documents/` ;
- ADR ajoutés pour les décisions durables ;
- `PROJECT_STATE.md` fidèle à l’état réel ;
- procédures indépendantes du système d’exploitation lorsque possible.

## Sortie attendue

La revue produit :

1. une liste courte des problèmes trouvés ;
2. les corrections réalisables sans changement fonctionnel ;
3. les dettes différées avec justification ;
4. une exécution réussie de `npm run verify` ;
5. une entrée dans le journal de `documents/PROJECT_STATE.md`.

## Revue 0–2 — 2026-06-24

Problèmes corrigés :

- documentation dispersée à la racine et dans `docs/` ;
- scripts PowerShell non portables ;
- arrêt incomplet des processus enfants Vite sous Windows ;
- service projet mélangeant CRUD et orchestration du matching ;
- requêtes N+1 pour les listes d’organisations et de membres ;
- hiérarchie des rôles dépendante de l’ordre numérique de l’enum ;
- emails non normalisés avant stockage et recherche.

Dette différée :

- découpage des fichiers Minimal API lorsque les routes augmenteront ;
- tests d’architecture automatisés entre les projets .NET ;
- montée de version majeure React 19 et TypeScript 6, à traiter dans une tâche
  dédiée plutôt que pendant une passe sans changement fonctionnel.
