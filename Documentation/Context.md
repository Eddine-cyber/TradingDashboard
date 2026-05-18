# Contexte de Migration : TradingDashboard.Core v2.0

Ce document résume les discussions architecturales, les décisions de conception et les *breaking changes* appliqués lors de la refonte du module `TradingDashboard.Core` pour s'aligner sur les spécifications du CDC V2.0.

## 1. Objectif Global
L'objectif était de faire du module `Core` la référence de vérité absolue du système de pricing, capable de gérer des options Multi-Actifs, Multi-Devises, FX et Quanto. La rétrocompatibilité avec les anciens modules de `Pricing` et `Risk` (V1.2) a été intentionnellement brisée pour assurer un modèle de domaine pur et performant.

## 2. Décisions Structurelles sur les Instruments (`TradingDashboard.Core.Entities.Instruments`)
- **Suppression du `Underlying` global** : La classe de base abstraite `Instrument` ne contient plus la propriété `Underlying`. Cette propriété a été redescendue dans les contrats Mono-Actif (`VanillaInstrument`, etc.), permettant au `MultiAssetInstrument` de ne contenir qu'une liste propre de composants (`IReadOnlyList<AssetComponent>`), éliminant ainsi le besoin de propriétés artificielles (hacks).
- **Strike et Polymorphisme** : Le polymorphisme de `StrikeOrNull` ne repose plus sur un *anti-pattern* (switch sur les classes enfants). `StrikeOrNull` est désormais une propriété virtuelle sur `Instrument` que les enfants redéfinissent (`override`).
- **Nouveaux Contrats** : Ajout de `BarrierInstrument`, `DigitalInstrument`, et `VarianceSwapInstrument` pour couvrir les nouveaux types définis dans `ProductType`.
- **Réorganisation des constructeurs** : Dans tous les records, le paramètre `Strike` a été réorganisé pour apparaître immédiatement après `ProductType`.

## 3. Données Structurelles vs Données de Marché
- **Nettoyage du `QuantoInstrument`** : Les propriétés telles que la volatilité de change (`FxVolatility`) et les corrélations (`EquityFxCorrelation`) ont été retirées du `QuantoInstrument`. 
- **Principe de conception** : Un objet `Instrument` représente le **contrat légal** (statique). Il fixe donc le `FxQuantoRate` (qui est le taux de change bloqué à la signature du contrat), mais *jamais* les données dynamiques du marché. Les données de volatilité et de corrélation appartiennent exclusivement au `MarketEnvironment`.

## 4. Le Modèle de Données de Marché (`TradingDashboard.Core.Entities.Market`)
- **`MarketEnvironment`** : Refonte totale. Il agit comme un cube d'entrée contenant le taux domestique, les taux étrangers, les paires de change (`FxRates`), les `Snapshots` locaux, et surtout l'unique `GlobalCorrelationMatrix`.
- **`MarketSnapshot`** : Déplacé dans le dossier `Market/` et allégé. Il ne contient plus de `RiskFreeRate` (qui est géré par l'environnement), se concentrant sur le Ticker, le Spot et la Volatilité Implicite.

## 5. Hiérarchie Domestique vs FX : Une Généralisation Mathématique
- **Perspective Mathématique (Modèle Unifié)** : D'un point de vue quantitatif, **toutes les options sont des options FX**. Une option domestique pure n'est qu'un cas particulier (dégénéré) d'une option FX où la devise étrangère (Underlying Currency) est strictement identique à la devise de paiement (Domestic Currency). Dans ce cas précis, le taux de change est constant ($FX = 1.0$) et la volatilité de change est nulle ($\sigma_X = 0$). Le moteur de Monte Carlo V2.0 vectorise tous les facteurs de risque, traitant ainsi les actifs domestiques et étrangers via les mêmes équations de diffusion stochastique.
- **Séparation Structurelle (Domain-Driven Design)** : Bien que mathématiquement unifiés au moment de la simulation, les types `VanillaInstrument` (Domestique) et `FxOptionInstrument` (FX) sont strictement séparés dans les entités du domaine (`TradingDashboard.Core.Entities.Instruments`). Cette ségrégation garantit une excellente ergonomie (UX) pour les traders et la construction d'API : elle évite de polluer la création d'une option classique (ex: Call sur action TotalEnergies en EUR) avec des champs superflus relatifs aux taux de change ou à leur volatilité.

## 6. Architecture des Données de Marché : Le Flux Temps Réel Réactif (Rx)
Le système de distribution des données de marché abandonne les appels synchrones locaux au profit du paradigme des **Reactive Extensions (Rx)** utilisant des flux `IObservable<T>`. Cette architecture est vitale pour le traitement haute-fréquence et le pricing continu :

1. **Ingestion et Structuration (Module Data)** : Un service externe (Data Feed) interroge un fournisseur (ex: Alpha Vantage). Les données brutes temps réel sont ingérées, validées, et transformées en structures de données hautement compactes (mémoire contiguë). Ce module agrège le tout pour maintenir un **"Giga MarketEnvironment"** (ou `GlobalMarketState`) contenant la totalité des spots, taux d'intérêts, et l'immense matrice de corrélation globale de tous les facteurs de risque mondiaux.
2. **Diffusion Asynchrone** : Ce Giga-Environnement est diffusé en continu via `IObservable<GlobalMarketState>`.
3. **Extraction niveau Portefeuille (Méthode de filtrage globale)** : L'objet `Portfolio` (qui regroupe une collection de trades) s'abonne à ce flux (`OnNext`). À chaque mise à jour, le Portfolio exécute une méthode de projection (que l'on nommera `ExtractLocalEnvironment*()`). Cette méthode filtre le flux géant pour n'en conserver que la **sous-matrice de corrélation** et les variables de marché qui concernent *uniquement* les actifs présents dans le portefeuille.
4. **Extraction niveau Instrument (Filtrage local)** : Lors de la phase de pricing, chaque `Instrument` (ou plus précisément son `IPathGenerator`) exécute à nouveau une méthode d'extraction depuis l'environnement du Portefeuille. Le générateur découpe sa propre sous-matrice de corrélation (ex: 3x3 pour un Basket de 3 actifs) à injecter dans l'algorithme de Cholesky. 

Cette conception en "cascade" (Marché Global $\rightarrow$ Environnement Portefeuille $\rightarrow$ Facteurs Instrument) permet au moteur de Monte Carlo de ne manipuler que les plus petits tableaux possibles, optimisant drastiquement les accès CPU/Cache L1 lors du tirage de millions de chemins aléatoires.

## 7. Prochaines Étapes
- Les modules `TradingDashboard.Pricing` et `TradingDashboard.Risk` ne compileront probablement plus suite à la disparition de l'interface `IPathSimulator` et à la modification des signatures de `IPayoff`.
- Implémentation des nouveaux Pricers (Monte Carlo et Analytiques) conformes à `IMonteCarloEngine` et au tenseur `SimulatedPaths`.
