using ERP.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ERP.Interface
{
    public interface IContractService
    {
        Task<Contract> CreateContractAsync(Contract contract, IFormFile file, string username);
    }
}