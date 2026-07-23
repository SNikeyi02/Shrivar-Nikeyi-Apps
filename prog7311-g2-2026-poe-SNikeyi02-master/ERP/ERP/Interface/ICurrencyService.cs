namespace ERP.Services
{
    public interface ICurrencyService
    {
        Task<decimal> GetZarRateAsync();
        Task<decimal> ConvertUsdToZarAsync(decimal amountUsd);
    }
}