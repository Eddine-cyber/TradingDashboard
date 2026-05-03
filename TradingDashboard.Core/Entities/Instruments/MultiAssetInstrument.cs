using TradingDashboard.Core.Entities.Schedule;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities.Instruments
{
    /// <summary>
    /// Instrument de type basket option : option sur un panier pondéré d'actifs.
    /// Les actifs peuvent appartenir à des devises différentes.
    ///
    /// Payoff basket call :
    ///   max(Σ w_i * S_i(T) - K, 0)
    /// Les S_i doivent être convertis dans la devise de règlement (DomesticCurrency).
    ///
    /// Non-régression :
    ///   Hérite de Instrument (existant, inchangé). Ajoute des propriétés sans modifier la base.
    ///   Le champ Underlying de la base Instrument est utilisé pour le premier actif du panier
    ///   (backward compat) ; la liste complète est dans BasketComponents.
    /// </summary>
    public record BasketInstrument(
        Guid Id,
        string Ticker,
        Underlying Underlying,                      // premier composant (compat Instrument de base)
        DateOnly Maturity,
        Currency DomesticCurrency,
        double Strike,
        ProductType ProductType,
        IReadOnlyList<BasketComponent> BasketComponents,
        FixingSchedule? Schedule = null)
        : Instrument(Id, Ticker, Underlying, Maturity, DomesticCurrency, ProductType)
    {
        public static BasketInstrument Create(
            string ticker,
            Underlying primaryUnderlying,
            DateOnly maturity,
            Currency domesticCurrency,
            double strike,
            ProductType productType,
            IReadOnlyList<BasketComponent> components,
            FixingSchedule? schedule = null,
            Guid id = default)
        {
            var inst = new BasketInstrument(
                id == Guid.Empty ? Guid.NewGuid() : id,
                ticker, primaryUnderlying, maturity, domesticCurrency,
                strike, productType, components, schedule);
            inst.Validate();
            return inst;
        }

        public override void Validate()
        {
            if (ProductType != ProductType.BasketCall && ProductType != ProductType.BasketPut)
                throw new ArgumentException("ProductType must be BasketCall or BasketPut.");
            if (Strike < 0)
                throw new ArgumentException("Strike must be non-negative.");
            if (BasketComponents.Count < 2)
                throw new ArgumentException("A basket must have at least 2 components.");
            double weightSum = BasketComponents.Sum(c => c.Weight);
            if (Math.Abs(weightSum - 1.0) > 1e-6)
                throw new ArgumentException(
                    $"Basket weights must sum to 1. Got {weightSum:F6}.");
        }

        public override string ToString()
            => $"{Ticker} | {ProductType} | Strike: {Strike} | " +
               $"Components: {BasketComponents.Count} | Maturity: {Maturity:yyyy-MM-dd} | " +
               $"Domestic: {DomesticCurrency} | Id: {Id}";
    }

    /// <summary>
    /// Composant d'un panier : un actif avec son poids et sa devise naturelle.
    /// </summary>
    /// <param name="Underlying">Actif (nom + devise naturelle de cotation).</param>
    /// <param name="Weight">Poids dans le panier (somme des poids = 1).</param>
    public record BasketComponent(
        Underlying Underlying,
        double Weight);
}
