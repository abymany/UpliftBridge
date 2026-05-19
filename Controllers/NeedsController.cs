using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _env;

        public NeedsController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var needs = await _db.Needs
                .AsNoTracking()
                .Where(n => n.IsPublished)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var needIds = needs.Select(n => n.Id).ToList();

            var photoMap = await _db.NeedPhotos
                .AsNoTracking()
                .Where(p => needIds.Contains(p.NeedId) && !string.IsNullOrWhiteSpace(p.Path))
                .OrderByDescending(p => p.CreatedAtUtc)
                .ToListAsync();

            ViewBag.PhotoMap = photoMap
                .GroupBy(p => p.NeedId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Path).FirstOrDefault() ?? "");

            return View(needs);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new NeedCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NeedCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var cleanLink = (vm.InstitutionPaymentLink ?? "").Trim();

            if (vm.PreferDirectToInstitution)
            {
                if (!Uri.TryCreate(cleanLink, UriKind.Absolute, out var uri) || uri.Scheme != "https")
                {
                    ModelState.AddModelError(nameof(vm.InstitutionPaymentLink), "Please enter a valid HTTPS official payment link.");
                    return View(vm);
                }
            }

            var itemLines = vm.Items
                .Where(i => !string.IsNullOrWhiteSpace(i.Name) && !string.IsNullOrWhiteSpace(i.Cost))
                .Select((i, index) => $"{index + 1}. {i.Name?.Trim()} — {i.Cost?.Trim()}")
                .ToList();

            var description =
$@"Story
-----
{vm.Story?.Trim()}

Long-term dream
---------------
{vm.LongTermDream?.Trim()}

What has already been tried
---------------------------
{vm.TriedAlready?.Trim()}

Timing
------
Deadline: {vm.Deadline}
Urgency: {vm.Urgency}

Requested items
---------------
{string.Join(Environment.NewLine, itemLines)}";

            var need = new Need
            {
                Title = vm.Title.Trim(),
                ShortSummary = vm.Story.Length > 200 ? vm.Story.Substring(0, 200) : vm.Story,
                Description = description,
                Category = vm.Category,
                Location = vm.CityCountry.Trim(),
                GoalAmount = vm.GoalAmount,
                AmountRaised = 0m,
                RequesterName = vm.ForWhom.Trim(),
                RequesterEmail = vm.ContactEmail.Trim(),
                PhoneNumber = vm.ContactPhone?.Trim() ?? "",
                PayTo = vm.PayTo?.Trim() ?? "",
                InstitutionName = vm.InstitutionName?.Trim() ?? "",
                InstitutionType = vm.InstitutionType?.Trim() ?? "",
                InstitutionFullAddress = vm.InstitutionFullAddress?.Trim() ?? "",
                InstitutionPaymentLink = cleanLink,
                PreferDirectToInstitution = vm.PreferDirectToInstitution,
                VerificationLevel = VerificationLevel.BasicContactVerified,
                VerificationNote = vm.VerificationNote?.Trim() ?? "",
                InternalReviewStatus = "Pending",
                IsPublished = false,
                CreatedAt = DateTime.UtcNow,
                SubmissionToken = Guid.NewGuid().ToString("N")
            };

            _db.Needs.Add(need);
            await _db.SaveChangesAsync();

            if (vm.Photos != null && vm.Photos.Any())
            {
                var savedPaths = await SaveNeedPhotosAsync(need.Id, vm.Photos);

                foreach (var path in savedPaths)
                {
                    _db.NeedPhotos.Add(new NeedPhoto
                    {
                        NeedId = need.Id,
                        Path = path,
                        CreatedAtUtc = DateTime.UtcNow
                    });
                }

                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(CreateSuccess), new { id = need.Id });
        }

        [HttpGet]
        public async Task<IActionResult> CreateSuccess(int id)
        {
            var need = await _db.Needs
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id);

            if (need == null) return NotFound();

            return View(need);
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

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = domain + $"/Needs/FundSuccess?session_id={{CHECKOUT_SESSION_ID}}&needId={need.Id}",
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

        private async Task<List<string>> SaveNeedPhotosAsync(int needId, List<IFormFile> photos)
        {
            var saved = new List<string>();

            var usePersistent = Directory.Exists("/var/data");
            var root = usePersistent
                ? "/var/data/uploads"
                : Path.Combine(_env.WebRootPath, "uploads");

            var folder = Path.Combine(root, "needs", needId.ToString());
            Directory.CreateDirectory(folder);

            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            foreach (var photo in photos.Take(6))
            {
                if (photo == null || photo.Length <= 0) continue;

                var ext = Path.GetExtension(photo.FileName);
                if (!allowed.Contains(ext)) continue;

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(folder, fileName);

                await using var stream = new FileStream(fullPath, FileMode.Create);
                await photo.CopyToAsync(stream);

                saved.Add($"/uploads/needs/{needId}/{fileName}");
            }

            return saved;
        }
    }
}