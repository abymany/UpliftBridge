using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpliftBridge.Models
{
    public class NeedPhoto
    {
        public int Id { get; set; }

        [Required]
        public int NeedId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Path { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(NeedId))]
        public Need? Need { get; set; }
    }
}