using ERP.Models;
using System;
using System.Threading.Tasks;

namespace ERP.Interface
{
    public interface IClientService
    {
        Task<Client> GetClientDashboardDataAsync(string username);
        Task<Client> GetClientDetailsAsync(Guid clientId);
        Task<ServiceRequest> GetServiceRequestDetailsAsync(Guid requestId);
    }
}