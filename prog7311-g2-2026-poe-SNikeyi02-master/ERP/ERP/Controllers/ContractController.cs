using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

//Code attribution
//Upload files in ASP.NET Core
//Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-10.0
//[Accessed: 3 June 2026]

//Code attribution
//Policy-based authorization in ASP.NET Core
//Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-10.0
//[Accessed: 2 June 2026]

namespace ERP.Controllers
{
    [Authorize(AuthenticationSchemes = "CookieAuth")]
    public class ContractController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public ContractController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // This helper method centralizes the logic for creating an HttpClient instance
        // that includes the necessary JWT token for authenticated API requests.
        private HttpClient GetSecureClient()
        {
            var client = _clientFactory.CreateClient("ErpApiClient");
            string jwtToken = HttpContext.User.FindFirst("JwtToken")?.Value ?? "";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            return client;
        }

        [Authorize(Roles = "Admin")]

        // GET: /Contract/Index
        public async Task<IActionResult> Index()
        {
            var client = GetSecureClient();
            try
            {
                var contracts = await client.GetFromJsonAsync<dynamic>("api/contractsapi");
                return View(contracts);
            }
            catch
            {
                return View(new System.Collections.Generic.List<dynamic>());
            }
        }

        [Authorize(Roles = "Admin,Client")]
        public IActionResult Create() => View();

        // POST: /Contract/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Client")]
        public async Task<IActionResult> Create(ERP.Models.Contract contract, IFormFile AgreementFile)
        {
            if (AgreementFile == null || AgreementFile.Length == 0)
            {
                ModelState.AddModelError("", "Please attach a valid corporate agreement file document.");
                return View(contract);
            }

            if (contract.StartDate == DateTime.MinValue) contract.StartDate = DateTime.Now;
            if (contract.EndDate == DateTime.MinValue) contract.EndDate = DateTime.Now.AddYears(1);

            var client = GetSecureClient();

            using var content = new MultipartFormDataContent();

            // 1. Pack text parameters safely inside the multi-part data payload
            content.Add(new StringContent(contract.ServiceLevel ?? "Standard"), "ServiceLevel");
            content.Add(new StringContent(User.Identity?.Name ?? ""), "Username");
            content.Add(new StringContent(((int)(contract.Status)).ToString()), "Status");

            // 2. Explicitly forward dates to Web API Backend
            content.Add(new StringContent(contract.StartDate.ToString("yyyy-MM-ddTHH:mm:ss")), "StartDate");
            content.Add(new StringContent(contract.EndDate.ToString("yyyy-MM-ddTHH:mm:ss")), "EndDate");

            // 3. Pack stream block for file handling with its true original filename extension
            var fileStreamContent = new StreamContent(AgreementFile.OpenReadStream());
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(AgreementFile.ContentType);

            // Explicitly retain the exact original filename string (e.g., "my_contract.pdf")
            content.Add(fileStreamContent, "AgreementFile", AgreementFile.FileName);

            var response = await client.PostAsync("api/contractsapi/create", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Contract and signed PDF processed successfully!";
                return RedirectToAction("Index", "Client");
            }

            // Capture and surface backend database model execution faults directly to the UI
            var errorMsg = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", !string.IsNullOrWhiteSpace(errorMsg) ? errorMsg : "API Gateway processing fault.");
            return View(contract);
        }

        [Authorize(Roles = "Admin,Client")]

        // GET: /Contract/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var client = GetSecureClient();
            try
            {
                var contractDetails = await client.GetFromJsonAsync<ERP.Models.Contract>($"api/contractsapi/details/{id}");

                if (contractDetails == null)
                {
                    TempData["Error"] = "Contract data could not be parsed.";
                    return RedirectToAction("Index", "Client");
                }

                return View(contractDetails);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading contract status: {ex.Message}";
                return RedirectToAction("Index", "Client");
            }
        }

        // POST: /Contract/UploadPDF/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Client")]
        public async Task<IActionResult> UploadPDF(Guid contractId, IFormFile AgreementFile)
        {
            if (AgreementFile == null || AgreementFile.Length == 0)
            {
                ModelState.AddModelError("", "Please select a valid PDF file before submitting.");
                return RedirectToAction("Index", "Client");
            }

            var client = GetSecureClient();
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent("Active"), "ServiceLevel");
            content.Add(new StringContent(User.Identity?.Name ?? ""), "Username");

            var fileStreamContent = new StreamContent(AgreementFile.OpenReadStream());
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(AgreementFile.ContentType);
            content.Add(fileStreamContent, "AgreementFile", AgreementFile.FileName);

            var response = await client.PostAsync("api/contractsapi/create", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Signed document uploaded successfully!";
                return RedirectToAction("Index", "Client");
            }

            var errorMsg = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"Upload rejected: {errorMsg}";
            return RedirectToAction("Index", "Client");
        }

        // GET: /Contract/DownloadAgreement/
        [Authorize(Roles = "Admin,Client")]
        public async Task<IActionResult> DownloadAgreement(Guid? id, Guid? contractId)
        {
            Guid targetContractId = id ?? contractId ?? Guid.Empty;

            if (targetContractId == Guid.Empty)
            {
                TempData["Error"] = "No valid contract identifier was supplied.";
                return RedirectToAction("Index", "Client");
            }

            var client = GetSecureClient();

            try
            {
                // 1. Fetch contract details from the Web API
                var contractDetails = await client.GetFromJsonAsync<ERP.Models.Contract>($"api/contractsapi/details/{targetContractId}");

                if (contractDetails == null || string.IsNullOrWhiteSpace(contractDetails.AgreementFilePath))
                {
                    TempData["Error"] = "This contract database record does not contain a valid PDF document path.";
                    return RedirectToAction("Index", "Client");
                }

                // 2. Normalize path strings safely
                string cleanPath = contractDetails.AgreementFilePath;
                if (cleanPath.StartsWith("/")) cleanPath = cleanPath.Substring(1);
                if (cleanPath.StartsWith("wwwroot/")) cleanPath = cleanPath.Replace("wwwroot/", "");

                string apiBaseAddress = client.BaseAddress?.ToString() ?? "https://localhost:7239/";
                string absoluteFileUrl = $"{apiBaseAddress.TrimEnd('/')}/{cleanPath}";

                // 3. Request the physical file stream from the Web API project
                var fileResponse = await client.GetAsync(absoluteFileUrl);

                if (!fileResponse.IsSuccessStatusCode)
                {
                    // Capture and surface backend file system faults directly to the UI
                    var apiErrorText = await fileResponse.Content.ReadAsStringAsync();
                    TempData["Error"] = $"File System Error: The backend could not find the physical file on disk. ({fileResponse.StatusCode})";
                    return RedirectToAction("Index", "Client");
                }

                // 4. Read the file binary array data bytes safely
                byte[] fileBytes = await fileResponse.Content.ReadAsByteArrayAsync();
                string cleanDownloadName = Path.GetFileName(contractDetails.AgreementFilePath);

                if (!cleanDownloadName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    cleanDownloadName += ".pdf";
                }

                // 5. Forces a clean, valid binary PDF download context block
                return File(fileBytes, "application/pdf", cleanDownloadName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Download system gate error: {ex.Message}";
                return RedirectToAction("Index", "Client");
            }
        }
    }
}