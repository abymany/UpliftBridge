using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpliftBridge.Models
{
    public enum PledgeStatus
    {
        PendingReview = 0,
        ApprovedToCollect = 1,
        Declined = 2,
        Cancelled = 3
    }

    public class Pledge
    {
        public int Id { get; set; }

        [Required]
        public int NeedId { get; set; }

        // Optional donor identity (anonymous supported)
        [MaxLength(120)]
        public string DonorName { get; set; } = "";

        [MaxLength(160)]
        public string DonorEmail { get; set; } = "";

        [Range(0.50, 1000000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PlatformFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public PledgeStatus Status { get; set; } = PledgeStatus.PendingReview;

        [MaxLength(600)]
        public string AdminNote { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional: snapshot the Need title for admin list stability
        [MaxLength(140)]
        public string NeedTitleSnapshot { get; set; } = "";
    }
}

