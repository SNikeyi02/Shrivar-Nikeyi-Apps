using ERP.Api.Models;
using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

//Code attribution
//Model Validation in ASP.NET Web API
//Available at: https://learn.microsoft.com/en-us/aspnet/web-api/overview/formats-and-model-binding/model-validation-in-aspnet-web-api
//[Accessed: 6 June 2026]

//Code attribution
//Asynchronous Programming
//Available at: https://learn.microsoft.com/en-us/ef/core/miscellaneous/async
//[Accessed: 6 June 2026]

namespace ERP.Api.Controllers
{
    // This controller is designed to handle client-specific API requests, such as creating a new client profile or fetching client dashboard data.
    [ApiController]
    [Route("api/[controller]")]
    public class ClientApiController : ControllerBase
    {
        private readonly GLMSDbContext _context;

        public ClientApiController(GLMSDbContext context)
        {
            _context = context;
        }

        // POST: api/clientapi/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateClient([FromBody] ClientCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest("The Web API received a null or un-parsable DTO payload.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Model state validation failed on incoming payload.");
            }

            try
            {
                // Map the pure DTO values directly over to your main database domain tracking entity
                var newClient = new Client
                {
                    ClientID = Guid.NewGuid(),
                    Name = dto.Name,
                    Email = dto.Email,
                    Description = dto.Description,
                    Region = dto.Region,
                    Contracts = new List<Contract>() // Initializes a safe, empty navigation tracking collection
                };

                _context.Clients.Add(newClient);
                await _context.SaveChangesAsync();

                // Return a clean confirmation object back across the network wire
                return Ok(new { Success = true, Message = "Client database profile inserted seamlessly." });
            }
            catch (Exception ex)
            {
                // Catches internal database errors and returns the text
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData([FromQuery] string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Username identifier parameter is missing.");
            }

            var clientProfile = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.Email == username);

            if (clientProfile == null)
            {
                return NotFound("Client identity framework configuration profile not found.");
            }

            return Ok(clientProfile);
        }
    }
}