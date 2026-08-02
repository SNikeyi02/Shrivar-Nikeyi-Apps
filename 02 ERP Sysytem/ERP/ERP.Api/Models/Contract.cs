using ERP.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ERP.Models
{
    public class Contract
    {
        public Guid ContractId { get; set; }

        public Guid ClientId { get; set; }

        [JsonIgnore]
        public Client? Client { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; }
        public ContractStatus Status { get; set; }
        public String ServiceLevel { get; set; } = "Basic";
        public string? AgreementFilePath { get; set; }
        public List<ServiceRequest>? ServiceRequests { get; set; }
    }
    //Code attribution
    //OpenAI. 2026. ChatGPT (Version 5.3). [Large language model]
    //Available at: https://chatgpt.com/share/69e8fc46-6d48-83ea-8b23-a5253cdc0cb2
    //[Accessed: 10 April 2026]
}