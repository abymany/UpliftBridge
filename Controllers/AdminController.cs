using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UpliftBridge.Data;

namespace Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _cfg;

        public AdminController(AppDbContext context, IConfiguration cfg)
        {
            _context = context;
            _cfg = cfg;
        }

        private bool IsValidKey(string? key)
        {
            var expected = (_cfg["Admin:Key"] ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(expected)) return false;

            var provided = (key ?? string.Empty).Trim();
            return string.Equals(provided, expected, StringComparison.Ordinal);
        }

        private IActionResult Reject()
            => Unauthorized("Missing or invalid admin key.");

        // GET: /Admin?key=Mani0751
        public IActionResult Index(string? key)
        {
            if (!IsValidKey(key)) return Reject();

            ViewBag.AdminKey = key;
            return View();
        }

        // GET: /Admin/Needs?key=Mani0751
        public IActionResult Needs(string? key)
        {
            if (!IsValidKey(key)) return Reject();

            ViewBag.AdminKey = key;

            var needs = _context.Needs
                .AsNoTracking()
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(needs);
        }

        // POST: /Admin/ApproveNeed?key=Mani0751
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveNeed(string? key, int id)
        {
            if (!IsValidKey(key)) return Reject();

            var need = _context.Needs.FirstOrDefault(n => n.Id == id);
            if (need == null) return NotFound("Need not found.");

            need.IsPublished = true;
            need.RejectionReason = string.Empty;
            need.ReviewedBy = "Admin";
            need.ReviewedAtUtc = DateTime.UtcNow;

            _context.SaveChanges();
            return RedirectToAction(nameof(Needs), new { key });
        }

        // POST: /Admin/RejectNeed?key=Mani0751
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectNeed(string? key, int id, string reason)
        {
            if (!IsValidKey(key)) return Reject();

            reason = (reason ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["AdminError"] = "Rejection reason is required.";
                return RedirectToAction(nameof(Needs), new { key });
            }

            var need = _context.Needs.FirstOrDefault(n => n.Id == id);
            if (need == null) return NotFound("Need not found.");

            need.IsPublished = false;
            need.RejectionReason = reason;
            need.ReviewedBy = "Admin";
            need.ReviewedAtUtc = DateTime.UtcNow;

            _context.SaveChanges();
            return RedirectToAction(nameof(Needs), new { key });
        }

        // GET: /Admin/Pledges?key=Mani0751
        public IActionResult Pledges(string? key)
        {
            if (!IsValidKey(key)) return Reject();

            ViewBag.AdminKey = key;

            var pledges = _context.Pledges
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return View(pledges);
        }

        // GET: /Admin/Updates?key=Mani0751
        public IActionResult Updates(string? key)
        {
            if (!IsValidKey(key)) return Reject();

            ViewBag.AdminKey = key;

            var updates = _context.NeedUpdates
                .AsNoTracking()
                .Include(u => u.Need)
                .OrderByDescending(u => u.CreatedAtUtc)
                .ToList();

            return View(updates);
        }

        // POST: /Admin/ToggleUpdateVisibility?key=Mani0751
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleUpdateVisibility(string? key, int id)
        {
            if (!IsValidKey(key)) return Reject();

            var u = _context.NeedUpdates.FirstOrDefault(x => x.Id == id);
            if (u == null) return NotFound("Update not found.");

            u.IsVisible = !u.IsVisible;
            _context.SaveChanges();

            return RedirectToAction(nameof(Updates), new { key });
        }
    }
}
