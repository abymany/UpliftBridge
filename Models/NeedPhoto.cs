using System;

namespace UpliftBridge.Models
{
    public class NeedPhoto
    {
        public int Id { get; set; }

        public int NeedId { get; set; }

        public string Path { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public Need? Need { get; set; }
    }
}