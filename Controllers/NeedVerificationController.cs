using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using UpliftBridge.Data;

namespace UpliftBridge.Controllers
{
    [Route("Needs/Verify")]
    public class NeedVerificationController : Controller
    {
        private readonly AppDbContext _context;

        public NeedVerificationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id:int}")]
        public IActionResult Index(int id)
        {
            var need = _context.Needs.FirstOrDefault(x => x.Id == id);
            if (need == null) return NotFound();

            ViewBag.NeedId = id;
            ViewBag.Email = need.RequesterEmail;
            return View();
        }

        [HttpPost("SendOtp")]
        [ValidateAntiForgeryToken]
        public IActionResult SendOtp(int id)
        {
            var need = _context.Needs.FirstOrDefault(x => x.Id == id);
            if (need == null) return NotFound();

            var otp = new Random().Next(100000, 999999).ToString();

            need.EmailOtpCode = otp;
            need.EmailOtpExpiresAtUtc = DateTime.UtcNow.AddMinutes(10);

            _context.SaveChanges();

            TempData["OtpMessage"] = $"DEV OTP: {otp}";
            return RedirectToAction(nameof(Index), new { id });
        }

        [HttpPost("ConfirmOtp")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmOtp(int id, string otp)
        {
            var need = _context.Needs.FirstOrDefault(x => x.Id == id);
            if (need == null) return NotFound();

            if (string.IsNullOrWhiteSpace(need.EmailOtpCode) ||
                !need.EmailOtpExpiresAtUtc.HasValue ||
                need.EmailOtpExpiresAtUtc.Value < DateTime.UtcNow)
            {
                TempData["OtpError"] = "OTP expired. Please request a new code.";
                return RedirectToAction(nameof(Index), new { id });
            }

            if (!string.Equals(need.EmailOtpCode, otp?.Trim(), StringComparison.Ordinal))
            {
                TempData["OtpError"] = "Invalid OTP.";
                return RedirectToAction(nameof(Index), new { id });
            }

            need.IsEmailVerified = true;
            need.EmailVerifiedAtUtc = DateTime.UtcNow;
            need.EmailOtpCode = string.Empty;
            need.EmailOtpExpiresAtUtc = null;

            if (string.IsNullOrWhiteSpace(need.VerificationNote))
                need.VerificationNote = "Email verified by OTP.";

            _context.SaveChanges();

            TempData["OtpSuccess"] = "Email verified successfully.";
            return RedirectToAction(nameof(Index), new { id });
        }
    }
}