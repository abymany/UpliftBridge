using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpliftBridge.Data;
using UpliftBridge.Models;

namespace UpliftBridge.Controllers
{
    [Route("Admin/Needs")]
    public class AdminNeedsController : Controller
    {
        private readonly AppDbContext _context;

        private const string ADMIN_KEY = "Mani0751";
        private const string ADMIN_NAME = "Admin";

        public AdminNeedsController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin(string? key)
        {
            if (!string.IsNullOrWhiteSpace(key) && key == ADMIN_KEY)
            {
                HttpContext.Session.SetString("kg_admin", "1");
                return true;
            }

            return HttpContext.Session.GetString("kg_admin") == "1";
        }

        private IActionResult RedirectBackOrDetails(int id, string? key, string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Details), new { id, key });
        }

        private void StampReview(Need need, string reviewStatus)
        {
            need.ReviewedBy = ADMIN_NAME;
            need.ReviewedAtUtc = DateTime.UtcNow;
            need.InternalReviewStatus = string.IsNullOrWhiteSpace(reviewStatus) ? "Pending" : reviewStatus.Trim();
        }

        private static string NormalizePhotoPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            var clean = path.Trim().Replace("\\", "/");
            if (!clean.StartsWith("/"))
                clean = "/" + clean;

            return clean;
        }

        [HttpGet("")]
        public IActionResult Index(string? key)
        {
            if (!IsAdmin(key)) return Unauthorized("Admin key required.");

            var needs = _context.Needs
                .AsNoTracking()
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            ViewBag.AdminKey = key ?? "";
            return View(needs);
        }

        [HttpGet("See/{id:int}")]
        public IActionResult See(int id, string? key)
        {
            return Details(id, key);
        }

        [HttpGet("Details/{id:int}")]
        public IActionResult Details(int id, string? key)
        {
            if (!IsAdmin(key)) return Unauthorized("Admin key required.");

            var need = _context.Needs
                .AsNoTracking()
                .FirstOrDefault(n => n.Id == id);

            if (need == null) return NotFound();

            var updates = _context.NeedUpdates
                .AsNoTracking()
                .Where(u => u.NeedId == id && u.IsVisible)
                .OrderByDescending(u => u.CreatedAtUtc)
                .ToList();

            var photos = _context.NeedPhotos
                .AsNoTracking()
                .Where(n => n.NeedId == id && n.Path != null && n.Path.Trim() != "")
                .OrderByDescending(n => n.CreatedAtUtc)
                .Select(n => n.Path)
                .ToList()
                .Select(p => NormalizePhotoPath(p ?? ""))
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();
                
            ViewBag.AdminKey = key ?? "";
            ViewBag.Updates = updates;
            ViewBag.Photos = photos;

            return View("See", need);
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id, string? key, string? returnUrl)
        {
            if (!IsAdmin(key)) return Unauthorized("Admin key required.");

            var need = _context.Needs.FirstOrDefault(n => n.Id == id);
            if (need == null) return NotFound();

            need.IsPublished = true;
            need.RejectionReason = "";
            need.IsSuspicious = false;

            StampReview(need, "Approved");

            if (need.VerificationLevel == VerificationLevel.BasicContactVerified)
            {
                need.VerifiedBy = ADMIN_NAME;
                need.VerifiedAtUtc = DateTime.UtcNow;
            }

            _context.SaveChanges();

            return RedirectBackOrDetails(id, key, returnUrl);
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id, string reason, string? key, string? returnUrl)
        {
            if (!IsAdmin(key)) return Unauthorized("Admin key required.");

            if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 10)
                return BadRequest("Please enter a clear rejection reason (at least 10 characters).");

            var need = _context.Needs.FirstOrDefault(n => n.Id == id);
            if (need == null) return NotFound();

            need.IsPublished = false;
            need.RejectionReason = reason.Trim();

            StampReview(need, "Rejected");

            _context.SaveChanges();

            return RedirectBackOrDetails(id, key, returnUrl);
        }

        [HttpPost("MarkNeedsInfo")]
        [ValidateAntiForgeryToken]
        public IActionResult MarkNeedsInfo(int id, string notes, string? key, string? returnUrl)
        {
            if (!IsAdmin(key)) return Unauthorized("Admin key required.");

            var need = _context.Needs.FirstOrDefault(n => n.Id == id);
            if (need == null) return NotFound();

            need.IsPublished = false;
            need.RejectionReason = "";
            need.InternalReviewNotes = (notes ?? "").Trim();

            StampReview(need, "Needs More Info");

            _context.SaveChanges();

            return RedirectBackOrDetails(id, key, returnUrl);
        }

        [HttpPost("FlagSuspicious")]
        [ValidateAntiForgeryToken]
        public IActionResult FlagSuspicious(int id, string notes, string? key, string? returnUrl)
        {
            if (!IsAdmin(key)) return Unauthorized("Admin key required.");

            var need = _context.Needs.FirstOrDefault(n => n.Id == id);
            if (need == null) return NotFound();

            need.IsPublished = false;
            need.IsSuspicious = true;
            need.RiskNotes = (notes ?? "").Trim();

            StampReview(need, "Flagged");

            _context.SaveChanges();

            return RedirectBackOrDetails(id, key, returnUrl);
        }

        [HttpPost("SetEmailVerified")]
        [ValidateAntiForgeryToken]
        public IActionResult SetEmailVerified(int id, bool verified, string? key, string? returnUrl)
        {
            if (!IsAdmin(key)) return Unauthorized("Admin key required.");

            var need = _context.Needs.FirstOrDefault(n => n.Id == id);
            if (need == null) return NotFound();

            need.IsEmailVerified = verified;
            need.EmailVerifiedAtUtc = verified ? DateTime.UtcNow : null;
            need.VerifiedBy = ADMIN_NAME;
            need.VerifiedAtUtc = DateTime.UtcNow;

            if (verified && need.VerificationLevel == VerificationLevel.BasicContactVerified)
            {
                if (string.IsNullOrWhiteSpace(need.VerificationNote))
                    need.VerificationNote = "Email reviewed by admin.";
            }

            StampReview(need, verified ? "Email Verified" : "Email Not Verified");

            _context.SaveChanges();

            return RedirectBackOrDetails(id, key, returnUrl);
        }

        [HttpPost("SetPhoneVerified")]
        [ValidateAntiForgeryToken]
        public IActionResult SetPhoneVerified(int id, bool verified, string? key, string? returnUrl)
        {
            if (!IsAdmin(key)) return Unauthorized("Admin key required.");

            var need = _context.Needs.FirstOrDefault(n => n.Id == id);
            if (need == null) return NotFound();

            need.IsPhoneVerified = verified;
            need.VerifiedBy = ADMIN_NAME;
            need.VerifiedAtUtc = DateTime.UtcNow;

            StampReview(need, verified ? "Phone Verified" : "Phone Not Verified");

            _context.SaveChanges();

            return RedirectBackOrDetails(id, key, returnUrl);
        }

        [HttpPost("SaveInternalNotes")]
        [ValidateAntiForgeryToken]
        public IActionResult SaveInternalNotes(int id, string notes, string? key, string? returnUrl)
        {
            if (!IsAdmin(key)) return Unauthorized("Admin key required.");

            var need = _context.Needs.FirstOrDefault(n => n.Id == id);
            if (need == null) return NotFound();

            need.InternalReviewNotes = (notes ?? "").Trim();

            StampReview(need, string.IsNullOrWhiteSpace(need.InternalReviewStatus) ? "Pending" : need.InternalReviewStatus);

            _context.SaveChanges();

            return RedirectBackOrDetails(id, key, returnUrl);
        }

        [HttpPost("SetVerificationLevel")]
        [ValidateAntiForgeryToken]
        public IActionResult SetVerificationLevel(int id, VerificationLevel verificationLevel, string? note, string? key, string? returnUrl)
        {
            if (!IsAdmin(key)) return Unauthorized("Admin key required.");

            var need = _context.Needs.FirstOrDefault(n => n.Id == id);
            if (need == null) return NotFound();

            need.VerificationLevel = verificationLevel;

            if (!string.IsNullOrWhiteSpace(note))
                need.VerificationNote = note.Trim();

            need.VerifiedBy = ADMIN_NAME;
            need.VerifiedAtUtc = DateTime.UtcNow;

            StampReview(need, "Verification Updated");

            _context.SaveChanges();

            return RedirectBackOrDetails(id, key, returnUrl);
        }

        [HttpPost("UpdateVerification")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateVerification(
            int id,
            bool emailVerified,
            bool phoneVerified,
            string status,
            string riskNotes,
            string? internalNotes,
            string? key,
            string? returnUrl)
        {
            if (!IsAdmin(key)) return Unauthorized("Admin key required.");

            var need = _context.Needs.FirstOrDefault(n => n.Id == id);
            if (need == null) return NotFound();

            need.IsEmailVerified = emailVerified;
            need.IsPhoneVerified = phoneVerified;
            need.InternalReviewStatus = string.IsNullOrWhiteSpace(status) ? "Pending" : status.Trim();
            need.RiskNotes = (riskNotes ?? "").Trim();
            need.InternalReviewNotes = (internalNotes ?? "").Trim();
            need.IsSuspicious =
                need.InternalReviewStatus.Equals("Flagged", StringComparison.OrdinalIgnoreCase) ||
                !string.IsNullOrWhiteSpace(need.RiskNotes);

            need.EmailVerifiedAtUtc = emailVerified ? (need.EmailVerifiedAtUtc ?? DateTime.UtcNow) : null;

            need.VerifiedBy = ADMIN_NAME;
            need.VerifiedAtUtc = DateTime.UtcNow;

            StampReview(need, need.InternalReviewStatus);

            _context.SaveChanges();

            return RedirectBackOrDetails(id, key, returnUrl);
        }
    }
}