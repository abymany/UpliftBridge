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
        // Gift (amount donor wants to cover)
        // -----------------------------
        [Range(50, 1000000, ErrorMessage = "Minimum gift is $50.")]
        public decimal ItemCost { get; set; }

        // -----------------------------
        // Platform support (your revenue)
        // -----------------------------
        [Range(0, 20)]
        public int TipPercent { get; set; } = 1;

        // -----------------------------
        // Need snapshot
        // -----------------------------
        public decimal GoalAmount { get; set; }
        public decimal AmountRaised { get; set; }

        public decimal RemainingAmount
        {
            get
            {
                var remaining = GoalAmount - AmountRaised;
                return remaining < 0 ? 0 : remaining;
            }
        }

        public bool IsFullyFunded => RemainingAmount <= 0;

        // -----------------------------
        // CORE LOGIC (DO NOT BREAK)
        // -----------------------------
        public decimal CappedGiftAmount
        {
            get
            {
                var raw = ItemCost < 0 ? 0 : ItemCost;
                return Math.Min(raw, RemainingAmount);
            }
        }

        public decimal PlatformFee
        {
            get
            {
                var pct = Math.Max(0, Math.Min(20, TipPercent));
                var fee = Math.Round(CappedGiftAmount * (pct / 100m), 2);

                // Stripe minimum charge protection
                if (fee > 0m && fee < 0.50m)
                    fee = 0.50m;

                return fee;
            }
        }

        // 🔴 THIS is what Stripe will charge
        public decimal TotalCharge
        {
            get
            {
                return PlatformFee; // ONLY fee charged on your platform
            }
        }
    }
}