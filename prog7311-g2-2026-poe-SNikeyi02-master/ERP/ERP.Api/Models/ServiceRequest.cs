using ERP.Enums;
using System;
using System.Text.Json.Serialization;

namespace ERP.Models
{
    public class ServiceRequest
    {
        public Guid ServiceRequestId { get; set; }
        public Guid ContractID { get; set; }

        [JsonIgnore]
        public Contract Contract { get; set; }

        public String Description { get; set; } = String.Empty;
        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }
        public RequestStatus Status { get; set; }
        public RequestType Type { get; set; }
        public double Weight { get; set; }
    }
    //Code attribution
    //OpenAI. 2026. ChatGPT (Version 5.3). [Large language model]
    //Available at: https://chatgpt.com/share/69e8fc46-6d48-83ea-8b23-a5253cdc0cb2
    //[Accessed: 10 April 2026]
}