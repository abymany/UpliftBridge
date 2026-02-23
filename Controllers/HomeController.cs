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

        // SUCCESS STORIES (LIST)
        public async Task<IActionResult> Stories()
        {
            ViewData["Title"] = "Success Stories";
            ViewData["BodyClass"] = "stories-body";

            var stories = await _db.Stories
                .Where(s => s.IsPublished)
                .OrderByDescending(s => s.PublishedUtc ?? s.CreatedUtc)
                .ToListAsync();

            return View(stories);
        }

        // SUCCESS STORIES (DETAIL)
        public async Task<IActionResult> StoryDetails(int id)
        {
            ViewData["Title"] = "Story";
            ViewData["BodyClass"] = "stories-body";

            var story = await _db.Stories
                .FirstOrDefaultAsync(s => s.Id == id && s.IsPublished);

            if (story == null) return NotFound();
            return View(story);
        }

        public IActionResult Verify() => View();
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
