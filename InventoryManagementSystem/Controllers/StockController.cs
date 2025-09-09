using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Models;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace InventoryManagementSystem.Controllers
{
    public class StockController : Controller
    {
        private readonly IMSContext _context;

        public StockController(IMSContext context)
        {
            _context = context;
        }

        // ------------------ Index ------------------ //
        public async Task<IActionResult> Index()
        {
            var stocks = await _context.Stocks
                                       .OrderByDescending(s => s.Date)
                                       .ToListAsync();
            return View(stocks);
        }

        // ------------------ UploadStock (GET) ------------------ //
        public IActionResult UploadStock()
        {
            return View();
        }

        // ------------------ UploadStock (POST) ------------------ //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadStock(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                ViewBag.Message = "Please select a valid Excel file.";
                return View();
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        ViewBag.Message = "No worksheet found in Excel file.";
                        return View();
                    }

                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++) // skip header
                    {
                        string ingredientName = worksheet.Cells[row, 1].Text.Trim();
                        string stockUnitText = worksheet.Cells[row, 2].Text.Trim();
                        string dateText = worksheet.Cells[row, 3].Text.Trim();

                        if (string.IsNullOrEmpty(ingredientName) ||
                            string.IsNullOrEmpty(stockUnitText) ||
                            string.IsNullOrEmpty(dateText))
                            continue;

                        if (!int.TryParse(stockUnitText, out int stockUnit))
                            continue;

                        if (!DateTime.TryParse(dateText, out DateTime date))
                            continue;

                        var existing = await _context.Stocks
                            .FirstOrDefaultAsync(s => s.IngredientName == ingredientName && s.Date == date);

                        if (existing != null)
                        {
                            existing.StockUnit = stockUnit;
                            _context.Stocks.Update(existing);
                        }
                        else
                        {
                            var newStock = new Stock
                            {
                                IngredientName = ingredientName,
                                StockUnit = stockUnit,
                                Date = date
                            };
                            _context.Stocks.Add(newStock);
                        }
                    }

                    await _context.SaveChangesAsync();
                    UpdateRemainingStock(); // recalc remaining stock

                    ViewBag.Message = "Stock uploaded successfully!";
                    return View();
                }
            }
        }

        // ------------------ Edit GET ------------------ //
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null) return NotFound();

            return View(stock);
        }

        // ------------------ Edit POST ------------------ //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Stock stock)
        {
            if (id != stock.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stock);
                    await _context.SaveChangesAsync();
                    UpdateRemainingStock(); // recalc after edit
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Stocks.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(stock);
        }

        // ------------------ Delete GET ------------------ //
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == id);
            if (stock == null) return NotFound();

            return View(stock);
        }

        // ------------------ Delete POST ------------------ //
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stock = await _context.Stocks.FindAsync(id);
            if (stock != null)
            {
                _context.Stocks.Remove(stock);
                await _context.SaveChangesAsync();
                UpdateRemainingStock(); // recalc after delete
            }
            return RedirectToAction(nameof(Index));
        }

        // ------------------ Update RemainingStock (Cumulative) ------------------ //
        public void UpdateRemainingStock()
        {
            var ingredients = _context.Stocks
                                      .Select(s => s.IngredientName)
                                      .Distinct()
                                      .ToList();

            var itemRecipes = _context.ItemRecipes.AsEnumerable().ToList();
            var mtdProductions = _context.MTDProductions.AsEnumerable().ToList();

            foreach (var ingredient in ingredients)
            {
                var totalStock = _context.Stocks
                                         .Where(s => s.IngredientName == ingredient)
                                         .Sum(s => s.StockUnit);

                decimal usedUnit = 0m;

                var recipesForIngredient = itemRecipes
                                           .Where(r => r.IngredientName == ingredient)
                                           .ToList();

                foreach (var recipe in recipesForIngredient)
                {
                    var mtd = mtdProductions.FirstOrDefault(m => m.ItemName == recipe.ItemName);
                    decimal totalProduced = mtd != null ? mtd.TotalProduction : 0m;

                    if (recipe.QuantityGmPerPc > 0)
                        usedUnit += recipe.QuantityGmPerPc * totalProduced;
                    if (recipe.QuantityPcPerPc > 0)
                        usedUnit += recipe.QuantityPcPerPc * totalProduced;
                }

                var ingredientLeftOver = totalStock - usedUnit;

                var remaining = _context.RemainingStocks
                                        .FirstOrDefault(r => r.IngredientName == ingredient);

                if (remaining == null)
                {
                    remaining = new RemainingStock
                    {
                        IngredientName = ingredient,
                        Stock = totalStock,
                        UsedUnit = usedUnit,
                        IngredientLeftOver = ingredientLeftOver
                    };
                    _context.RemainingStocks.Add(remaining);
                }
                else
                {
                    remaining.Stock = totalStock;
                    remaining.UsedUnit = usedUnit;
                    remaining.IngredientLeftOver = ingredientLeftOver;
                    _context.RemainingStocks.Update(remaining);
                }
            }

            _context.SaveChanges();
        }
    }
}
