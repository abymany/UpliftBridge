using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UpliftBridge.Models
{
    public class NeedItemLineVm
    {
        // OPTIONAL unless your Validate() says otherwise
        [MaxLength(120)]
        public string? Name { get; set; }

        [MaxLength(60)]
        public string? Cost { get; set; }

        [MaxLength(400)]
        public string? Link { get; set; }
    }

    public class NeedCreateViewModel : IValidatableObject
    {
        // -----------------------------
        // Core (REQUIRED)
        // -----------------------------
        [Required, MaxLength(140)]
        public string Title { get; set; } = "";

        [Required, MaxLength(120)]
        [Display(Name = "Who is this for?")]
        public string ForWhom { get; set; } = "";

        [Required, MaxLength(40)]
        [Display(Name = "Approximate age range")]
        public string AgeRange { get; set; } = "";

        [Required, MaxLength(4000)]
        [Display(Name = "Story and background")]
        public string Story { get; set; } = "";

        [Required, MaxLength(1200)]
        [Display(Name = "Long-term dream connected to this gift")]
        public string LongTermDream { get; set; } = "";

        [Required, MaxLength(1200)]
        [Display(Name = "What has already been tried?")]
        public string TriedAlready { get; set; } = "";

        [Required]
        public NeedCategory Category { get; set; } = NeedCategory.Other;

        [Required, MaxLength(160)]
        [Display(Name = "City & country")]
        public string CityCountry { get; set; } = "";

        // -----------------------------
        // Money
        // -----------------------------
        [Required]
        [Range(1, 1000000, ErrorMessage = "Please enter a realistic amount (at least 1).")]
        [Display(Name = "Goal amount")]
        public decimal GoalAmount { get; set; }

        [MaxLength(120)]
        public string? Deadline { get; set; }

        [MaxLength(40)]
        public string? Urgency { get; set; }

        // -----------------------------
        // Verification / trust
        // -----------------------------
        public VerificationLevel VerificationLevel { get; set; } = VerificationLevel.BasicContactVerified;

        [MaxLength(600)]
        [Display(Name = "Verification note")]
        public string? VerificationNote { get; set; }

        [MaxLength(180)]
        [Display(Name = "Pay to (optional)")]
        public string? PayTo { get; set; }

        // -----------------------------
        // Institution / payment routing (OPTIONAL)
        // -----------------------------
        [MaxLength(180)]
        [Display(Name = "Institution name (optional)")]
        public string? InstitutionName { get; set; }

        [MaxLength(60)]
        [Display(Name = "Institution type (optional)")]
        public string? InstitutionType { get; set; }

        [MaxLength(400)]
        [Display(Name = "Institution payment link (optional)")]
        public string? InstitutionPaymentLink { get; set; }

        [Display(Name = "Prefer donor to pay the institution directly (recommended when available)")]
        public bool PreferDirectToInstitution { get; set; }

        // -----------------------------
        // Contact (private)
        // -----------------------------
        [Required, MaxLength(160)]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Contact email")]
        public string ContactEmail { get; set; } = "";

        [MaxLength(40)]
        public string? ContactPhone { get; set; }

        // -----------------------------
        // Items
        // -----------------------------
        public List<NeedItemLineVm> Items { get; set; } = new()
        {
            new NeedItemLineVm(),
            new NeedItemLineVm(),
            new NeedItemLineVm()
        };

        // -----------------------------
        // Photos (optional)
        // -----------------------------
        public List<IFormFile> Photos { get; set; } = new();

        // -----------------------------
        // Consent
        // -----------------------------
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm the honesty pledge.")]
        public bool HonestyPledge { get; set; }

        // -----------------------------
        // Server-side validation rules (THE SOURCE OF TRUTH)
        // -----------------------------
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Ensure 3 slots exist
            while (Items.Count < 3) Items.Add(new NeedItemLineVm());

            // Ban fake placeholders
            static bool IsTrash(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return true;
                var v = s.Trim().ToLowerInvariant();
                return v is "na" or "n/a" or "none" or "-" or "--" or "null" or "0";
            }

            // Item 1 REQUIRED: Name + Cost (link optional)
            var i1 = Items[0] ?? new NeedItemLineVm();

            if (IsTrash(i1.Name))
                yield return new ValidationResult("Item 1 name is required (don’t use NA).", new[] { "Items[0].Name" });

            if (IsTrash(i1.Cost))
                yield return new ValidationResult("Item 1 estimated cost is required (don’t use NA).", new[] { "Items[0].Cost" });

            // Item 2/3 OPTIONAL — if user touches any field with REAL content, require Name + Cost (no trash)
            for (int idx = 1; idx <= 2; idx++)
            {
                var it = Items[idx] ?? new NeedItemLineVm();

                // ✅ treat NA / n/a / none / "-" as empty (not "touched")
                var any =
                    !IsTrash(it.Name) ||
                    !IsTrash(it.Cost) ||
                    !IsTrash(it.Link);

                if (!any) continue;

                if (IsTrash(it.Name))
                    yield return new ValidationResult($"Item {idx + 1} name is required when you add an item.", new[] { $"Items[{idx}].Name" });

                if (IsTrash(it.Cost))
                    yield return new ValidationResult($"Item {idx + 1} estimated cost is required when you add an item.", new[] { $"Items[{idx}].Cost" });
            }

            // Institution payment link required ONLY if PreferDirectToInstitution is checked
            if (PreferDirectToInstitution)
            {
                if (string.IsNullOrWhiteSpace(InstitutionPaymentLink))
                {
                    yield return new ValidationResult(
                        "Institution payment link is required when direct payment is selected.",
                        new[] { nameof(InstitutionPaymentLink) }
                    );
                }
                else if (!Uri.IsWellFormedUriString(InstitutionPaymentLink, UriKind.Absolute))
                {
                    yield return new ValidationResult(
                        "Please enter a valid, official payment URL.",
                        new[] { nameof(InstitutionPaymentLink) }
                    );
                }
            }
        }
    }
}
