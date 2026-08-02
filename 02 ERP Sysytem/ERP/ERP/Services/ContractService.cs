using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using ERP.Data;
using ERP.Models;
using ERP.Enums;
using ERP.Builders;
using ERP.Interface;

//Code attribution
//IWebHostEnvironment Interface
//Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.hosting.iwebhostenvironment?view=aspnetcore-10.0
//[Accessed: 1 June 2026]

//Code attribution
//File and Stream I/O
//Available at: https://learn.microsoft.com/en-us/dotnet/standard/io/
//[Accessed: 1 June 2026]

namespace ERP.Services
{
    public class ContractService : IContractService
    {
        private readonly GLMSDbContext _context;
        private readonly string _storageFolder;

        public ContractService(GLMSDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _storageFolder = Path.Combine(env.WebRootPath ?? Path.GetTempPath(), "contracts");
        }

        // This method encapsulates the entire contract creation workflow,
        // including business logic, file handling, and design pattern implementation.
        public async Task<Contract> CreateContractAsync(Contract contract, IFormFile file, string username)
        {
            // 1. Business Logic: Identify Client Profile
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Name.ToLower() == username.ToLower())
                ?? throw new UnauthorizedAccessException("Client profile mapping failed.");

            // 2. Business Logic: PDF File Validation
            if (file == null || file.Length == 0)
                throw new ArgumentException("Please upload a PDF agreement.");

            if (Path.GetExtension(file.FileName).ToLowerInvariant() != ".pdf")
                throw new BadHttpRequestException("Only PDF files are allowed.");

            // 3. File System I/O Logic
            if (!Directory.Exists(_storageFolder))
            {
                Directory.CreateDirectory(_storageFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var path = Path.Combine(_storageFolder, uniqueFileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 4. DESIGN PATTERN IMPLEMENTATION: Dynamic Builder Pattern 
            var slaBuilder = new StandardSLABuilder();

            // Pass actual form and context values directly into the pattern operations!
            slaBuilder.BuildClientInfo(client.Description ?? client.Name);

            // Safely pull the type description if a request container exists, else label as Standard
            string initialMethod = contract.ServiceRequests?.FirstOrDefault()?.Type.ToString() ?? "Standard Road";
            slaBuilder.BuildShippingTerms(initialMethod);

            // Capture baseline amount details from form payload properties
            decimal? baseCost = contract.ServiceRequests?.FirstOrDefault()?.CostUSD;
            slaBuilder.BuildCurrencyRules(baseCost);

            var standardSla = slaBuilder.GetResult();

            // 5. Build Final Domain Entity Data
            contract.ContractId = Guid.NewGuid();
            contract.Status = ContractStatus.Active;
            contract.Client = client;
            contract.AgreementFilePath = uniqueFileName;
            contract.ServiceLevel = standardSla.Details;

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            return contract;
        }
    }
}