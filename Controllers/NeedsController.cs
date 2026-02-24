using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpliftBridge.Data;
using UpliftBridge.Models;
using Stripe.Checkout;

namespace UpliftBridge.Controllers
{
    public class NeedsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public NeedsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ----------------------------------
        // INDEX – list PUBLIC needs only
        // ----------------------------------
        public IActionResult Index()
        {
            var needs = _context.Needs
                .AsNoTracking()
                .Where(n => n.IsPublished)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(needs);
        }

        // ----------------------------------
        // DETAILS – only for published needs
        // ----------------------------------
        public IActionResult Details(int id)
        {
            var need = _context.Needs
                .AsNoTracking()
                .FirstOrDefault(n => n.Id == id && n.IsPublished);

            if (need == null) return NotFound();

            var updates = _context.NeedUpdates
                .AsNoTracking()
                .Where(u => u.NeedId == id && u.IsVisible)
                .OrderByDescending(u => u.CreatedAtUtc)
                .ToList();

            var photos = _context.NeedPhotos
                .AsNoTracking()
                .Where(p => p.NeedId == id)
                .OrderByDescending(p => p.CreatedAtUtc)
                .Select(p => p.Path)
                .ToList();

            ViewBag.Updates = updates;
            ViewBag.Photos = photos;

            return View(need);
        }

        // ----------------------------------
        // CREATE – GET
        // ----------------------------------
        [HttpGet]
        public IActionResult Create()
        {
            return View(new NeedCreateViewModel());
        }

        // ----------------------------------
        // CREATE – POST (redirects to CreateSuccess)
        // ----------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NeedCreateViewModel vm)
        {
            // Institution link is ONLY required when PreferDirectToInstitution is checked.
            if (vm.PreferDirectToInstitution &&
                string.IsNullOrWhiteSpace(vm.InstitutionPaymentLink))
            {
                ModelState.AddModelError(
                    nameof(vm.InstitutionPaymentLink),
                    "Institution payment link is required when direct payment is selected."
                );
            }

            if (!string.IsNullOrWhiteSpace(vm.InstitutionPaymentLink) &&
                !Uri.IsWellFormedUriString(vm.InstitutionPaymentLink, UriKind.Absolute))
            {
                ModelState.AddModelError(
                    nameof(vm.InstitutionPaymentLink),
                    "Please enter a valid, official payment URL."
                );
            }

            if (!ModelState.IsValid)
                return View(vm);

            var desc = BuildNeedDescription(vm);
            var shortSummary = BuildShortSummary(vm, desc);

            var need = new Need
            {
                Title = (vm.Title ?? "").Trim(),
                Description = desc,
                ShortSummary = shortSummary,

                Location = (vm.CityCountry ?? "").Trim(),
                Category = vm.Category,

                GoalAmount = vm.GoalAmount,
                AmountRaised = 0m,

                RequesterName = (vm.ForWhom ?? "").Trim(),
                RequesterEmail = (vm.ContactEmail ?? "").Trim(),

                PayTo = (vm.PayTo ?? "").Trim(),

                VerificationLevel = vm.VerificationLevel,
                VerificationNote = (vm.VerificationNote ?? "").Trim(),

                InstitutionName = (vm.InstitutionName ?? "").Trim(),
                InstitutionType = (vm.InstitutionType ?? "").Trim(),
                InstitutionPaymentLink = (vm.InstitutionPaymentLink ?? "").Trim(),
                PreferDirectToInstitution = vm.PreferDirectToInstitution,

                IsPublished = false, // pending review
                CreatedAt = DateTime.UtcNow
            };

            _context.Needs.Add(need);
            _context.SaveChanges();

            SaveNeedPhotos(need.Id, vm);

            // ✅ Pending needs must NOT go to Details (Details is published-only)
            return RedirectToAction(nameof(CreateSuccess), new { id = need.Id });
        }

        // ----------------------------------
        // CREATE SUCCESS – confirmation page for pending needs
        // Views/Needs/CreateSuccess.cshtml
        // ----------------------------------
        [HttpGet]
        public IActionResult CreateSuccess(int id)
        {
            var need = _context.Needs
                .AsNoTracking()
                .FirstOrDefault(n => n.Id == id);

            if (need == null) return NotFound();

            var photos = _context.NeedPhotos
                .AsNoTracking()
                .Where(p => p.NeedId == id)
                .OrderByDescending(p => p.CreatedAtUtc)
                .Select(p => p.Path)
                .ToList();

            ViewBag.Photos = photos;

            return View(need);
        }

        // ----------------------------------
        // FUND – GET (shows funding + platform tip)
        // ----------------------------------
        // GET: /Needs/Fund/5
        [HttpGet]
        public IActionResult Fund(int id)
        {
            var need = _context.Needs.AsNoTracking().FirstOrDefault(n => n.Id == id && n.IsPublished);
            if (need == null) return NotFound();

            var remaining = Math.Max(0m, need.GoalAmount - need.AmountRaised);

            var vm = new FundNeedViewModel
            {
                NeedId = need.Id,
                Title = need.Title,
                GoalAmount = need.GoalAmount,
                AmountRaised = need.AmountRaised,
                RemainingAmount = remaining,
                IsFullyFunded = remaining <= 0m,
                TipPercent = 1
            };

            return View(vm);
        }

        // ----------------------------------
        // FUND – POST (creates Stripe checkout for platform support ONLY)
        // ----------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Fund(FundNeedViewModel vm)
        {
            // Re-load need from DB (never trust posted RemainingAmount etc.)
            var need = _context.Needs.FirstOrDefault(n => n.Id == vm.NeedId && n.IsPublished);
            if (need == null) return NotFound();

            var remaining = Math.Max(0m, need.GoalAmount - need.AmountRaised);

            // Rebuild VM snapshot values (so view stays correct on validation errors)
            vm.Title = need.Title;
            vm.GoalAmount = need.GoalAmount;
            vm.AmountRaised = need.AmountRaised;
            vm.RemainingAmount = remaining;
            vm.IsFullyFunded = remaining <= 0m;

            if (vm.IsFullyFunded)
            {
                ModelState.AddModelError("", "This need is already fully funded.");
                return View(vm);
            }

            // Enforce anonymous behavior server-side
            if (vm.IsAnonymous)
            {
                vm.DonorName = "";
                vm.DonorEmail = "";
            }

            if (!ModelState.IsValid)
                return View(vm);

            // Compute platform support server-side (ignore client TipAmount)
            var cappedGift = Math.Min(Math.Max(0m, vm.ItemCost), remaining);
            var pct = Math.Max(0, Math.Min(20, vm.TipPercent));
            var platformSupport = Math.Round(cappedGift * (pct / 100m), 2);

            // Stripe min charge safety (USD)
            if (platformSupport > 0m && platformSupport < 0.50m) platformSupport = 0.50m;

            if (platformSupport <= 0m)
            {
                ModelState.AddModelError("", "Platform support must be greater than $0.00.");
                return View(vm);
            }

            var cents = (long)Math.Round(platformSupport * 100m, 0);

            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = cents,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "UpliftBridge platform support",
                                Description = $"Support fee for: {need.Title}"
                            }
                        }
                    }
                },
                SuccessUrl = domain + Url.Action("FundSuccess", "Needs", new { needId = need.Id, session_id = "{CHECKOUT_SESSION_ID}" }),
                CancelUrl = domain + Url.Action("Fund", "Needs", new { id = need.Id }),
                Metadata = new Dictionary<string, string>
                {
                    ["needId"] = need.Id.ToString(),
                    ["giftAmount"] = cappedGift.ToString("0.00"),
                    ["platformSupport"] = platformSupport.ToString("0.00"),
                    ["tipPercent"] = pct.ToString(),
                    ["isAnonymous"] = vm.IsAnonymous ? "1" : "0",
                    ["donorEmail"] = vm.IsAnonymous ? "" : (vm.DonorEmail ?? "")
                }
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }

        // ----------------------------------
        // FUND SUCCESS – Stripe returns here (Option A)
        // ----------------------------------
        [HttpGet]
        public IActionResult FundSuccess(int needId, string session_id)
        {
            if (string.IsNullOrWhiteSpace(session_id))
                return RedirectToAction("Fund", new { id = needId });

            var sessionService = new SessionService();
            var session = sessionService.Get(session_id);

            // must be paid
            if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Fund", new { id = needId });

            // metadata guard
            if (session.Metadata == null ||
                !session.Metadata.TryGetValue("needId", out var metaNeedId) ||
                !int.TryParse(metaNeedId, out var parsedNeedId) ||
                parsedNeedId != needId)
            {
                return RedirectToAction("Fund", new { id = needId });
            }

            var need = _context.Needs.FirstOrDefault(n => n.Id == needId && n.IsPublished);
            if (need == null) return NotFound();

            // amounts from metadata
            decimal giftAmount = 0m;
            decimal platformSupport = 0m;

            if (session.Metadata.TryGetValue("giftAmount", out var g) && decimal.TryParse(g, out var giftParsed))
                giftAmount = Math.Max(0m, giftParsed);

            if (session.Metadata.TryGetValue("platformSupport", out var p) && decimal.TryParse(p, out var platParsed))
                platformSupport = Math.Max(0m, platParsed);

            var stripePaid = ((session.AmountTotal ?? 0) / 100m);

            // Stripe truth must match platformSupport
            if (Math.Abs(stripePaid - platformSupport) > 0.02m)
                return RedirectToAction("Fund", new { id = needId });

            // Idempotent
            var existing = _context.GiftOrders.FirstOrDefault(o => o.StripeSessionId == session_id);
            GiftOrder order;

            if (existing != null)
            {
                order = existing;
            }
            else
            {
                var isAnon = session.Metadata.TryGetValue("isAnonymous", out var anon) && anon == "1";
                var donorEmail = session.Metadata.TryGetValue("donorEmail", out var de) ? de : "";

                order = new GiftOrder
                {
                    NeedId = need.Id,
                    NeedTitle = need.Title,

                    DonorName = null,
                    DonorEmail = isAnon ? null : (string.IsNullOrWhiteSpace(donorEmail) ? null : donorEmail),

                    PledgedGiftAmount = giftAmount,
                    PlatformSupportPaid = platformSupport,

                    PaymentStatus = PaymentStatus.Paid,

                    StripeSessionId = session_id,
                    StripePaymentIntentId = session.PaymentIntentId ?? "",
                    CreatedAt = DateTime.UtcNow,

                    OffsiteStatus = OffsiteGiftStatus.Unconfirmed
                };

                _context.GiftOrders.Add(order);

                need.AmountRaised += giftAmount;
                _context.Needs.Update(need);

                _context.SaveChanges();
            }

            var vm = new FundSuccessViewModel
            {
                Need = need,
                Order = order
            };

            return View(vm);
        }

        // ----------------------------------
        // COMPLETE DONATION – send them to official payment link page
        // ----------------------------------
        [HttpGet]
        public IActionResult CompleteDonation(int id)
        {
            var need = _context.Needs.AsNoTracking().FirstOrDefault(n => n.Id == id && n.IsPublished);
            if (need == null) return NotFound();

            return View(need);
        }

        // ----------------------------------
        // Helpers
        // ----------------------------------

        private void SaveNeedPhotos(int needId, NeedCreateViewModel vm)
        {
            var files = vm.Photos;
            if (files == null || files.Count == 0) return;

            const int MAX_FILES = 6;
            const long MAX_BYTES = 5 * 1024 * 1024; // 5MB

            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".webp" };

            var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var dir = Path.Combine(root, "uploads", "needs", needId.ToString());
            Directory.CreateDirectory(dir);

            int saved = 0;

            foreach (var file in files)
            {
                if (file == null || file.Length <= 0) continue;
                if (saved >= MAX_FILES) break;
                if (file.Length > MAX_BYTES) continue;

                var ext = Path.GetExtension(file.FileName ?? "");
                if (string.IsNullOrWhiteSpace(ext) || !allowed.Contains(ext)) continue;

                var safeName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
                var fullPath = Path.Combine(dir, safeName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var webPath = $"/uploads/needs/{needId}/{safeName}";

                _context.NeedPhotos.Add(new NeedPhoto
                {
                    NeedId = needId,
                    Path = webPath,
                    CreatedAtUtc = DateTime.UtcNow
                });

                saved++;
            }

            if (saved > 0) _context.SaveChanges();
        }

        private static string BuildNeedDescription(NeedCreateViewModel vm)
        {
            var sb = new StringBuilder();

            static bool IsTrashLocal(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return true;
                var v = s.Trim().ToLowerInvariant();
                return v == "na" || v == "n/a" || v == "none" || v == "-" || v == "--" || v == "null" || v == "0";
            }

            void AddBlock(string title, string text)
            {
                text = (text ?? "").Trim();
                if (string.IsNullOrWhiteSpace(text)) return;

                if (sb.Length > 0) sb.AppendLine().AppendLine();
                sb.AppendLine(title);
                sb.AppendLine(new string('-', Math.Min(title.Length, 28)));
                sb.AppendLine(text);
            }

            AddBlock("Story", vm.Story);
            AddBlock("Long-term dream", vm.LongTermDream);
            AddBlock("What has already been tried", vm.TriedAlready);

            if (!string.IsNullOrWhiteSpace(vm.Deadline) || !string.IsNullOrWhiteSpace(vm.Urgency))
            {
                if (sb.Length > 0) sb.AppendLine().AppendLine();
                sb.AppendLine("Timing");
                sb.AppendLine("------");
                if (!string.IsNullOrWhiteSpace(vm.Deadline)) sb.AppendLine($"Deadline: {vm.Deadline.Trim()}");
                if (!string.IsNullOrWhiteSpace(vm.Urgency)) sb.AppendLine($"Urgency: {vm.Urgency.Trim()}");
            }

            var items = (vm.Items ?? new List<NeedItemLineVm>())
                .Where(i => i != null)
                .Select(i => new NeedItemLineVm
                {
                    Name = (i!.Name ?? "").Trim(),
                    Cost = (i!.Cost ?? "").Trim(),
                    Link = (i!.Link ?? "").Trim()
                })
                .Select(i => new NeedItemLineVm
                {
                    Name = IsTrashLocal(i.Name) ? "" : i.Name,
                    Cost = IsTrashLocal(i.Cost) ? "" : i.Cost,
                    Link = IsTrashLocal(i.Link) ? "" : i.Link
                })
                .Where(i => !string.IsNullOrWhiteSpace(i.Name) ||
                            !string.IsNullOrWhiteSpace(i.Cost) ||
                            !string.IsNullOrWhiteSpace(i.Link))
                .ToList();

            if (items.Count > 0)
            {
                if (sb.Length > 0) sb.AppendLine().AppendLine();
                sb.AppendLine("Requested items");
                sb.AppendLine("--------------");

                int n = 1;
                foreach (var it in items)
                {
                    var name = (it.Name ?? "").Trim();
                    var cost = (it.Cost ?? "").Trim();
                    var link = (it.Link ?? "").Trim();

                    var line = $"{n}. {name}";
                    if (!string.IsNullOrWhiteSpace(cost)) line += $" — {cost}";
                    sb.AppendLine(line);

                    if (!string.IsNullOrWhiteSpace(link)) sb.AppendLine($"   Link: {link}");
                    n++;
                }
            }

            return sb.ToString().Trim();
        }

        private static string BuildShortSummary(NeedCreateViewModel vm, string desc)
        {
            var story = (vm.Story ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(story))
                return TruncateWithEllipsis(story, 160);

            var title = (vm.Title ?? "").Trim();

            string? firstLine =
                FirstNonEmptyLine(vm.LongTermDream, vm.TriedAlready) ??
                FirstNonEmptyLine(desc);

            var combined = (title + (string.IsNullOrWhiteSpace(firstLine) ? "" : $": {firstLine}")).Trim();
            if (string.IsNullOrWhiteSpace(combined))
                combined = "Support request";

            return TruncateWithEllipsis(combined, 160);
        }

        private static string? FirstNonEmptyLine(params string[] texts)
        {
            if (texts == null) return null;

            foreach (var t in texts)
            {
                if (string.IsNullOrWhiteSpace(t)) continue;

                var lines = t.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (var raw in lines)
                {
                    var line = (raw ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.Equals("Story", StringComparison.OrdinalIgnoreCase)) continue;
                    if (line.Equals("Long-term dream", StringComparison.OrdinalIgnoreCase)) continue;
                    if (line.Equals("What has already been tried", StringComparison.OrdinalIgnoreCase)) continue;
                    if (line.Equals("Timing", StringComparison.OrdinalIgnoreCase)) continue;
                    if (line.Equals("Requested items", StringComparison.OrdinalIgnoreCase)) continue;
                    if (line.All(ch => ch == '-' || ch == '—' || ch == '_')) continue;

                    return line;
                }
            }

            return null;
        }

        private static string TruncateWithEllipsis(string text, int max)
        {
            text = (text ?? "").Trim();
            if (text.Length <= max) return text;

            return text.Substring(0, max).TrimEnd() + "...";
        }
    }
}