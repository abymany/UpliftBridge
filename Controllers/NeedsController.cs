using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using UpliftBridge.Data;
using UpliftBridge.Models;

namespace UpliftBridge.Controllers
{
    public class NeedsController : Controller
    {
        private readonly AppDbContext _db;

        public NeedsController(AppDbContext db)
        {
            _db = db;
        }

        // =========================
        // DETAILS PAGE
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var need = await _db.Needs.FirstOrDefaultAsync(n => n.Id == id && n.IsPublished);
            if (need == null) return NotFound();

            var photos = await _db.NeedPhotos
                .Where(p => p.NeedId == id && !string.IsNullOrWhiteSpace(p.Path))
                .Select(p => p.Path)
                .ToListAsync();

            var updates = await _db.NeedUpdates
                .Where(u => u.NeedId == id && u.IsVisible)
                .OrderByDescending(u => u.CreatedAtUtc)
                .ToListAsync();

            ViewBag.Photos = photos;
            ViewBag.Updates = updates;

            return View(need);
        }

        // =========================
        // FUND PAGE (GET)
        // =========================
        public async Task<IActionResult> Fund(int id)
        {
            var need = await _db.Needs.FirstOrDefaultAsync(n => n.Id == id && n.IsPublished);
            if (need == null) return NotFound();

            var vm = new FundNeedViewModel
            {
                NeedId = need.Id,
                Title = need.Title,
                GoalAmount = need.GoalAmount,
                AmountRaised = need.AmountRaised
            };

            return View(vm);
        }

        // =========================
        // CREATE STRIPE SESSION
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckout(FundNeedViewModel vm)
        {
            var need = await _db.Needs.FirstOrDefaultAsync(n => n.Id == vm.NeedId && n.IsPublished);
            if (need == null) return NotFound();

            // 🔴 HARD RULE: always recalc server-side
            vm.GoalAmount = need.GoalAmount;
            vm.AmountRaised = need.AmountRaised;

            var platformFee = vm.PlatformFee;

            if (platformFee <= 0)
            {
                ModelState.AddModelError("", "Invalid payment amount.");
                return View("Fund", vm);
            }

            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = domain + $"/Needs/Success?session_id={{CHECKOUT_SESSION_ID}}&needId={need.Id}",
                CancelUrl = domain + $"/Needs/Fund/{need.Id}",

                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(platformFee * 100), // cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "UpliftBridge platform support",
                                Description = $"Support fee for: {need.Title}"
                            }
                        }
                    }
                },

                Metadata = new Dictionary<string, string>
                {
                    { "needId", need.Id.ToString() },
                    { "platformFee", platformFee.ToString() },
                    { "giftAmount", vm.CappedGiftAmount.ToString() }
                }
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Redirect(session.Url);
        }

        // =========================
        // SUCCESS PAGE
        // =========================
        public async Task<IActionResult> Success(string session_id, int needId)
        {
            if (string.IsNullOrEmpty(session_id))
                return RedirectToAction("Details", new { id = needId });

            var service = new SessionService();
            var session = service.Get(session_id);

            if (session.PaymentStatus != "paid")
                return RedirectToAction("Details", new { id = needId });

            var need = await _db.Needs.FirstOrDefaultAsync(n => n.Id == needId);
            if (need == null) return NotFound();

            // 🔴 Update raised amount ONLY for platform-tracked gifts
            if (session.Metadata.TryGetValue("giftAmount", out var giftStr)
                && decimal.TryParse(giftStr, out var giftAmount))
            {
                need.AmountRaised += giftAmount;
            }

            await _db.SaveChangesAsync();

            return View("Success", need);
        }
    }
}