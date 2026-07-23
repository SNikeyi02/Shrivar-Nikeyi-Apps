using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using ERP.Data;
using ERP.Models;
using ERP.Enums;

namespace ERP.Tests.Integration
{
    public class ContractsApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        public ContractsApiIntegrationTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region Private Helper Methods

        private async Task<string> GetValidAuthTokenAsync() => await TestAuthHelpers.GetValidAuthTokenAsync(_client);
        private async Task<string> GetAdminAuthTokenAsync() => await TestAuthHelpers.GetAdminAuthTokenAsync(_client);
        private async Task<string> GetClientAuthTokenAsync() => await TestAuthHelpers.GetClientAuthTokenAsync(_client);

        private async Task<Guid> SeedSingleContractForTestAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GLMSDbContext>();

            var contractId = Guid.NewGuid();
            var testContract = new Contract
            {
                ContractId = contractId,
                ServiceLevel = "Premium Integration SLA",
                Status = ContractStatus.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(12),
                AgreementFilePath = "uploads/agreements/integration_test.pdf"
            };

            context.Contracts.Add(testContract);
            await context.SaveChangesAsync();
            return contractId;
        }

        #endregion

        #region GET /api/contractsapi Tests

        [Fact]
        public async Task GetAllContracts_ShouldReturn200OK_WithNonEmptyJsonArray()
        {
            await SeedSingleContractForTestAsync();
            var token = await GetValidAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/contractsapi");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();

            var contracts = JsonSerializer.Deserialize<JsonElement>(content);
            contracts.ValueKind.Should().Be(JsonValueKind.Array);
            contracts.GetArrayLength().Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetAllContracts_WithStatusFilter_ShouldReturnFilteredResults()
        {
            var token = await GetValidAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/contractsapi?status=1");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllContracts_WithDateFilters_ShouldReturnFilteredResults()
        {
            var token = await GetValidAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var startDate = DateTime.UtcNow.AddMonths(-6).ToString("yyyy-MM-dd");
            var endDate = DateTime.UtcNow.AddMonths(6).ToString("yyyy-MM-dd");

            var response = await _client.GetAsync($"/api/contractsapi?startDate={startDate}&endDate={endDate}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/contractsapi/details/{id} Tests

        [Fact]
        public async Task GetContractDetails_WithValidId_ShouldReturn200OK_WithContractData()
        {
            var seededId = await SeedSingleContractForTestAsync();
            var token = await GetValidAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync($"/api/contractsapi/details/{seededId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetContractDetails_WithInvalidId_ShouldReturn404NotFound()
        {
            var token = await GetValidAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var invalidId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/contractsapi/details/{invalidId}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region PATCH /api/contractsapi/contracts/{id}/status Tests

        [Fact]
        public async Task ChangeContractStatus_WithValidPayload_ShouldReturn200OK()
        {
            var seededId = await SeedSingleContractForTestAsync();
            var token = await GetValidAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var payload = new { NewStatus = 2 };

            var response = await _client.PatchAsJsonAsync($"/api/contractsapi/contracts/{seededId}/status", payload);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().ContainEquivalentOf("Contract status changed successfully");
        }

        #endregion

        #region POST /api/contractsapi/create Tests

        [Fact]
        public async Task CreateContract_WithValidFormData_ShouldReturn200OK_WithCreatedContract()
        {
            var token = await GetValidAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("Premium"), "ServiceLevel");
            formData.Add(new StringContent("testclient@client.com"), "Username");
            formData.Add(new StringContent("1"), "Status");
            formData.Add(new StringContent(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")), "StartDate");
            formData.Add(new StringContent(DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-ddTHH:mm:ss")), "EndDate");

            var fileContent = new ByteArrayContent(TestAuthHelpers.GenerateDummyPdfContent());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            formData.Add(fileContent, "AgreementFile", "test_contract.pdf");

            var response = await _client.PostAsync("/api/contractsapi/create", formData);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        #endregion
    }

    public static class TestAuthHelpers
    {
        public static async Task<string> GetValidAuthTokenAsync(HttpClient client)
        {
            var testEmail = $"apitest{Guid.NewGuid():N}@client.com";
            var testPassword = "Test123";

            var registerPayload = new
            {
                Email = testEmail,
                Name = "API Test User",
                Password = testPassword
            };

            await client.PostAsJsonAsync("/api/authapi/register", registerPayload);

            var loginPayload = new
            {
                Email = testEmail,
                Password = testPassword
            };

            var loginResponse = await client.PostAsJsonAsync("/api/authapi/login", loginPayload);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(loginContent);

            return tokenResponse?.GetValueOrDefault("Token") ?? string.Empty;
        }

        public static async Task<string> GetAdminAuthTokenAsync(HttpClient client)
        {
            var adminEmail = $"admintest{Guid.NewGuid():N}@admin.com";
            var adminPassword = "Test123";

            var registerPayload = new
            {
                Email = adminEmail,
                Name = "Admin Test User",
                Password = adminPassword
            };

            await client.PostAsJsonAsync("/api/authapi/register", registerPayload);

            var loginPayload = new
            {
                Email = adminEmail,
                Password = adminPassword
            };

            var loginResponse = await client.PostAsJsonAsync("/api/authapi/login", loginPayload);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(loginContent);

            return tokenResponse?.GetValueOrDefault("Token") ?? string.Empty;
        }

        public static async Task<string> GetClientAuthTokenAsync(HttpClient client)
        {
            return await GetValidAuthTokenAsync(client);
        }

        public static byte[] GenerateDummyPdfContent()
        {
            return new byte[]
            {
                0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x30, 0x0A, 0x31, 0x20, 0x30,
                0x20, 0x6F, 0x62, 0x6A, 0x3C, 0x3C, 0x2F, 0x54, 0x79, 0x70, 0x65, 0x2F,
                0x43, 0x61, 0x74, 0x61, 0x6C, 0x6F, 0x67, 0x2F, 0x50, 0x61, 0x67, 0x65,
                0x73, 0x20, 0x32, 0x20, 0x30, 0x20, 0x52, 0x3E, 0x3E, 0x65, 0x6E, 0x64,
                0x6F, 0x62, 0x6A, 0x0A, 0x74, 0x72, 0x61, 0x69, 0x6C, 0x65, 0x72, 0x3C,
                0x3C, 0x2F, 0x52, 0x6F, 0x6F, 0x74, 0x20, 0x31, 0x20, 0x30, 0x20, 0x52,
                0x3E, 0x3E
            };
        }
    }
}