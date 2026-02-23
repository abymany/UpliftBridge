using System;

namespace UpliftBridge.Models
{
    public class FundSuccessViewModel
    {
        // pledge-based FundSuccess flow
        public int PledgeId { get; set; }
        public int NeedId { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }

        // ✅ Must be nullable (it IS null currently unless controller loads it)
        public Need? Need { get; set; }

        // Optional
        public GiftOrder? Order { get; set; }
    }
}