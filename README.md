# TradingDashboard

Système de pricing et de gestion de risque pour un portefeuille d'options vanilles, développé en C# 12 / .NET 8.

Le projet couvre la valorisation d'options (Black-Scholes analytique et Monte Carlo), le calcul des sensibilités (Greeks), le suivi du PnL en temps réel, la vérification de limites de risque, le stress testing par scénarios de choc, et l'export de rapports.

---

## Architecture

La solution est organisée en 8 projets selon les principes de la Clean Architecture. Les dépendances sont unidirectionnelles : aucune référence circulaire n'est tolérée.

| Projet                        | Type          | Responsabilité                                                  |
|-------------------------------|---------------|-----------------------------------------------------------------|
| TradingDashboard.Core         | Class Library | Entités, interfaces, enums. Aucune dépendance externe.          |
| TradingDashboard.Pricing      | Class Library | Black-Scholes, Monte Carlo, Greeks, patterns Factory et Strategy. |
| TradingDashboard.Data         | Class Library | Entity Framework Core 8, migrations, pattern Repository.        |
| TradingDashboard.MarketData   | Class Library | Client Alpha Vantage, cache mémoire, résilience Polly.          |
| TradingDashboard.Risk         | Class Library | PnL Engine, LimitChecker, StressTestEngine, pattern Observer.   |
| TradingDashboard.Reports      | Class Library | Export Excel via EPPlus, export PDF via QuestPDF.               |
| TradingDashboard.Dashboard    | Console App   | Point d'entrée, injection de dépendances, affichage temps réel. |
| TradingDashboard.Tests        | xUnit Project | Tests unitaires et d'intégration (couverture minimale : 70%).   |

---

## Stack technique

| Composant         | Choix                                              |
|-------------------|----------------------------------------------------|
| Langage           | C# 12 / .NET 8 LTS                                 |
| ORM               | Entity Framework Core 8                            |
| Base de données   | SQLite                                             |
| Résilience HTTP   | Polly (retry exponentiel, circuit breaker)         |
| Tests             | xUnit, Moq, FluentAssertions                       |
| Export            | EPPlus (Excel), QuestPDF (PDF)                     |
| Données de marché | Alpha Vantage API                                  |

---

## Fonctionnalités

**Pricing**

- Valorisation analytique Black-Scholes pour les calls et puts vanilles. Le prix du put est dérivé par parité Call-Put afin de garantir la cohérence par construction.
- Moteur Monte Carlo parallélisé avec calcul de l'intervalle de confiance à 95%.
- Calcul des Greeks analytiques : Delta, Gamma, Vega, Theta, Rho.

**Gestion des données de marché**

- Récupération des prix en temps réel via l'API Alpha Vantage.
- Cache mémoire avec expiration configurable et rate limiter (token bucket).
- Reconnexion automatique avec backoff exponentiel via Polly.

**PnL et Risk**

- Calcul du PnL journalier, MTD et YTD par position et à l'échelle du portefeuille.
- Vérification en continu des limites de risque (perte journalière, Delta net, Vega net, taille de position).
- Stress testing sur des scénarios de choc marché (spot, volatilité, scénarios combinés).

**Reporting**

- Export des résultats PnL et stress test en fichier Excel et PDF.

---

## Installation

### Prérequis

- .NET 8 SDK
- Une clé API Alpha Vantage (gratuite sur [alphavantage.co](https://www.alphavantage.co))

### Configuration

```bash
git clone https://github.com/TON_USERNAME/TradingDashboard.git
cd TradingDashboard
```

La clé API doit être renseignée via les User Secrets .NET pour ne pas être versionnée :

```bash
cd TradingDashboard.Dashboard
dotnet user-secrets set "AlphaVantage:ApiKey" "VOTRE_CLE"
```

### Base de données

```bash
cd TradingDashboard.Data
dotnet ef database update
```

### Lancement

```bash
cd TradingDashboard.Dashboard
dotnet run
```

### Tests

```bash
dotnet test
```

---

## Limites de risque par défaut

| Limite             | Seuil                    | Niveau d'alerte |
|--------------------|--------------------------|-----------------|
| Perte journalière  | < -50 000 EUR            | Critical        |
| Delta net          | > 100 en valeur absolue  | Warning         |
| Vega nette         | > 500 en valeur absolue  | Warning         |
| Notionnel position | > 10 000 000 EUR         | Info            |

Ces seuils sont configurables via l'objet `LimitConfiguration` injecté au démarrage.

---

## Licence

MIT
