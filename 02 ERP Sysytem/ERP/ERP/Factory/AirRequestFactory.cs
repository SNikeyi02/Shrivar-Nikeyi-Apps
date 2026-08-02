using ERP.Models;
using ERP.Enums;

namespace ERP.Factory
{
    public class AirRequestFactory : ServiceRequestFactory
    {
        public override ServiceRequest CreateRequest()
        {
            // Create and return a new ServiceRequest object with the Type set to Air
            return new ServiceRequest { Type = RequestType.Air };
        }
    }
}