using System.Linq;
using Microsoft.AspNetCore.Mvc;
using UpliftBridge.Data;
using Microsoft.EntityFrameworkCore;

namespace UpliftBridge.Controllers
{
    public class AdminUpdatesController : Controller
    {
        private readonly AppDbContext _context;

        // Demo admin key. Later move to appsettings.json / env var.
        private const string ADMIN_KEY = "Mani0751";

        public AdminUpdatesController(AppDbContext context) { _context = context; }

        private bool IsAdmin(string key) => !string.IsNullOrWhiteSpace(key) && key == ADMIN_KEY;

        public IActionResult Index(string key)
        {
            if (!IsAdmin(key)) return Unauthorized("Invalid admin key.");

            var updates = _context.NeedUpdates
                .Include(u => u.Need)
                .OrderByDescending(u => u.CreatedAtUtc)
                .ToList();

            return View("~/Views/Admin/Updates.cshtml", updates);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleVisibility(int id, string key)
        {
            if (!IsAdmin(key)) return Unauthorized("Invalid admin key.");

            var u = _context.NeedUpdates.FirstOrDefault(x => x.Id == id);
            if (u == null) return NotFound();

            u.IsVisible = !u.IsVisible;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index), new { key });
        }
    }
}
