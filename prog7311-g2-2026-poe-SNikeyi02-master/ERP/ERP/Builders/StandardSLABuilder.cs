using ERP.Interface;
using ERP.Models;

namespace ERP.Builders
{
    public class StandardSLABuilder : ISLABuilder
    {
        private SLA _sla = new SLA();

        // 1. Updated methods to accept and inject dynamic details
        public void BuildClientInfo(string clientName)
        {
            _sla.Details += $"Client Profile: {clientName}; ";
        }

        public void BuildShippingTerms(string shippingType)
        {
            // Fallback gracefully if no specific type is assigned yet
            string type = string.IsNullOrEmpty(shippingType) ? "Standard Ground" : shippingType;
            _sla.Details += $"Shipping Method: {type} Logistics; ";
        }

        public void BuildCurrencyRules(decimal? costUsd)
        {
            string priceInfo = costUsd.HasValue ? $"${costUsd.Value:N2} USD Base" : "Rates pending calculation";
            _sla.Details += $"Financial Terms: {priceInfo} (Subject to Live ZAR Validation); ";
        }

        public SLA GetResult() => _sla;

        public void BuildClientInfo()
        {
            throw new NotImplementedException();
        }

        public void BuildShippingTerms()
        {
            throw new NotImplementedException();
        }

        public void BuildCurrencyRules()
        {
            throw new NotImplementedException();
        }
    }
}