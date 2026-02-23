using System;
using System.ComponentModel.DataAnnotations;

namespace UpliftBridge.Models
{
    public class FundNeedViewModel
    {
        public int NeedId { get; set; }

        [MaxLength(140)]
        public string Title { get; set; } = "";

        // -----------------------------
        // Donor identity
        // -----------------------------
        public bool IsAnonymous { get; set; }

        [MaxLength(120)]
        public string DonorName { get; set; } = "";

        [MaxLength(160)]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string DonorEmail { get; set; } = "";

        // -----------------------------
        // Gift (pledge amount)
        // -----------------------------
        [Range(50, 1000000, ErrorMessage = "Please enter an amount of at least $50.00.")]
        public decimal ItemCost { get; set; }

        // -----------------------------
        // Platform support (Option A)
        // Donor pays ONLY platform support on UpliftBridge.
        // Gift is handled on the official payment link later.
        // -----------------------------
        [Range(0, 20, ErrorMessage = "Tip percent must be between 0 and 20.")]
        public int TipPercent { get; set; } = 1;

        // Client can send it, but server will RE-CALCULATE anyway.
        public decimal TipAmount { get; set; }

        // -----------------------------
        // Need snapshot for UI
        // -----------------------------
        public decimal GoalAmount { get; set; }
        public decimal AmountRaised { get; set; }
        public decimal RemainingAmount { get; set; }
        public bool IsFullyFunded { get; set; }

        // -----------------------------
        // Helpers
        // -----------------------------
        public decimal CappedGiftAmount
        {
            get
            {
                var remaining = RemainingAmount < 0 ? 0m : RemainingAmount;
                var raw = ItemCost < 0 ? 0m : ItemCost;
                return Math.Min(raw, remaining);
            }
        }

        public decimal CalculatedPlatformFee
        {
            get
            {
                // 1% (or TipPercent), but prevent Stripe-min-charge failures
                var pct = Math.Max(0, Math.Min(20, TipPercent));
                var fee = Math.Round(CappedGiftAmount * (pct / 100m), 2);

                // Stripe min charge safety (USD): $0.50
                // If you hate this, then set minimum gift to $50 instead.
                if (fee > 0m && fee < 0.50m) fee = 0.50m;

                return fee;
            }
        }
    }
}
