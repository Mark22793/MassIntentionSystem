using Microsoft.AspNetCore.Mvc;

namespace MassIntentionSystem.Controllers
{
    public class AccountController : Controller
    {
        private const string HARDCODED_USER = "admin";
        private const string HARDCODED_PASS = "AdminPassword123!";

        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (username?.Trim().ToLower() == HARDCODED_USER && password == HARDCODED_PASS)
            {
                HttpContext.Session.SetString("AdminUser", "admin");
                HttpContext.Session.SetString("AdminRole", "SuperAdmin");
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Maling Username o Password.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}