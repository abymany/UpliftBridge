namespace UpliftBridge.Models
{
    public class Recipient
    {
        public int Id { get; set; }

        public string Country { get; set; } = "";
        public string Region { get; set; } = "";
        public string Category { get; set; } = "";

        public bool IsActive { get; set; } = true;
    }
}
