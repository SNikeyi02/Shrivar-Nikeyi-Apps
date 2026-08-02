using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

//Code attribution
//Model Binding in ASP.NET Core
//Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-10.0
//[Accessed: 6 June 2026]


namespace ERP.Controllers
{
    [Authorize(AuthenticationSchemes = "CookieAuth")]
    public class ServiceRequestController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public ServiceRequestController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // This helper method centralizes the logic for creating an
        // HttpClient instance that includes the necessary JWT token for authenticated API requests.
        private HttpClient GetSecureClient()
        {
            var client = _clientFactory.CreateClient("ErpApiClient");
            string jwtToken = HttpContext.User.FindFirst("JwtToken")?.Value ?? "";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            return client;
        }

        // GET: /ServiceRequest/Create?contractId
        public async Task<IActionResult> Create(Guid contractId)
        {
            var client = GetSecureClient();
            try
            {
                // 1. Fetch details of the specific contract to verify validity and display its metadata
                var contract = await client.GetFromJsonAsync<ERP.Models.Contract>($"api/contractsapi/details/{contractId}");

                if (contract == null)
                {
                    TempData["Error"] = "The referenced active service contract could not be resolved.";
                    return RedirectToAction("Index", "Client");
                }

                // 2. Pre-populate the request model with its corresponding contract tracking values
                var model = new ERP.Models.ServiceRequest
                {
                    ContractID = contractId,
                    Contract = contract
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Gateway connection error: {ex.Message}";
                return RedirectToAction("Index", "Client");
            }
        }

        // POST: /ServiceRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ERP.Models.ServiceRequest model, string requestType)
        {
            var client = GetSecureClient();

            var payload = new
            {
                ContractID = model.ContractID,
                Description = model.Description,
                Weight = model.Weight,
                CostUSD = model.CostUSD,
                RequestType = requestType
            };

            var response = await client.PostAsJsonAsync("api/servicerequestsapi/create", payload);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Logistics shipment service request successfully logged!";
                return RedirectToAction("Index", "Client");
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, errorMessage != "" ? errorMessage : "Gateway processing error occurred.");

            try
            {
                model.Contract = await client.GetFromJsonAsync<ERP.Models.Contract>($"api/contractsapi/details/{model.ContractID}");
            }
            catch
            {
                // Silent fallback container initialization
                model.Contract = new ERP.Models.Contract { ServiceLevel = "Active Standard" };
            }

            return View(model);
        }

        // GET: /ServiceRequest/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var client = GetSecureClient();
            try
            {
                var request = await client.GetFromJsonAsync<dynamic>($"api/servicerequestsapi/details/{id}");
                return View(request);
            }
            catch
            {
                return RedirectToAction("Index", "Client");
            }
        }
    }
}