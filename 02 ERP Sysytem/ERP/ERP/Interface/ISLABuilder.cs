using ERP.Models;

namespace ERP.Interface
{
    public interface ISLABuilder
    {
        void BuildClientInfo(string clientName);
        void BuildShippingTerms(string shippingType);
        void BuildCurrencyRules(decimal? costUsd);
        SLA GetResult();
    }
}
