using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UpliftBridge.Models
{
    public class NeedItemLineVm
    {
        [MaxLength(120)]
        public string? Name { get; set; }

        [MaxLength(60)]
        public string? Cost { get; set; }

        [MaxLength(400)]
        public string? Link { get; set; }
    }

    public class NeedCreateViewModel : IValidatableObject
    {
        // CORE
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

        // MONEY
        [Required]
        [Range(1, 1000000)]
        [Display(Name = "Goal amount")]
        public decimal GoalAmount { get; set; }

        [MaxLength(120)]
        public string? Deadline { get; set; }

        [MaxLength(40)]
        public string? Urgency { get; set; }

        // TRUST
        public VerificationLevel VerificationLevel { get; set; } =
            VerificationLevel.BasicContactVerified;

        [MaxLength(600)]
        public string? VerificationNote { get; set; }

        [MaxLength(180)]
        public string? PayTo { get; set; }

        // FUNDING ROUTE
        [Display(Name =
            "Prefer donor to pay institution directly")]
        public bool PreferDirectToInstitution { get; set; }

        [MaxLength(180)]
        [Display(Name = "Institution name")]
        public string? InstitutionName { get; set; }

        [MaxLength(60)]
        [Display(Name = "Institution type")]
        public string? InstitutionType { get; set; }

        [MaxLength(400)]
        [Display(Name = "Official payment link")]
        public string? InstitutionPaymentLink { get; set; }

        [MaxLength(400)]
        [Display(Name = "Institution address (private)")]
        public string? InstitutionFullAddress { get; set; }

        // NEW
        [MaxLength(1200)]
        [Display(Name =
            "If no official payment link exists, explain where support should go")]
        public string? FundingRouteExplanation { get; set; }

        // CONTACT
        [Required, MaxLength(160)]
        [EmailAddress]
        public string ContactEmail { get; set; } = "";

        [MaxLength(40)]
        public string? ContactPhone { get; set; }

        // ITEMS
        public List<NeedItemLineVm> Items { get; set; } = new()
        {
            new NeedItemLineVm(),
            new NeedItemLineVm(),
            new NeedItemLineVm()
        };

        public List<IFormFile> Photos { get; set; } = new();

        [Range(typeof(bool),"true","true")]
        public bool HonestyPledge { get; set; }

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            while (Items.Count < 3)
                Items.Add(new NeedItemLineVm());

            static bool IsTrash(string? s)
            {
                if (string.IsNullOrWhiteSpace(s))
                    return true;

                var v = s.Trim().ToLowerInvariant();

                return v is
                    "na" or "n/a" or
                    "none" or "-" or
                    "--" or "null" or "0";
            }

            var i1 = Items[0];

            if (IsTrash(i1.Name))
                yield return new ValidationResult(
                    "Item 1 required",
                    new[] { "Items[0].Name" });

            if (IsTrash(i1.Cost))
                yield return new ValidationResult(
                    "Item 1 cost required",
                    new[] { "Items[0].Cost" });

            // DIRECT INSTITUTION ROUTE
            if (PreferDirectToInstitution)
            {
                if (string.IsNullOrWhiteSpace(
                    InstitutionPaymentLink))
                {
                    yield return new ValidationResult(
                        "Official payment link required.",
                        new[]
                        {
                            nameof(
                            InstitutionPaymentLink)
                        });
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(
                    FundingRouteExplanation))
                {
                    yield return new ValidationResult(
                        "Explain where support should go when no official payment route exists.",
                        new[]
                        {
                            nameof(
                            FundingRouteExplanation)
                        });
                }
            }
        }
    }
}