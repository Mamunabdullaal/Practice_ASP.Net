using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace InventoryManagementSystem.Controllers
{
    public class DatewiseProductionController : Controller
    {
        private readonly IMSContext _context;

        public DatewiseProductionController(IMSContext context)
        {
            _context = context;
        }

        // ------------------ Index ------------------ //
        public async Task<IActionResult> Index()
        {
            var productions = await _context.DatewiseProductions
                                            .OrderByDescending(d => d.Date)
                                            .ToListAsync();
            return View(productions);
        }

        // ------------------ Details ------------------ //
        public async Task<IActionResult> Details(int prodID)
        {
            var production = await _context.DatewiseProductions
                                           .FirstOrDefaultAsync(p => p.ProdID == prodID);
            if (production == null) return NotFound();
            return View(production);
        }

        // ------------------ Create (GET) ------------------ //
        public IActionResult Create()
        {
            return View();
        }

        // ------------------ Create (POST) ------------------ //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DatewiseProduction datewiseProduction)
        {
            if (ModelState.IsValid)
            {
                CalculateStockAndLeftOver(datewiseProduction);

                _context.DatewiseProductions.Add(datewiseProduction);
                await _context.SaveChangesAsync();

                UpdateMTDProduction(datewiseProduction.ItemName);
                UpdateRemainingStock();

                return RedirectToAction(nameof(Index));
            }
            return View(datewiseProduction);
        }

        // ------------------ Edit (GET) ------------------ //
        public async Task<IActionResult> Edit(int prodID)
        {
            var production = await _context.DatewiseProductions.FindAsync(prodID);
            if (production == null) return NotFound();
            return View(production);
        }

        // ------------------ Edit (POST) ------------------ //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int prodID, DatewiseProduction datewiseProduction)
        {
            if (prodID != datewiseProduction.ProdID) return NotFound();

            if (ModelState.IsValid)
            {
                CalculateStockAndLeftOver(datewiseProduction);

                _context.Update(datewiseProduction);
                await _context.SaveChangesAsync();

                UpdateMTDProduction(datewiseProduction.ItemName);
                UpdateRemainingStock();

                return RedirectToAction(nameof(Index));
            }
            return View(datewiseProduction);
        }

        // ------------------ Delete ------------------ //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int prodID)
        {
            var production = await _context.DatewiseProductions.FindAsync(prodID);
            if (production != null)
            {
                string itemName = production.ItemName;

                _context.DatewiseProductions.Remove(production);
                await _context.SaveChangesAsync();

                UpdateMTDProduction(itemName);
                UpdateRemainingStock();
            }
            return RedirectToAction(nameof(Index));
        }

        // ------------------ Calculate Stock and LeftOver ------------------ //
        private void CalculateStockAndLeftOver(DatewiseProduction datewiseProduction)
        {
            // Get previous date's LeftOver for this item
            var previous = _context.DatewiseProductions
                                   .Where(d => d.ItemName == datewiseProduction.ItemName && d.Date < datewiseProduction.Date)
                                   .OrderByDescending(d => d.Date)
                                   .FirstOrDefault();

            int previousLeftOver = previous?.LeftOver ?? 0;

            // Stock = Previous LeftOver + Current Production
            datewiseProduction.Stock = previousLeftOver + datewiseProduction.Production;

            // LeftOver = Stock - (Sell + Waste)
            datewiseProduction.LeftOver = datewiseProduction.Stock - (datewiseProduction.Sell + datewiseProduction.Waste);
        }

        // ------------------ Update MTDProduction ------------------ //
        private void UpdateMTDProduction(string itemName)
        {
            var allProductions = _context.DatewiseProductions
                                         .Where(d => d.ItemName == itemName)
                                         .OrderBy(d => d.Date)
                                         .ToList();

            int totalProduction = allProductions.Sum(d => d.Production);
            int totalSell = allProductions.Sum(d => d.Sell);
            int totalWaste = allProductions.Sum(d => d.Waste);
            int totalStock = allProductions.Sum(d => d.Stock);

            // TotalLeftOver = latest date's LeftOver
            int totalLeftOver = allProductions.LastOrDefault()?.LeftOver ?? 0;

            var mtd = _context.MTDProductions.FirstOrDefault(m => m.ItemName == itemName);
            if (mtd == null)
            {
                mtd = new MTDProduction
                {
                    ItemName = itemName,
                    TotalProduction = totalProduction,
                    TotalStock = totalStock,
                    TotalSell = totalSell,
                    TotalWaste = totalWaste,
                    TotalLeftOver = totalLeftOver
                };
                _context.MTDProductions.Add(mtd);
            }
            else
            {
                mtd.TotalProduction = totalProduction;
                mtd.TotalStock = totalStock;
                mtd.TotalSell = totalSell;
                mtd.TotalWaste = totalWaste;
                mtd.TotalLeftOver = totalLeftOver;
                _context.MTDProductions.Update(mtd);
            }

            _context.SaveChanges();
        }

        // ------------------ Update RemainingStock ------------------ //
        private void UpdateRemainingStock()
        {
            var ingredients = _context.Stocks.Select(s => s.IngredientName).Distinct().ToList();
            var itemRecipes = _context.ItemRecipes.AsEnumerable().ToList();
            var mtdProductions = _context.MTDProductions.AsEnumerable().ToList();

            foreach (var ingredient in ingredients)
            {
                var totalStock = _context.Stocks
                                         .Where(s => s.IngredientName == ingredient)
                                         .Sum(s => s.StockUnit);

                var usedUnit = (from recipe in itemRecipes
                                join mtd in mtdProductions
                                on recipe.ItemName equals mtd.ItemName into mtdJoin
                                from mtd in mtdJoin.DefaultIfEmpty()
                                where recipe.IngredientName == ingredient
                                select recipe.QuantityGmPerPc * (mtd != null ? mtd.TotalProduction : 0m))
                               .DefaultIfEmpty(0m)
                               .Sum();

                var ingredientLeftOver = totalStock - usedUnit;

                var remaining = _context.RemainingStocks.FirstOrDefault(r => r.IngredientName == ingredient);
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
