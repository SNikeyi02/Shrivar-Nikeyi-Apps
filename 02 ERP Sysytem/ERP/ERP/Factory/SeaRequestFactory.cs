using ERP.Models;
using ERP.Enums;

namespace ERP.Factory
{
    public class SeaRequestFactory : ServiceRequestFactory
    {
        public override ServiceRequest CreateRequest()
        {
            // Create and return a new ServiceRequest object with the Type set to Sea
            return new ServiceRequest { Type = RequestType.Sea };
        }
    }
}