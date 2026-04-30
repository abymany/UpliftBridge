using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IActionResult> Details(int id)
        {
            var need = await _db.Needs.FirstOrDefaultAsync(n => n.Id == id && n.IsPublished);
            if (need == null) return NotFound();

            ViewBag.Photos = await _db.NeedPhotos
                .AsNoTracking()
                .Where(p => p.NeedId == id && !string.IsNullOrWhiteSpace(p.Path))
                .OrderByDescending(p => p.CreatedAtUtc)
                .Select(p => p.Path)
                .ToListAsync();

            ViewBag.Updates = await _db.NeedUpdates
                .AsNoTracking()
                .Where(u => u.NeedId == id && u.IsVisible)
                .OrderByDescending(u => u.CreatedAtUtc)
                .ToListAsync();

            return View(need);
        }

        [HttpGet]
        public async Task<IActionResult> Fund(int id)
        {
            var need = await _db.Needs
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id && n.IsPublished);

            if (need == null) return NotFound();

            var vm = new FundNeedViewModel
            {
                NeedId = need.Id,
                Title = need.Title,
                GoalAmount = need.GoalAmount,
                AmountRaised = need.AmountRaised,
                TipPercent = 1
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckout(FundNeedViewModel vm)
        {
            var need = await _db.Needs
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == vm.NeedId && n.IsPublished);

            if (need == null) return NotFound();

            vm.GoalAmount = need.GoalAmount;
            vm.AmountRaised = need.AmountRaised;

            var platformFee = vm.CalculatedPlatformFee;
            var giftAmount = vm.CappedGiftAmount;

            if (giftAmount < 50m)
            {
                ModelState.AddModelError(nameof(vm.ItemCost), "Minimum gift is $50.");
                return View("Fund", vm);
            }

            if (platformFee <= 0m)
            {
                ModelState.AddModelError("", "Please choose platform support greater than $0.");
                return View("Fund", vm);
            }

            var domain = $"{Request.Scheme}://{Request.Host}";

            var successUrl = !string.IsNullOrWhiteSpace(need.InstitutionPaymentLink)
                ? domain + $"/Needs/FundSuccess?session_id={{CHECKOUT_SESSION_ID}}&needId={need.Id}"
                : domain + $"/Needs/FundSuccess?session_id={{CHECKOUT_SESSION_ID}}&needId={need.Id}";

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = domain + $"/Needs/Fund/{need.Id}",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)Math.Round(platformFee * 100m),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "UpliftBridge platform support",
                                Description = $"Platform support for: {need.Title}"
                            }
                        }
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    { "needId", need.Id.ToString() },
                    { "giftAmount", giftAmount.ToString("0.00") },
                    { "platformSupport", platformFee.ToString("0.00") },
                    { "donorName", vm.IsAnonymous ? "" : (vm.DonorName ?? "") },
                    { "donorEmail", vm.IsAnonymous ? "" : (vm.DonorEmail ?? "") },
                    { "isAnonymous", vm.IsAnonymous ? "1" : "0" }
                }
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }

        [HttpGet]
        public async Task<IActionResult> FundSuccess(int needId, string session_id)
        {
            if (string.IsNullOrWhiteSpace(session_id))
                return RedirectToAction(nameof(Details), new { id = needId });

            var service = new SessionService();
            var session = service.Get(session_id);

            if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction(nameof(Details), new { id = needId });

            var need = await _db.Needs
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == needId && n.IsPublished);

            if (need == null) return NotFound();

            // Important: we do NOT update AmountRaised here.
            // The gift is completed on the official external site, not collected by UpliftBridge.

            return RedirectToAction(nameof(CompleteDonation), new { id = need.Id });
        }

        [HttpGet]
        public async Task<IActionResult> CompleteDonation(int id)
        {
            var need = await _db.Needs
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id && n.IsPublished);

            if (need == null) return NotFound();

            return View(need);
        }
    }
}