using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

//Code attribution
//Uri.EscapeDataString Method
//Available at: https://learn.microsoft.com/en-us/dotnet/api/system.uri.escapedatastring?view=net-10.0
//[Accessed: 3 June 2026]

//Code attribution
//HTTP authentication
//Available at: https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/Authentication
//[Accessed: 4 June 2026]

namespace ERP.Controllers
{
    [Authorize(AuthenticationSchemes = "CookieAuth", Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public ClientController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private HttpClient GetSecureClient()
        {
            var client = _clientFactory.CreateClient("ErpApiClient");

            // Retrieve the verified JWT security token string out of the current cookie identity claims bundle
            string jwtToken = HttpContext.User.FindFirst("JwtToken")?.Value ?? "";

            // Inject the authorization bearer string down into the outgoing request stream pipeline
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            return client;
        }

        // GET: /Client
        public async Task<IActionResult> Index()
        {
            var client = GetSecureClient();
            var username = User.Identity?.Name;

            try
            {
                var clientData = await client.GetFromJsonAsync<ERP.Models.Client>($"api/clientapi/dashboard?username={Uri.EscapeDataString(username ?? "")}");

                // if API returned null, initialize an empty object to avoid a view crash
                if (clientData == null)
                {
                    clientData = new ERP.Models.Client { Name = username ?? "Valued Client", Contracts = new List<ERP.Models.Contract>() };
                }

                return View(clientData);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unable to stream client telemetry over gateway: {ex.Message}";

                var fallbackModel = new ERP.Models.Client
                {
                    Name = username ?? "Valued Client",
                    Contracts = new List<ERP.Models.Contract>() // Clear list ensures Model.Contracts isn't null
                };

                return View(fallbackModel);
            }
        }

        // GET: /Client/ClientDetails/{id}
        public async Task<IActionResult> ClientDetails(Guid id)
        {
            var client = GetSecureClient();
            try
            {
                var details = await client.GetFromJsonAsync<dynamic>($"api/clientapi/details/{id}");
                return View(details);
            }
            catch
            {
                TempData["Error"] = "Error reaching backend service node.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Client/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var client = GetSecureClient();
            try
            {
                var requestDetails = await client.GetFromJsonAsync<dynamic>($"api/clientapi/requests/{id}");
                return View(requestDetails);
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult CreateContract() => RedirectToAction("Create", "Contract");
    }
}