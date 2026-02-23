using System.Linq;
using Microsoft.AspNetCore.Mvc;
using UpliftBridge.Data;

namespace UpliftBridge.Controllers
{
    public class AdminPledgesController : Controller
    {
        private readonly AppDbContext _context;

        // Demo admin key. Later move to appsettings.json / env var.
        private const string ADMIN_KEY = "Mani0751";

        public AdminPledgesController(AppDbContext context) { _context = context; }

        private bool IsAdmin(string key) => !string.IsNullOrWhiteSpace(key) && key == ADMIN_KEY;

        // /AdminPledges?key=Mani0751
        public IActionResult Index(string key)
        {
            if (!IsAdmin(key)) return Unauthorized("Invalid admin key.");

            var pledges = _context.Pledges
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return View("~/Views/Admin/Pledges.cshtml", pledges);
        }
    }
}
