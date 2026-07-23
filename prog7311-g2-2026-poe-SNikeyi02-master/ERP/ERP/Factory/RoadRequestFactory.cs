using ERP.Models;
using ERP.Enums;

namespace ERP.Factory
{
    public class RoadRequestFactory : ServiceRequestFactory
    {
        public override ServiceRequest CreateRequest()
        {
            // For Road Requests, we can set some default values if needed
            return new ServiceRequest { Type = RequestType.Road };
        }
    }
}