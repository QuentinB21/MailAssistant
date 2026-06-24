# Moteur de matching — MVP

## Pipeline

1. Retirer les préfixes répétés `RE:`, `FW:`, `FWD:` et `TR:`.
2. Décomposer les caractères Unicode et retirer les accents.
3. Passer les lettres en minuscules.
4. Remplacer ponctuation et caractères spéciaux par des espaces.
5. Réduire les espaces multiples.
6. Chercher les noms et alias actifs sur des limites de termes.
7. Regrouper les correspondances par projet.
8. Résoudre le résultat en `NoMatch`, `Matched` ou `Conflict`.

## Limites de termes

L’alias `CRM` correspond à `Migration CRM`, mais pas à `Migration ACRMTool`.
Cette règle réduit les faux positifs des alias courts.

## Conflits

- Aucun projet : `NoMatch`, aucune action.
- Un projet : `Matched`, projet sélectionné.
- Plusieurs projets : `Conflict`, aucun projet sélectionné.
- Projet ou alias désactivé : ignoré.

## Extensibilité

Le pipeline dépend des interfaces suivantes :

- `ISubjectNormalizer`
- `IMatchingStrategy`
- `IConflictResolutionPolicy`

Une stratégie regex, score de mots-clés ou IA pourra être ajoutée sans modifier
les adaptateurs Gmail et Outlook.
