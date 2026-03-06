using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpliftBridge.Data;
using UpliftBridge.Models;

namespace UpliftBridge.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        // -----------------------------
        // SUCCESS STORIES (LIST)
        // URLs supported:
        //   /Home/Stories
        //   /Stories            (alias)
        // -----------------------------
        [HttpGet("/Stories")]
        public async Task<IActionResult> Stories()
        {
            ViewData["Title"] = "Success Stories";
            ViewData["BodyClass"] = "stories-body";

            // IMPORTANT:
            // This assumes Story.IsPublished is bool in C# AND the DB column is boolean.
            // If your DB column is currently int(0/1), fix the DB in Step 2 below.
            var stories = await _db.Stories
                .Where(s => s.IsPublished)
                .OrderByDescending(s => s.PublishedUtc ?? s.CreatedUtc)
                .ToListAsync();

            return View(stories);
        }

        // -----------------------------
        // SUCCESS STORIES (DETAIL)
        // URLs supported:
        //   /Home/StoryDetails/5
        //   /Stories/5          (alias)
        // -----------------------------
        [HttpGet("/Stories/{id:int}")]
        public async Task<IActionResult> StoryDetails(int id)
        {
            ViewData["Title"] = "Story";
            ViewData["BodyClass"] = "stories-body";

            var story = await _db.Stories
                .FirstOrDefaultAsync(s => s.Id == id && s.IsPublished);

            if (story == null) return NotFound();
            return View(story);
        }

        // -----------------------------
        // VERIFICATION PAGE
        // URLs supported:
        //   /Home/Verify
        //   /Home/Verification   (alias)
        //   /Verify              (alias)
        // -----------------------------
        [HttpGet("/Verify")]
        [HttpGet("/Home/Verification")]
        public IActionResult Verify() => View();

        // Keep your existing pages
        public IActionResult About() => View();
        public IActionResult Privacy() => View();
        public IActionResult Faq() => View();
        public IActionResult Schools() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}