using ERP.Factory;
using ERP.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

//Code attribution
//Make HTTP requests with the HttpClient class
//Available at: https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient
//[Accessed: 1 June 2026]

//Code attribution
//Exception-handling statements - throw, try-catch, try-finally, and try-catch-finally
//Available at: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/guidelines
//[Accessed: 1 June 2026]

namespace ERP.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        // The CurrencyService is responsible for fetching the current USD to ZAR exchange rate
        // from an external API and performing currency conversions. It uses HttpClient
        // to make API requests and IConfiguration to access necessary configuration values such as the API key and base URL.
        public CurrencyService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        // This method retrieves the current exchange rate from USD to ZAR by calling an external currency exchange API.
        public async Task<decimal> GetZarRateAsync()
        {
            var apiKey = _config["CurrencyApi:ApiKey"] ?? "265f49aae3f3392715d495d9";
            var baseUrl = _config["CurrencyApi:BaseUrl"] ?? "https://v6.exchangerate-api.com/v6/";

            // Clean string interpolation: https://v6.exchangerate-api.com/v6/API_KEY/latest/USD
            string requestUrl = $"{baseUrl.TrimEnd('/')}/{apiKey}/latest/USD";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<ExchangeRateResponse>(requestUrl);

                if (response?.conversion_rates?.ZAR == null)
                {
                    throw new HttpRequestException("The currency server returned an invalid or empty data payload.");
                }

                return (decimal)response.conversion_rates.ZAR;
            }
            catch (Exception ex) when (!(ex is HttpRequestException))
            {
                throw new HttpRequestException("The external currency exchange validation server is currently offline or unreachable. Please try again later.", ex);
            }
        }

        // This method converts a given amount in USD to ZAR using the current exchange rate fetched from the API.
        public async Task<decimal> ConvertUsdToZarAsync(decimal amountUsd)
        {
            decimal rate = await GetZarRateAsync();
            return amountUsd * rate;
        }
    }
}