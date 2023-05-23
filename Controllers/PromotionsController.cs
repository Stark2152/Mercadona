using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mercadona4.Context;
using Mercadona4.Models;

namespace Mercadona4.Controllers
{
    public class PromotionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PromotionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Promotions
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Authenticated") == "true")
            {
                return _context.Promotions != null ?
                        View(await _context.Promotions.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Promotions'  is null.");
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // GET: Promotions/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Authenticated") == "true")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // POST: Promotions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartDate,EndDate,DiscountPercentage")] Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(promotion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(promotion);
        }

        // GET: Promotions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("Authenticated") == "true")
            {
                if (id == null || _context.Promotions == null)
                {
                    return NotFound();
                }

                var promotion = await _context.Promotions.FindAsync(id);
                if (promotion == null)
                {
                    return NotFound();
                }
                return View(promotion);
            }
            else
            {
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
            return View(promotion);
        }

        // GET: Promotions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("Authenticated") == "true")
            {
                if (id == null || _context.Promotions == null)
                {
                    return NotFound();
                }

                var promotion = await _context.Promotions
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (promotion == null)
                {
                    return NotFound();
                }

                return View(promotion);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // POST: Promotions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Promotions == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Promotions'  is null.");
            }

            // Set PromotionId to null for all products using this promotion
            var productsWithPromotion = _context.Products.Where(p => p.PromotionId == id);
            foreach (var product in productsWithPromotion)
            {
                product.PromotionId = null;
            }
            await _context.SaveChangesAsync();

            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion != null)
            {
                _context.Promotions.Remove(promotion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PromotionExists(int id)
        {
            return (_context.Promotions?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
