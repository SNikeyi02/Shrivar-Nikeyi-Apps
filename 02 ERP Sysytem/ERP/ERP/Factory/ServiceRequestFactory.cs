using ERP.Enums;
using ERP.Models;

namespace ERP.Factory
{
    public abstract class ServiceRequestFactory
    {
        public abstract ServiceRequest CreateRequest();
    }
}