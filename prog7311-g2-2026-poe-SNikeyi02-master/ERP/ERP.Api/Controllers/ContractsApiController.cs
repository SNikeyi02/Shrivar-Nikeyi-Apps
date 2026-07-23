using ERP.Data;
using ERP.Models;
using ERP.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

//Code attribution
//IQueryable Interface
//Available at: https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable?view=net-10.0
//[Accessed: 6 June 2026]

namespace ERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContractsApiController : ControllerBase
    {
        private readonly GLMSDbContext _context;
        private readonly IWebHostEnvironment _env;

        // Dependency Injection of the database context and hosting environment for file handling
        public ContractsApiController(GLMSDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/contractsapi
        [HttpGet]
        public async Task<IActionResult> GetAllContracts([FromQuery] int? status, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var query = _context.Contracts.Include(c => c.Client).AsQueryable();

                if (status.HasValue)
                {
                    query = query.Where(c => (int)c.Status == status.Value);
                }
                if (startDate.HasValue)
                {
                    query = query.Where(c => c.StartDate >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    query = query.Where(c => c.EndDate <= endDate.Value);
                }

                var contracts = await query.ToListAsync();
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Database Error: {ex.Message}");
            }
        }

        // POST: api/contractsapi/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateContract([FromForm] IFormCollection form, IFormFile AgreementFile)
        {
            try
            {
                string serviceLevelStr = form["ServiceLevel"]!;
                string username = form["Username"]!;
                string startDateStr = form["StartDate"]!;
                string endDateStr = form["EndDate"]!;
                string statusStr = form["Status"]!;

                if (AgreementFile == null || AgreementFile.Length == 0)
                {
                    return BadRequest("Only PDF files are allowed. No document was detected.");
                }

                if (!Path.GetExtension(AgreementFile.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Validation Failed: Only PDF files are allowed.");
                }

                var clientRecord = await _context.Clients.FirstOrDefaultAsync(c => c.Email == username);
                if (clientRecord == null)
                {
                    return BadRequest($"System Error: Client profile matching account email '{username}' was not found.");
                }

                ContractStatus contractStatus = ContractStatus.Active;
                if (!string.IsNullOrEmpty(statusStr) && int.TryParse(statusStr, out int statusInt))
                {
                    contractStatus = (ContractStatus)statusInt;
                }

                string baseWwwRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string uploadsFolder = Path.Combine(baseWwwRoot, "uploads", "agreements");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(AgreementFile.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await AgreementFile.CopyToAsync(fileStream);
                }

                DateTime parsedStartDate = DateTime.Now;
                DateTime parsedEndDate = DateTime.Now.AddYears(1);

                if (!string.IsNullOrEmpty(startDateStr))
                {
                    DateTime.TryParse(startDateStr, out parsedStartDate);
                }
                if (!string.IsNullOrEmpty(endDateStr))
                {
                    DateTime.TryParse(endDateStr, out parsedEndDate);
                }

                var newContract = new Contract
                {
                    ContractId = Guid.NewGuid(),
                    ClientId = clientRecord.ClientID,
                    ServiceLevel = !string.IsNullOrWhiteSpace(serviceLevelStr) ? serviceLevelStr : "Standard",
                    Status = ContractStatus.Active,
                    StartDate = parsedStartDate,
                    EndDate = parsedEndDate,
                    AgreementFilePath = $"uploads/agreements/{uniqueFileName}"
                };

                _context.Contracts.Add(newContract);
                await _context.SaveChangesAsync();

                return Ok(newContract);
            }
            catch (Exception ex)
            {
                return BadRequest($"Database Transaction Exception: {ex.Message}");
            }
        }

        // GET: api/contractsapi/details/{id}
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetContractDetails(Guid id)
        {
            var contract = await _context.Contracts
                                         .Include(c => c.Client)
                                         .Include(c => c.ServiceRequests)
                                         .FirstOrDefaultAsync(c => c.ContractId == id);

            if (contract == null) return NotFound("Contract tracking node not found.");
            return Ok(contract);
        }

        // api/contractsapi/{id}/status
        [HttpPatch("contracts/{id}/status")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] StatusUpdatePayload dto)
        {
            if (dto == null) return BadRequest("Malformed processing payload parameters.");

            var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.ContractId == id);
            if (contract == null) return NotFound("Target contract record sequence not found.");

            // Enforce the new status assignment safely
            contract.Status = (ContractStatus)dto.NewStatus;
            _context.Entry(contract).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Contract status changed successfully over PATCH route." });
        }

        public class StatusUpdatePayload
        {
            public int NewStatus { get; set; }
        }
    }

    // DTO class for status update payload
    public class StatusUpdateDto
    {
        public Guid ContractId { get; set; }
        public int NewStatus { get; set; }
    }
}