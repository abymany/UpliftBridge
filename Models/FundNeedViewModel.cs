using System;
using System.ComponentModel.DataAnnotations;

namespace UpliftBridge.Models
{
    public class FundNeedViewModel
    {
        public int NeedId { get; set; }

        [MaxLength(140)]
        public string Title { get; set; } = "";

        public bool IsAnonymous { get; set; }

        [MaxLength(120)]
        public string DonorName { get; set; } = "";

        [MaxLength(160)]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string DonorEmail { get; set; } = "";

        [Range(50, 1000000, ErrorMessage = "Minimum gift is $50.")]
        public decimal ItemCost { get; set; }

        [Range(1, 20, ErrorMessage = "Platform support must be between 1% and 20%.")]
        public int TipPercent { get; set; } = 1;

        public decimal GoalAmount { get; set; }
        public decimal AmountRaised { get; set; }

        public decimal RemainingAmount
        {
            get
            {
                var remaining = GoalAmount - AmountRaised;
                return remaining < 0 ? 0m : remaining;
            }
        }

        public bool IsFullyFunded => RemainingAmount <= 0m;

        public decimal CappedGiftAmount
        {
            get
            {
                var raw = ItemCost < 0 ? 0m : ItemCost;
                return Math.Min(raw, RemainingAmount);
            }
        }

        public decimal CalculatedPlatformFee
        {
            get
            {
                var pct = Math.Max(1, Math.Min(20, TipPercent));
                var fee = Math.Round(CappedGiftAmount * (pct / 100m), 2);

                if (fee > 0m && fee < 0.50m)
                    fee = 0.50m;

                return fee;
            }
        }
    }
}