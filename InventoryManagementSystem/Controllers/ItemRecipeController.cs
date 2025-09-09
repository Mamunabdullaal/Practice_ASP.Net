using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Models;
using System.Threading.Tasks;
using System.Linq;

namespace InventoryManagementSystem.Controllers
{
    public class ItemRecipeController : Controller
    {
        private readonly IMSContext _context;

        public ItemRecipeController(IMSContext context)
        {
            _context = context;
        }

        // ------------------ Index ------------------ //
        public async Task<IActionResult> Index()
        {
            var recipes = await _context.ItemRecipes
                                        .OrderBy(r => r.ItemName)
                                        .ThenBy(r => r.IngredientName)
                                        .ToListAsync();
            return View(recipes);
        }

        // ------------------ Details ------------------ //
        public async Task<IActionResult> Details(int id)
        {
            var recipe = await _context.ItemRecipes.FindAsync(id);
            if (recipe == null) return NotFound();
            return View(recipe);
        }

        // ------------------ Create (GET) ------------------ //
        public IActionResult Create()
        {
            return View();
        }

        // ------------------ Create (POST) ------------------ //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemRecipe recipe)
        {
            if (ModelState.IsValid)
            {
                _context.ItemRecipes.Add(recipe);
                await _context.SaveChangesAsync();

                // Update RemainingStock after adding recipe
                UpdateRemainingStock();

                return RedirectToAction(nameof(Index));
            }
            return View(recipe);
        }

        // ------------------ Edit (GET) ------------------ //
        public async Task<IActionResult> Edit(int id)
        {
            var recipe = await _context.ItemRecipes.FindAsync(id);
            if (recipe == null) return NotFound();
            return View(recipe);
        }

        // ------------------ Edit (POST) ------------------ //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemRecipe recipe)
        {
            if (id != recipe.ItemRecipeID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(recipe);
                await _context.SaveChangesAsync();

                // Update RemainingStock after editing
                UpdateRemainingStock();

                return RedirectToAction(nameof(Index));
            }
            return View(recipe);
        }

        // ------------------ Delete ------------------ //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var recipe = await _context.ItemRecipes.FindAsync(id);
            if (recipe != null)
            {
                _context.ItemRecipes.Remove(recipe);
                await _context.SaveChangesAsync();

                // Update RemainingStock after deleting
                UpdateRemainingStock();
            }
            return RedirectToAction(nameof(Index));
        }

        // ------------------ RemainingStock Update ------------------ //
        private void UpdateRemainingStock()
        {
            // Call your existing logic in StockController to recalc RemainingStock
            var stockController = new StockController(_context);
            stockController.UpdateRemainingStock();
        }
    }
}
