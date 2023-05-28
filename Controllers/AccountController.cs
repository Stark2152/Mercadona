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

			// On vérifie si la connexion est bloquée
			string? blockedUntilString = HttpContext.Session.GetString("BlockedUntil");
			if (!string.IsNullOrEmpty(blockedUntilString))
			{
				DateTime blockedUntil = DateTime.Parse(blockedUntilString);
				if (blockedUntil > DateTime.UtcNow)
				{
					return Json(new { success = false, message = "Vous avez fait trop de tentatives infructueuses. Veuillez réessayer plus tard." });
				}
			}

			if (password == Password)
			{
				HttpContext.Session.SetString("Authenticated", "true");

				// On réinitialise le compteur des tentatives de connexion
				HttpContext.Session.SetInt32("FailedLoginAttempts", 0);

				return Json(new { success = true });
			}
			else
			{
				int failedLoginAttempts = HttpContext.Session.GetInt32("FailedLoginAttempts") ?? 0;

				// Increase failed login attempts count
				failedLoginAttempts++;
				HttpContext.Session.SetInt32("FailedLoginAttempts", failedLoginAttempts);

				if (failedLoginAttempts > 3) // 3 tentatives de connexion maximum
				{
					// On bloque la connexion pour 5 min
					HttpContext.Session.SetString("BlockedUntil", DateTime.UtcNow.AddMinutes(5).ToString());

					return Json(new { success = false, message = "Vous avez fait trop de tentatives infructueuses. Veuillez réessayer plus tard." });
				}
				else
				{
					return Json(new { success = false, message = "Le mot de passe est incorrect" });
				}
			}
		}

		public IActionResult Logout()
        {
            HttpContext.Session.Remove("Authenticated");
            return RedirectToAction("Index", "Products");
        }
    }
}
