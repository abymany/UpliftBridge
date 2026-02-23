using System;
using System.Linq;
using System.Threading.Tasks;
using UpliftBridge.Data;
using UpliftBridge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace UpliftBridge.Controllers
{
    public class AdminStoriesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _cfg;

        public AdminStoriesController(AppDbContext db, IConfiguration cfg)
        {
            _db = db;
            _cfg = cfg;
        }

        private bool IsValidKey(string? key)
        {
            var expected = (_cfg["Admin:Key"] ?? "").Trim();
            if (string.IsNullOrWhiteSpace(expected)) return false;

            var provided = (key ?? "").Trim();
            return string.Equals(provided, expected, StringComparison.Ordinal);
        }

        private IActionResult Deny() => Unauthorized("Missing or invalid admin key.");

        // GET: /AdminStories?key=Mani0751
        public async Task<IActionResult> Index(string? key)
        {
            if (!IsValidKey(key)) return Deny();

            ViewBag.AdminKey = key; // ✅ makes _AdminLinks work everywhere
            ViewData["Title"] = "Admin — Stories";

            var items = await _db.Stories
                .OrderByDescending(s => s.CreatedUtc)
                .ToListAsync();

            return View(items);
        }

        // GET: /AdminStories/Create?key=Mani0751
        public IActionResult Create(string? key)
        {
            if (!IsValidKey(key)) return Deny();

            ViewBag.AdminKey = key;
            ViewData["Title"] = "Admin — Create Story";

            return View(new Story());
        }

        // POST: /AdminStories/Create?key=Mani0751
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string? key, Story model)
        {
            if (!IsValidKey(key)) return Deny();

            ViewBag.AdminKey = key;
            ViewData["Title"] = "Admin — Create Story";

            if (!ModelState.IsValid)
                return View(model);

            model.CreatedUtc = DateTime.UtcNow;
            model.IsPublished = false;
            model.PublishedUtc = null;

            _db.Stories.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { key });
        }

        // GET: /AdminStories/Edit/5?key=Mani0751
        public async Task<IActionResult> Edit(int id, string? key)
        {
            if (!IsValidKey(key)) return Deny();

            ViewBag.AdminKey = key;
            ViewData["Title"] = "Admin — Edit Story";

            var story = await _db.Stories.FirstOrDefaultAsync(s => s.Id == id);
            if (story == null) return NotFound();

            return View(story);
        }

        // POST: /AdminStories/Edit/5?key=Mani0751
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string? key, Story model)
        {
            if (!IsValidKey(key)) return Deny();
            if (id != model.Id) return BadRequest();

            ViewBag.AdminKey = key;
            ViewData["Title"] = "Admin — Edit Story";

            if (!ModelState.IsValid)
                return View(model);

            var existing = await _db.Stories.FirstOrDefaultAsync(s => s.Id == id);
            if (existing == null) return NotFound();

            existing.Title = model.Title;
            existing.Category = model.Category;
            existing.Location = model.Location;
            existing.VerificationLabel = model.VerificationLabel;
            existing.Tagline = model.Tagline;
            existing.ShortSummary = model.ShortSummary;
            existing.CoverImagePath = model.CoverImagePath;
            existing.GalleryImagePaths = model.GalleryImagePaths;
            existing.Body = model.Body;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { key });
        }

        // POST: /AdminStories/TogglePublish/5?key=Mani0751
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(int id, string? key)
        {
            if (!IsValidKey(key)) return Deny();

            var story = await _db.Stories.FirstOrDefaultAsync(s => s.Id == id);
            if (story == null) return NotFound();

            story.IsPublished = !story.IsPublished;
            story.PublishedUtc = story.IsPublished ? DateTime.UtcNow : null;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { key });
        }
    }
}
