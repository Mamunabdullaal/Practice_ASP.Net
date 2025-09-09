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

                // EPPlus 7.7.3 License
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

                    // Update RemainingStock safely
                    UpdateRemainingStock();

                    ViewBag.Message = "Stock uploaded successfully and RemainingStock updated!";
                    return View();
                }
            }
        }

        // ------------------ Update RemainingStock ------------------ //
        private void UpdateRemainingStock()
        {
            // Get distinct ingredients from Stocks table
            var ingredients = _context.Stocks
                                      .Select(s => s.IngredientName)
                                      .Distinct()
                                      .ToList();

            // Load data into memory to avoid LINQ-to-SQL translation issues
            var itemRecipes = _context.ItemRecipes.AsEnumerable().ToList();
            var mtdProductions = _context.MTDProductions.AsEnumerable().ToList();

            foreach (var ingredient in ingredients)
            {
                // Total Stock for this ingredient
                var totalStock = _context.Stocks
                                         .Where(s => s.IngredientName == ingredient)
                                         .Sum(s => s.StockUnit);

                // UsedUnit calculation in-memory
                var usedUnit = (from recipe in itemRecipes
                                join mtd in mtdProductions
                                on recipe.ItemName equals mtd.ItemName into mtdJoin
                                from mtd in mtdJoin.DefaultIfEmpty()
                                where recipe.IngredientName == ingredient
                                select recipe.QuantityGmPerPc * (mtd != null ? mtd.TotalProduction : 0m))
                               .DefaultIfEmpty(0m)
                               .Sum();

                var ingredientLeftOver = totalStock - usedUnit;

                // Update or insert RemainingStock
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
