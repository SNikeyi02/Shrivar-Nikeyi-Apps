using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

//Code attribution
//Role-based authorization in ASP.NET Core
//Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-10.0
//[Accessed: 5 June 2026]

//Code attribution
//Make HTTP requests with IHttpClientFactory in ASP.NET Core
//Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-10.0
//[Accessed: 4 June 2026]

namespace ERP.Controllers
{
    [Authorize(AuthenticationSchemes = "CookieAuth", Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public AdminController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // This helper method centralizes the logic for creating an HttpClient instance that includes the necessary JWT token for authenticated API requests.
        private HttpClient GetSecureClient()
        {
            var client = _clientFactory.CreateClient("ErpApiClient");
            string jwtToken = HttpContext.User.FindFirst("JwtToken")?.Value ?? "";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            return client;
        }

        // GET: /Admin/AdminDashboard
        public async Task<IActionResult> AdminDashboard()
        {
            var client = GetSecureClient();
            try
            {
                var contracts = await client.GetFromJsonAsync<List<ERP.Models.Contract>>("api/contractsapi");
                return View(contracts);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Gateway connection dropped: {ex.Message}";
                return View(new List<ERP.Models.Contract>());
            }
        }

        // GET: /Admin/SearchContracts
        [HttpGet]
        public async Task<IActionResult> SearchContracts(int? status, DateTime? startDate, DateTime? endDate)
        {
            var client = GetSecureClient();
            try
            {
                // query parameter list
                var queryParams = new List<string>();

                if (status.HasValue && status.Value != -1)
                {
                    queryParams.Add($"status={status.Value}");
                }

                if (startDate.HasValue)
                {
                    queryParams.Add($"startDate={startDate.Value.ToString("yyyy-MM-dd")}");
                }

                if (endDate.HasValue)
                {
                    queryParams.Add($"endDate={endDate.Value.ToString("yyyy-MM-dd")}");
                }

                // Join parameters using standard URL separator syntax
                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                string apiRoute = $"api/contractsapi{queryString}";

                // Fetch the data list
                var filteredContracts = await client.GetFromJsonAsync<List<ERP.Models.Contract>>(apiRoute);

                // Pass the hydrated list back into the main AdminDashboard UI layout view
                return View("AdminDashboard", filteredContracts ?? new List<ERP.Models.Contract>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Filter execution failed: {ex.Message}";
                return RedirectToAction(nameof(AdminDashboard));
            }
        }

        // GET: Admin/ViewContract/{id}
        public async Task<IActionResult> ViewContract(Guid id)
        {
            // Fetch an authenticated HttpClient from the built-in IHttpClientFactory system
            var client = GetSecureClient();
            try
            {
                // Make an async GET request to your specific backend endpoint route details mapping
                var contract = await client.GetFromJsonAsync<ERP.Models.Contract>($"api/contractsapi/details/{id}");

                if (contract == null)
                {
                    return NotFound("The specified contract ledger entry does not exist.");
                }

                return View(contract);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to fetch contract profile data stream: {ex.Message}";
                return RedirectToAction(nameof(AdminDashboard));
            }
        }

        // GET: /Admin/ViewPDF/{id}
        [HttpGet]
        public async Task<IActionResult> ViewPDF(Guid id)
        {
            var client = GetSecureClient();
            try
            {
                var contractDetails = await client.GetFromJsonAsync<ERP.Models.Contract>($"api/contractsapi/details/{id}");
                if (contractDetails == null || string.IsNullOrEmpty(contractDetails.AgreementFilePath))
                {
                    return NotFound("PDF file path data reference is missing.");
                }

                string cleanPath = contractDetails.AgreementFilePath.TrimStart('/');
                string baseAddress = client.BaseAddress?.ToString() ?? "https://localhost:7239/";
                string absoluteFileUrl = $"{baseAddress.TrimEnd('/')}/{cleanPath}";

                var fileResponse = await client.GetAsync(absoluteFileUrl);
                if (!fileResponse.IsSuccessStatusCode)
                {
                    return NotFound("The physical PDF file could not be found inside the API server storage.");
                }

                byte[] fileBytes = await fileResponse.Content.ReadAsByteArrayAsync();
                return File(fileBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"File retrieval gateway error: {ex.Message}");
            }
        }

        // POST: /Admin/ChangeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(Guid id, int status)
        {
            var client = GetSecureClient();

            // Build payload matching StatusUpdatePayload class on the API layer
            var payload = new { NewStatus = status };

            // Send request over the structural tokenized PATCH route pattern
            var response = await client.PatchAsJsonAsync($"api/contractsapi/contracts/{id}/status", payload);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Status patched successfully via API channel pipeline!";
            }
            else
            {
                TempData["Error"] = $"Failed to patch status. Status code: {response.StatusCode}";
            }

            return RedirectToAction(nameof(ViewContract), new { id = id });
        }

        // GET: /Admin/CreateClient
        [HttpGet]
        public IActionResult CreateClient() => View();

        // POST: /Admin/CreateClient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClient(IFormCollection form)
        {
            string email = form["Email"]!;
            string companyName = form["Name"]!;
            string region = form["Region"]!;
            string description = form["Description"]!;
            string password = form["Password"]!;

            if (string.IsNullOrWhiteSpace(description))
            {
                ModelState.AddModelError("", "The Description field is required.");
                return View();
            }

            var client = GetSecureClient();

            var identityPayload = new { Email = email, Name = companyName, Password = password };
            var identityResponse = await client.PostAsJsonAsync("api/authapi/register", identityPayload);
            if (!identityResponse.IsSuccessStatusCode)
            {
                var registerError = await identityResponse.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Security Account Creation Failed: {registerError}");
                return View();
            }

            var clientDomainPayload = new ERP.Api.Models.ClientCreateDto
            {
                Name = companyName,
                Email = email,
                Description = description,
                Region = region
            };

            var clientDbResponse = await client.PostAsJsonAsync("api/clientapi/create", clientDomainPayload);
            if (clientDbResponse.IsSuccessStatusCode)
            {
                TempData["Success"] = "Client profile and access account provisioned successfully!";
                return RedirectToAction("AdminDashboard");
            }

            var dbError = await clientDbResponse.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Database profile generation failed: {dbError}");
            return View();
        }
    }
}