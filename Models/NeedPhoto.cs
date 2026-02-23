using System;
using System.ComponentModel.DataAnnotations;

namespace UpliftBridge.Models
{
    public class NeedPhoto
    {
        public int Id { get; set; }

        [Required]
        public int NeedId { get; set; }
        public Need? Need { get; set; }

        [Required, MaxLength(400)]
        public string Path { get; set; } = ""; // "/uploads/needs/3/abc.jpg"

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}