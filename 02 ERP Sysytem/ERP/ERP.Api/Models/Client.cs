namespace ERP.Models
{
    public class Client
    {
        public Guid ClientID { get; set; }
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public String Name { get; set; }
        public String Email { get; set; } = String.Empty;
        public String Description { get; set; }
        public String Region { get; set; }
    }
    //Code attribution
    //OpenAI. 2026. ChatGPT (Version 5.3). [Large language model]
    //Available at: https://chatgpt.com/share/69e8fc46-6d48-83ea-8b23-a5253cdc0cb2
    //[Accessed: 10 April 2026]
}
