using ERP.Data;
using ERP.Models;
using ERP.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

//Code attribution
//Best practices for comparing strings in .NET
//Available at: https://learn.microsoft.com/en-us/dotnet/standard/base-types/best-practices-strings
//[Accessed: 1 June 2026]

namespace ERP.Services
{
    public class ClientService : IClientService
    {
        private readonly GLMSDbContext _context;

        public ClientService(GLMSDbContext context)
        {
            _context = context;
        }

        // This method retrieves the client dashboard data for a given username, including their contracts and associated service requests.
        public async Task<Client> GetClientDashboardDataAsync(string username)
        {
            return await _context.Clients
                .Include(c => c.Contracts)
                    .ThenInclude(con => con.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Name.ToLower() == username.ToLower())
                ?? throw new KeyNotFoundException($"Client record for '{username}' not found.");
        }

        // This method retrieves detailed information about a specific client based on their unique identifier, including their contracts.
        public async Task<Client> GetClientDetailsAsync(Guid clientId)
        {
            return await _context.Clients
                .Include(c => c.Contracts)
                    .ThenInclude(con => con.ServiceRequests)
                .FirstOrDefaultAsync(c => c.ClientID == clientId)
                ?? throw new KeyNotFoundException("Client profile not found.");
        }

        // This method retrieves detailed information about a specific service request based on its unique identifier, including the associated contract details.
        public async Task<ServiceRequest> GetServiceRequestDetailsAsync(Guid requestId)
        {
            return await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(s => s.ServiceRequestId == requestId)
                ?? throw new KeyNotFoundException("Service Request details not found.");
        }
    }
}