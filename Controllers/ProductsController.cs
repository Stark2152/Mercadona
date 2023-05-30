using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Mercadona4.Context;
using Mercadona4.Models;
using System.Globalization;

namespace Mercadona4.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Products.Include(p => p.Promotion);
            return View(await applicationDbContext.ToListAsync());
        }

        public IActionResult GetProductsByCategory(string category)
        {
            var products = _context.Products
                .Where(p => string.IsNullOrEmpty(category) || p.Category == category)
                .Include(p => p.Promotion)
                .ToList();

            return Json(products);
        }

        // GET: Products/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetString("Authenticated") == "true")
            {
                var applicationDbContext = _context.Products.Include(p => p.Promotion);
                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Authenticated") == "true")
            {
                var promotions = _context.Promotions
                    .AsEnumerable()
                    .Where(p => p.EndDate > DateTime.UtcNow) // Seules les promotions non expirées
                    .OrderBy(p => p.StartDate)
                    .Select(p => new
                    {
                        p.Id,
                        DiscountDisplay = GeneratePromotionDisplay(p)
                    })
                    .ToList();

                ViewBag.PromotionId = new SelectList(promotions, "Id", "DiscountDisplay");
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Label,Description,Price,ImageUrl,Category,PromotionId")] Product product, IFormFile ImageUpload)
        {
            var validCategories = new List<string> { "Fruits", "Légumes", "Produits laitiers", "Surgelés" };
            var validFileTypes = new List<string> { "image/jpeg", "image/png" };

            if (product.Label.Length > 14 || product.Label.Length < 3)
            {
                ModelState.AddModelError("Label", "Le nom du produit doit comporter entre 3 et 14 caractères.");
            }

            if (product.Description.Length == 0)
            {
                ModelState.AddModelError("Description", "La description est obligatoire.");
            }

            if (product.Price < 0.01m || product.Price > 999.99m)
            {
                ModelState.AddModelError("Price", "Le prix doit être entre 0,01€ et 999,99€.");
            }

            if (ImageUpload != null)
            {
                var fileSize = ImageUpload.Length / 1024 / 1024; // size in MB
                if (fileSize > 5)
                {
                    ModelState.AddModelError("ImageUpload", "L'image ne doit pas dépasser 5Mo.");
                }
                else if (!validFileTypes.Contains(ImageUpload.ContentType))
                {
                    ModelState.AddModelError("ImageUpload", "Le format de l'image doit être .jpg, .jpeg ou .png.");
                }
            }

            if (!validCategories.Contains(product.Category))
            {
                ModelState.AddModelError("Category", "Veuillez sélectionner une catégorie valide.");
            }

            if (ModelState.IsValid && ImageUpload != null)
            {
                product.Id = _context.Products.DefaultIfEmpty().Max(p => p == null ? 0 : p.Id) + 1;

                var fileName = $"{product.Id}.jpg";
                var filePath = Path.Combine("wwwroot", "products_images", fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await ImageUpload.CopyToAsync(stream);
                }

                product.ImageUrl = "/products_images/" + fileName;

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Dashboard));
            }

            ViewData["PromotionId"] = new SelectList(_context.Promotions, "Id", "Id", product.PromotionId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("Authenticated") == "true")
            {
                if (id == null || _context.Products == null)
                {
                    return NotFound();
                }

                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                var now = DateTime.Now; // Get the current date and time
                var promotions = _context.Promotions
                             .Where(p => p.EndDate > now) // Filter out expired promotions
                             .AsEnumerable()
                             .OrderBy(p => p.StartDate)
                             .Select(p => new
                             {
                                 p.Id,
                                 DiscountDisplay = GeneratePromotionDisplay(p)
                             })
                             .ToList();

                ViewBag.PromotionId = new SelectList(promotions, "Id", "DiscountDisplay", product.PromotionId);
                return View(product);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        private static string GeneratePromotionDisplay(Promotion p)
        {
            string dateRange;
            var now = DateTime.Now;
            var remainingTime = p.EndDate - now;

            if (p.StartDate.Date == p.EndDate.Date)
            {
                dateRange = $"le {p.StartDate.ToString("d MMMM yyyy", new CultureInfo("fr-FR"))}";
            }
            else if (p.StartDate.Year == p.EndDate.Year && p.StartDate.Month == p.EndDate.Month)
            {
                dateRange = $"du {p.StartDate:dd} au {p.EndDate.ToString("d MMMM yyyy", new CultureInfo("fr-FR"))}";
            }
            else if (p.StartDate.Year == p.EndDate.Year)
            {
                dateRange = $"du {p.StartDate:d MMMM} au {p.EndDate.ToString("d MMMM yyyy", new CultureInfo("fr-FR"))}";
            }
            else
            {
                dateRange = $"du {p.StartDate.ToString("d MMMM yyyy", new CultureInfo("fr-FR"))} au {p.EndDate.ToString("d MMMM yyyy", new CultureInfo("fr-FR"))}";
            }

            string remainingDisplay = p.StartDate > now ? "" : remainingTime.TotalDays < 1 ? remainingTime.TotalHours < 1 ? $" - {remainingTime.Minutes} minutes restantes" : $" - {remainingTime.Hours} heures et {remainingTime.Minutes} minutes restantes" : remainingTime.TotalDays < 2 ? $" - 1 jour et {remainingTime.Hours} heures restantes" : $" - {remainingTime.Days} jours restant";

            return $"-{p.DiscountPercentage}% ({dateRange}{remainingDisplay})";
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Label,Description,Price,ImageUrl,Category,PromotionId")] Product product, IFormFile? ImageUpload)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            var validCategories = new List<string> { "Fruits", "Légumes", "Produits laitiers", "Surgelés" };
            var validFileTypes = new List<string> { "image/jpeg", "image/png" };

            if (product.Label.Length > 14 || product.Label.Length < 3)
            {
                ModelState.AddModelError("Label", "Le nom du produit doit comporter entre 3 et 14 caractères.");
            }

            if (product.Description.Length == 0)
            {
                ModelState.AddModelError("Description", "La description est obligatoire.");
            }

            if (product.Price < 0.01m || product.Price > 999.99m)
            {
                ModelState.AddModelError("Price", "Le prix doit être entre 0,01€ et 999,99€.");
            }

            if (ImageUpload != null)
            {
                var fileSize = ImageUpload.Length / 1024 / 1024; // size in MB
                if (fileSize > 5)
                {
                    ModelState.AddModelError("ImageUpload", "L'image ne doit pas dépasser 5Mo.");
                }
                else if (!validFileTypes.Contains(ImageUpload.ContentType))
                {
                    ModelState.AddModelError("ImageUpload", "Le format de l'image doit être .jpg, .jpeg ou .png.");
                }
            }

            if (!validCategories.Contains(product.Category))
            {
                ModelState.AddModelError("Category", "Veuillez sélectionner une catégorie valide.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var fileName = $"{product.Id}.jpg";
                    if (ImageUpload != null)
                    {
                        var filePath = Path.Combine("wwwroot", "products_images", fileName);
                        using (var stream = System.IO.File.Create(filePath))
                        {
                            await ImageUpload.CopyToAsync(stream);
                        }

                        product.ImageUrl = "/products_images/" + fileName;
                    }
                    else
                    {
                        product.ImageUrl = "/products_images/" + fileName;
                    }
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Dashboard));
            }
            ViewData["PromotionId"] = new SelectList(_context.Promotions, "Id", "Id", product.PromotionId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("Authenticated") == "true")
            {
                if (id == null || _context.Products == null)
                {
                    return NotFound();
                }

                var product = await _context.Products
                    .Include(p => p.Promotion)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (product == null)
                {
                    return NotFound();
                }

                return View(product);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Products'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                var fileName = $"{product.Id}.jpg";
                System.IO.File.Delete(Path.Combine("wwwroot", "products_images", fileName));
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Dashboard));
        }

        private bool ProductExists(int id)
        {
            return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
