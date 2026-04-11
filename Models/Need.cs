using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpliftBridge.Models
{
    public enum NeedCategory
    {
        Education,
        Sports,
        Family,
        Medical,
        Other
    }

    public enum VerificationLevel
    {
        BasicContactVerified,
        CommunityVerified,
        OrganizationVerified
    }

    public class Need
    {
        public int Id { get; set; }

        [Required, MaxLength(140)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(220)]
        public string ShortSummary { get; set; } = string.Empty;

        [Required, MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public NeedCategory Category { get; set; } = NeedCategory.Other;

        [Required, MaxLength(160)]
        public string Location { get; set; } = string.Empty;

        [Range(0, 1000000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GoalAmount { get; set; }

        [Range(0, 1000000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountRaised { get; set; }

        [Required, MaxLength(120)]
        public string RequesterName { get; set; } = string.Empty;

        [Required, MaxLength(160)]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string RequesterEmail { get; set; } = string.Empty;

        [MaxLength(40)]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public bool IsEmailVerified { get; set; } = false;

        public bool IsPhoneVerified { get; set; } = false;

        [MaxLength(12)]
        public string EmailOtpCode { get; set; } = string.Empty;

        public DateTime? EmailOtpExpiresAtUtc { get; set; }

        public DateTime? EmailVerifiedAtUtc { get; set; }

        // payment routing (optional)
        [MaxLength(180)]
        public string PayTo { get; set; } = string.Empty;

        [MaxLength(180)]
        public string InstitutionName { get; set; } = string.Empty;

        [MaxLength(60)]
        public string InstitutionType { get; set; } = string.Empty;

        [MaxLength(400)]
        public string InstitutionFullAddress { get; set; } = string.Empty;

        [MaxLength(400)]
        public string InstitutionPaymentLink { get; set; } = string.Empty;

        public bool PreferDirectToInstitution { get; set; } = false;

        // trust
        public VerificationLevel VerificationLevel { get; set; } = VerificationLevel.BasicContactVerified;

        [MaxLength(600)]
        public string VerificationNote { get; set; } = string.Empty;

        [MaxLength(120)]
        public string VerifiedBy { get; set; } = string.Empty;

        public DateTime? VerifiedAtUtc { get; set; }

        // moderation / risk
        public bool IsSuspicious { get; set; } = false;

        [MaxLength(800)]
        public string RiskNotes { get; set; } = string.Empty;

        [MaxLength(120)]
        public string InternalReviewStatus { get; set; } = "Pending";

        [MaxLength(1200)]
        public string InternalReviewNotes { get; set; } = string.Empty;

        // publication
        public bool IsPublished { get; set; } = false;

        [MaxLength(400)]
        public string RejectionReason { get; set; } = string.Empty;

        [MaxLength(120)]
        public string ReviewedBy { get; set; } = string.Empty;

        public DateTime? ReviewedAtUtc { get; set; } = null;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string SubmissionToken { get; set; } = "";

        [NotMapped]
        public string StatusLabel =>
            IsPublished ? "Approved"
            : !string.IsNullOrWhiteSpace(RejectionReason) ? "Rejected"
            : string.IsNullOrWhiteSpace(InternalReviewStatus) ? "Pending"
            : InternalReviewStatus;

        [NotMapped]
        public decimal RemainingAmount
        {
            get
            {
                if (GoalAmount <= 0) return 0m;
                var remaining = GoalAmount - AmountRaised;
                return remaining < 0 ? 0m : remaining;
            }
        }

        [NotMapped]
        public bool IsFullyFunded => GoalAmount > 0 && AmountRaised >= GoalAmount;

        [NotMapped]
        public int PercentFunded
        {
            get
            {
                if (GoalAmount <= 0) return 0;
                var pct = (int)Math.Round((AmountRaised / GoalAmount) * 100m);
                if (pct < 0) return 0;
                if (pct > 100) return 100;
                return pct;
            }
        }
    }
}