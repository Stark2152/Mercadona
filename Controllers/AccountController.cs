using Microsoft.AspNetCore.Mvc;

namespace Mercadona4.Controllers
{
    // Contrôleur pour la gestion du compte administrateur
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        // Constructeur qui prend en paramètre l'objet de configuration
        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Action pour afficher la page de connexion
        public IActionResult Login()
        {
            return View();
        }

        // Action pour traiter la demande de connexion
        [HttpPost]
        public IActionResult Login(string password)
        {
            // Récupération du mot de passe dans la configuration
            string Password = _configuration["AdminPassword"]!;

            // Récupération de la date jusqu'à laquelle la connexion est bloquée
            string? blockedUntilString = HttpContext.Session.GetString("BlockedUntil");
            if (!string.IsNullOrEmpty(blockedUntilString))
            {
                DateTime blockedUntil = DateTime.Parse(blockedUntilString);
                // Si le blocage est actif, retourne un message d'erreur
                if (blockedUntil > DateTime.UtcNow)
                {
                    return Json(new { success = false, message = "Vous avez fait trop de tentatives infructueuses. Veuillez réessayer plus tard." });
                }
            }

            // Si le mot de passe est correct
            if (password == Password)
            {
                // Authentifie l'utilisateur
                HttpContext.Session.SetString("Authenticated", "true");

                // Réinitialise le compteur de tentatives de connexion échouées
                HttpContext.Session.SetInt32("FailedLoginAttempts", 0);

                // Redirige vers la page de succès
                return Json(new { success = true });
            }
            else
            {
                // Récupère le nombre de tentatives de connexion échouées
                int failedLoginAttempts = HttpContext.Session.GetInt32("FailedLoginAttempts") ?? 0;

                // Incrémente le compteur de tentatives échouées
                failedLoginAttempts++;
                HttpContext.Session.SetInt32("FailedLoginAttempts", failedLoginAttempts);

                // Si l'utilisateur a échoué plus de 3 fois
                if (failedLoginAttempts > 3)
                {
                    // Bloque la connexion pour 5 min
                    HttpContext.Session.SetString("BlockedUntil", DateTime.UtcNow.AddMinutes(5).ToString());

                    // Retourne un message d'erreur
                    return Json(new { success = false, message = "Vous avez fait trop de tentatives infructueuses. Veuillez réessayer plus tard." });
                }
                else
                {
                    // Retourne un message indiquant que le mot de passe est incorrect
                    return Json(new { success = false, message = "Le mot de passe est incorrect" });
                }
            }
        }

        // Action pour déconnecter l'utilisateur
        public IActionResult Logout()
        {
            // Supprime le statut d'authentification de l'utilisateur
            HttpContext.Session.Remove("Authenticated");
            // Redirige vers la page d'accueil des produits
            return RedirectToAction("Index", "Products");
        }
    }
}
