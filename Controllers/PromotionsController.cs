using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mercadona4.Context;
using Mercadona4.Models;

namespace Mercadona4.Controllers
{
    // Contrôleur pour gérer les promotions
    public class PromotionsController : Controller
    {
        // Injection de dépendances pour le contexte de base de données de l'application
        private readonly ApplicationDbContext _context;

        // Constructeur pour initialiser le contexte de base de données
        public PromotionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Promotions
        public async Task<IActionResult> Index()
        {
            // Vérification si l'utilisateur est authentifié
            if (HttpContext.Session.GetString("Authenticated") == "true")
            {
                // Si l'utilisateur est authentifié, récupération et affichage de toutes les promotions
                // Si _context.Promotions est null, renvoie un message d'erreur
                return _context.Promotions != null ?
                        View(await _context.Promotions.ToListAsync()) :
                        Problem("L'ensemble d'entités 'ApplicationDbContext.Promotions' est null.");
            }
            else
            {
                // Si l'utilisateur n'est pas authentifié, redirection vers la page de connexion
                return RedirectToAction("Login", "Account");
            }
        }

        // GET: Promotions/Create
        public IActionResult Create()
        {
            // Vérification si l'utilisateur est authentifié
            if (HttpContext.Session.GetString("Authenticated") == "true")
            {
                // Si l'utilisateur est authentifié, affichage de la page de création de promotion
                return View();
            }
            else
            {
                // Si l'utilisateur n'est pas authentifié, redirection vers la page de connexion
                return RedirectToAction("Login", "Account");
            }
        }

        // POST: Promotions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartDate,EndDate,DiscountPercentage")] Promotion promotion)
        {
            // Vérification de la validité des données du formulaire
            if (ModelState.IsValid)
            {
                var now = DateTime.Now.Date;
                if (promotion.StartDate < now)
                {
                    ModelState.AddModelError("StartDate", "La date de début ne peut pas être avant la date actuelle.");
                }

                if (promotion.StartDate >= promotion.EndDate)
                {
                    ModelState.AddModelError("EndDate", "La date de fin ne peut pas précéder la date de début.");
                }

                if (promotion.DiscountPercentage < 5 || promotion.DiscountPercentage > 90)
                {
                    ModelState.AddModelError("DiscountPercentage", "Le pourcentage de réduction doit être situé entre 5 et 90%.");
                }

                // Si les données sont valides, enregistrement de la nouvelle promotion et redirection vers la page des promotions
                if (ModelState.IsValid)
                {
                    promotion.Id = _context.Promotions.DefaultIfEmpty().Max(p => p == null ? 0 : p.Id) + 1;

                    _context.Add(promotion);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            // Si les données ne sont pas valides, renvoi à la page de création avec les erreurs de validation
            return View(promotion);
        }

        // GET: Promotions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Vérification si l'utilisateur est authentifié
            if (HttpContext.Session.GetString("Authenticated") == "true")
            {
                // Vérification de l'existence de la promotion
                if (id == null || _context.Promotions == null)
                {
                    return NotFound();
                }

                // Récupération de la promotion à modifier
                var promotion = await _context.Promotions.FindAsync(id);
                if (promotion == null)
                {
                    return NotFound();
                }
                // Affichage de la page d'édition de la promotion
                return View(promotion);
            }
            else
            {
                // Si l'utilisateur n'est pas authentifié, redirection vers la page de connexion
                return RedirectToAction("Login", "Account");
            }
        }

        // POST: Promotions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StartDate,EndDate,DiscountPercentage")] Promotion promotion)
        {
            if (id != promotion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var now = DateTime.Now.Date;
                if (promotion.StartDate < now)
                {
                    ModelState.AddModelError("StartDate", "La date de début ne peut pas être avant la date actuelle.");
                }

                if (promotion.StartDate >= promotion.EndDate)
                {
                    ModelState.AddModelError("EndDate", "La date de fin ne peut pas précéder la date de début.");
                }

                if (promotion.DiscountPercentage < 5 || promotion.DiscountPercentage > 90)
                {
                    ModelState.AddModelError("DiscountPercentage", "Le pourcentage de réduction doit être situé entre 5 et 90%.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(promotion);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!PromotionExists(promotion.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
            }
            return View(promotion);
        }

        // GET: Promotions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Vérification si l'utilisateur est authentifié
            if (HttpContext.Session.GetString("Authenticated") == "true")
            {
                // Vérification de l'existence de la promotion
                if (id == null || _context.Promotions == null)
                {
                    return NotFound();
                }

                // Récupération de la promotion à supprimer
                var promotion = await _context.Promotions
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (promotion == null)
                {
                    return NotFound();
                }

                // Affichage de la page de confirmation de suppression de la promotion
                return View(promotion);
            }
            else
            {
                // Si l'utilisateur n'est pas authentifié, redirection vers la page de connexion
                return RedirectToAction("Login", "Account");
            }
        }

        // POST: Promotions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Vérification de l'existence de l'ensemble de promotions
            if (_context.Promotions == null)
            {
                return Problem("L'ensemble d'entités 'ApplicationDbContext.Promotions' est null.");
            }

            // Suppression de la référence à cette promotion pour tous les produits qui l'utilisent
            var productsWithPromotion = _context.Products.Where(p => p.PromotionId == id);
            foreach (var product in productsWithPromotion)
            {
                product.PromotionId = null;
            }
            await _context.SaveChangesAsync();

            // Suppression de la promotion
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion != null)
            {
                _context.Promotions.Remove(promotion);
            }

            // Sauvegarde des changements dans la base de données
            await _context.SaveChangesAsync();
            // Redirection vers la page des promotions
            return RedirectToAction(nameof(Index));
        }

        // Méthode privée pour vérifier l'existence d'une promotion dans la base de données
        private bool PromotionExists(int id)
        {
            // Vérification si l'ensemble des promotions est null
            // Si non, vérification de l'existence de la promotion
            return (_context.Promotions?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
