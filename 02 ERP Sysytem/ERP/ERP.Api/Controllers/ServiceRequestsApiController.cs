using ERP.Data;
using ERP.Models;
using ERP.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

//Code attribution
//Enum.TryParse Method
//Available at: https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable?view=net-10.0
//[Accessed: 6 June 2026]

//Code attribution
//Loading Related Data
//Available at: https://learn.microsoft.com/en-us/ef/core/querying/related-data/
//[Accessed: 6 June 2026]

namespace ERP.Api.Controllers
{
    [ApiController]
    [Route("api/servicerequestsapi")]
    [Authorize]
    public class ServiceRequestsApiController : ControllerBase
    {
        private readonly GLMSDbContext _context;

        public ServiceRequestsApiController(GLMSDbContext context)
        {
            _context = context;
        }

        // POST: api/servicerequestsapi/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateServiceRequest([FromBody] ServiceRequestPayload payload)
        {
            if (payload == null)
            {
                return BadRequest("Invalid processing exception: Payload data stream cannot be null.");
            }

            if (payload.ContractID == Guid.Empty)
            {
                return BadRequest("Validation Failed: A valid Contract ID mapping association is required.");
            }

            if (string.IsNullOrWhiteSpace(payload.Description))
            {
                return BadRequest("Validation Failed: Shipment description details cannot be blank.");
            }

            if (payload.Weight <= 0)
            {
                return BadRequest("Validation Failed: Cargo weight tracking metrics must be greater than 0 kg.");
            }

            try
            {
                var linkedContract = await _context.Contracts
                    .FirstOrDefaultAsync(c => c.ContractId == payload.ContractID);

                if (linkedContract == null)
                {
                    return NotFound($"System Error: Parent contract matching identity ID reference '{payload.ContractID}' was not found.");
                }

                // 1. Process Enum Conversion Safely from String to RequestType Enum (Road, Air, Sea)
                if (!Enum.TryParse<RequestType>(payload.RequestType, true, out var parsedType))
                {
                    // Fallback to Road if parsing fails, or return a BadRequest
                    parsedType = RequestType.Road;
                }

                // 2. Perform ZAR currency calculations automatically using a set standard market rate (e.g. 1 USD = 18.50 ZAR)
                decimal fixedExchangeRate = 18.50m;
                decimal computedZarCost = payload.CostUSD * fixedExchangeRate;

                // 3. Instantiate core ServiceRequest model
                var newRequest = new ServiceRequest
                {
                    ServiceRequestId = Guid.NewGuid(),
                    ContractID = payload.ContractID,
                    Description = payload.Description,
                    Weight = payload.Weight,
                    CostUSD = payload.CostUSD,
                    CostZAR = computedZarCost,
                    Status = RequestStatus.Pending, 
                    Type = parsedType
                };

                _context.ServiceRequests.Add(newRequest);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Logistics service request saved successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Database Transaction Exception: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // GET: api/servicerequestsapi/details/{id}
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetRequestDetails(Guid id)
        {
            var requestRecord = await _context.ServiceRequests
                .Include(r => r.Contract)
                .FirstOrDefaultAsync(r => r.ServiceRequestId == id);

            if (requestRecord == null)
            {
                return NotFound("The requested logistics service history record could not be found.");
            }

            return Ok(requestRecord);
        }
    }

    // DTO class for processing incoming service request creation payloads
    public class ServiceRequestPayload
    {
        public Guid ContractID { get; set; }
        public string Description { get; set; } = string.Empty;
        public double Weight { get; set; }
        public decimal CostUSD { get; set; }
        public string RequestType { get; set; } = string.Empty;
    }
}