namespace ERP.Factory
{
    public class ExchangeRateResponse
    {
        public string result { get; set; }
        public ConversionRates conversion_rates { get; set; }
    }

    public class ConversionRates
    {
        public double ZAR { get; set; }
    }
}