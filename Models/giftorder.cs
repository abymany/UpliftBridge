using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpliftBridge.Models
{
    public enum PaymentStatus
    {
        Demo = 0,
        Pending = 1,
        Paid = 2,
        Failed = 3,
        Refunded = 4
    }

    // ✅ rename to avoid collision with PledgeStatus enum in Pledge.cs
    public enum OffsiteGiftStatus
    {
        Unconfirmed = 0,   // donor has not proven offsite payment yet
        Confirmed = 1,     // admin verified offsite payment
        Rejected = 2       // invalid / duplicate / scam
    }

    public class GiftOrder
    {
        public int Id { get; set; }

        // Link to Need
        public int NeedId { get; set; }

        [Required, MaxLength(200)]
        public string NeedTitle { get; set; } = string.Empty;

        // Optional donor info
        [MaxLength(120)]
        public string? DonorName { get; set; }

        [MaxLength(160)]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string? DonorEmail { get; set; }

        // ✅ What donor intends to pay OFFSITE (not confirmed)
        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0.50", "1000000", ErrorMessage = "Gift must be at least $0.50.")]
        public decimal PledgedGiftAmount { get; set; }

        // ✅ What donor paid on UpliftBridge via Stripe (platform support)
        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0.00", "1000000", ErrorMessage = "Platform support cannot be negative.")]
        public decimal PlatformSupportPaid { get; set; }

        // Stripe tracking
        [MaxLength(200)]
        public string StripeSessionId { get; set; } = "";

        [MaxLength(200)]
        public string StripePaymentIntentId { get; set; } = "";

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        // ✅ Offsite confirmation workflow
        public OffsiteGiftStatus OffsiteStatus { get; set; } = OffsiteGiftStatus.Unconfirmed;

        [MaxLength(500)]
        public string? OffsiteReceiptNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ConfirmedAtUtc { get; set; }
    }
}