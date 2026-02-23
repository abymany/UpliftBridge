using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpliftBridge.Data;

namespace UpliftBridge.Controllers
{
    [Route("Admin/Needs")]
    public class AdminNeedsController : Controller
    {
        private readonly AppDbContext _context;

        // TODO: move to config later
        private const string ADMIN_KEY = "Mani0751";

        public AdminNeedsController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin(string? key)
        {
            // allow setting session via query key once
            if (!string.IsNullOrWhiteSpace(key) && key == ADMIN_KEY)
            {
                HttpContext.Session.SetString("kg_admin", "1");
                return true;
            }

            // otherwise require session
            return HttpContext.Session.GetString("kg_admin") == "1";
        }

        [HttpGet("")]
        public IActionResult Index(string? key)
        {
            if (!IsAdmin(key)) return Unauthorized("Admin key required.");

            var needs = _context.Needs
                .AsNoTracking()
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(needs);
        }

        // OPTIONAL: keep "See" URL if your button uses asp-action="See"
        [HttpGet("See/{id:int}")]
        public IActionResult See(int id, string? key) => Details(id, key);

        // Admin preview for pending + published
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
                .Where(p => p.NeedId == id)
                .OrderByDescending(p => p.CreatedAtUtc)
                .Select(p => p.Path)
                .ToList();

            ViewBag.AdminKey = key ?? "";
            ViewBag.Updates = updates;
            ViewBag.Photos = photos;

            return View(need);
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
            need.ReviewedBy = "Admin";
            need.ReviewedAtUtc = DateTime.UtcNow;

            _context.SaveChanges();

            // ✅ stay on preview page if provided
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index), new { key });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id, string reason, string? key, string? returnUrl)
        {
            if (!IsAdmin(key)) return Unauthorized("Admin key required.");

           
                if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 10)
                return BadRequest("Please enter a clear rejection reason (at least 10 characters).");
                if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 12)
                return BadRequest("Write a clear rejection reason (at least 12 characters).");


            var need = _context.Needs.FirstOrDefault(n => n.Id == id);
            if (need == null) return NotFound();

            need.IsPublished = false;
            need.RejectionReason = reason.Trim();
            need.ReviewedBy = "Admin";
            need.ReviewedAtUtc = DateTime.UtcNow;

            _context.SaveChanges();

            // ✅ stay on preview page if provided
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index), new { key });
        }
    }
}