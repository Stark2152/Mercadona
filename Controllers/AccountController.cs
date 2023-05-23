using Microsoft.AspNetCore.Mvc;

namespace Mercadona4.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string password)
        {
            string Password = _configuration["AdminPassword"]!;

            if (password == Password)
            {
                HttpContext.Session.SetString("Authenticated", "true");
                return RedirectToAction("Dashboard", "Products");
            }
            else
            {
                ViewBag.Error = "Incorrect password";
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Authenticated");
            return RedirectToAction("Index", "Products");
        }
    }
}
