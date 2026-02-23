using System;
using System.ComponentModel.DataAnnotations;

namespace UpliftBridge.Models
{
    public class NeedUpdate
    {
        public int Id { get; set; }

        [Required]
        public int NeedId { get; set; }
        public Need? Need { get; set; }

        [Required, MaxLength(140)]
        public string Title { get; set; } = "";

        [Required, MaxLength(3000)]
        public string Message { get; set; } = "";

        // If you want later: photo upload, donor mention, etc.
        [MaxLength(120)]
        public string? PublicName { get; set; } // "A grateful mom", etc.

        public bool IsThankYou { get; set; } = true;

        // Moderation
        public bool IsVisible { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}