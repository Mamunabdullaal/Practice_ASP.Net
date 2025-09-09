using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Models; // Adjust namespace
using System.Linq;
using System.Threading.Tasks;

public class DatewiseProductionController : Controller
{
    private readonly IMSContext _context;

    public DatewiseProductionController(IMSContext context)
    {
        _context = context;
    }

    // GET: DatewiseProduction
    public async Task<IActionResult> Index()
    {
        var productions = await _context.DatewiseProductions
            .OrderByDescending(d => d.Date)
            .ToListAsync();
        return View(productions);
    }

    // GET: DatewiseProduction/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var production = await _context.DatewiseProductions
            .FirstOrDefaultAsync(m => m.ProdID == id);
        if (production == null) return NotFound();

        return View(production);
    }

    // GET: DatewiseProduction/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: DatewiseProduction/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DatewiseProduction datewiseProduction)
    {
        if (ModelState.IsValid)
        {
            // Calculate Stock & LeftOver
            var previousLeftOver = _context.DatewiseProductions
                .Where(d => d.ItemName == datewiseProduction.ItemName && d.Date < datewiseProduction.Date)
                .OrderByDescending(d => d.Date)
                .Select(d => d.LeftOver)
                .FirstOrDefault();

            datewiseProduction.Stock = previousLeftOver + datewiseProduction.Production;
            datewiseProduction.LeftOver = datewiseProduction.Stock - (datewiseProduction.Sell + datewiseProduction.Waste);

            // Add record
            _context.Add(datewiseProduction);
            await _context.SaveChangesAsync();

            // Update MTD and RemainingStock
            UpdateMTDProduction(datewiseProduction.ItemName);
            UpdateRemainingStock();

            return RedirectToAction(nameof(Index));
        }
        return View(datewiseProduction);
    }

    // GET: DatewiseProduction/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var production = await _context.DatewiseProductions.FindAsync(id);
        if (production == null) return NotFound();

        return View(production);
    }

    // POST: DatewiseProduction/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DatewiseProduction datewiseProduction)
    {
        if (id != datewiseProduction.ProdID) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Recalculate Stock & LeftOver
                var previousLeftOver = _context.DatewiseProductions
                    .Where(d => d.ItemName == datewiseProduction.ItemName && d.Date < datewiseProduction.Date && d.ProdID != id)
                    .OrderByDescending(d => d.Date)
                    .Select(d => d.LeftOver)
                    .FirstOrDefault();

                datewiseProduction.Stock = previousLeftOver + datewiseProduction.Production;
                datewiseProduction.LeftOver = datewiseProduction.Stock - (datewiseProduction.Sell + datewiseProduction.Waste);

                _context.Update(datewiseProduction);
                await _context.SaveChangesAsync();

                // Update MTD & RemainingStock
                UpdateMTDProduction(datewiseProduction.ItemName);
                UpdateRemainingStock();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DatewiseProductionExists(datewiseProduction.ProdID))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(datewiseProduction);
    }

    // GET: DatewiseProduction/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var production = await _context.DatewiseProductions
            .FirstOrDefaultAsync(m => m.ProdID == id);
        if (production == null) return NotFound();

        return View(production);
    }

    // POST: DatewiseProduction/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var production = await _context.DatewiseProductions.FindAsync(id);
        if (production != null)
        {
            _context.DatewiseProductions.Remove(production);
            await _context.SaveChangesAsync();

            // Update MTD & RemainingStock
            UpdateMTDProduction(production.ItemName);
            UpdateRemainingStock();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool DatewiseProductionExists(int id)
    {
        return _context.DatewiseProductions.Any(e => e.ProdID == id);
    }

    // ---------------- Helper Methods ---------------- //

    private void UpdateMTDProduction(string itemName)
    {
        var datewiseRecords = _context.DatewiseProductions
            .Where(d => d.ItemName == itemName)
            .ToList();

        if (!datewiseRecords.Any())
        {
            var existingMTD = _context.MTDProductions.FirstOrDefault(m => m.ItemName == itemName);
            if (existingMTD != null)
            {
                _context.MTDProductions.Remove(existingMTD);
                _context.SaveChanges();
            }
            return;
        }

        var totalProduction = datewiseRecords.Sum(d => d.Production);
        var totalStock = datewiseRecords.Sum(d => d.Stock);
        var totalSell = datewiseRecords.Sum(d => d.Sell);
        var totalWaste = datewiseRecords.Sum(d => d.Waste);
        var totalLeftOver = totalStock - (totalSell + totalWaste);

        var mtdRecord = _context.MTDProductions.FirstOrDefault(m => m.ItemName == itemName);
        if (mtdRecord == null)
        {
            mtdRecord = new MTDProduction
            {
                ItemName = itemName,
                TotalProduction = totalProduction,
                TotalStock = totalStock,
                TotalSell = totalSell,
                TotalWaste = totalWaste,
                TotalLeftOver = totalLeftOver
            };
            _context.MTDProductions.Add(mtdRecord);
        }
        else
        {
            mtdRecord.TotalProduction = totalProduction;
            mtdRecord.TotalStock = totalStock;
            mtdRecord.TotalSell = totalSell;
            mtdRecord.TotalWaste = totalWaste;
            mtdRecord.TotalLeftOver = totalLeftOver;
            _context.MTDProductions.Update(mtdRecord);
        }
        _context.SaveChanges();
    }

    private void UpdateRemainingStock()
    {
        // Get all ingredients from Stock table
        var ingredients = _context.Stocks.Select(s => s.IngredientName).Distinct().ToList();

        foreach (var ingredient in ingredients)
        {
            // Get stock
            var stockValue = _context.Stocks
                .Where(s => s.IngredientName == ingredient)
                .Sum(s => s.StockUnit);

            // Calculate UsedUnit: sum of (ItemRecipe quantity * MTD Production)
            var usedUnit = (from recipe in _context.ItemRecipes
                            join mtd in _context.MTDProductions
                            on recipe.ItemName equals mtd.ItemName
                            where recipe.IngredientName == ingredient
                            select (recipe.QuantityGmPerPc * mtd.TotalProduction)).Sum();

            var ingredientLeftOver = stockValue - usedUnit;

            var remaining = _context.RemainingStocks.FirstOrDefault(r => r.IngredientName == ingredient);
            if (remaining == null)
            {
                remaining = new RemainingStock
                {
                    IngredientName = ingredient,
                    Stock = stockValue,
                    UsedUnit = usedUnit,
                    IngredientLeftOver = ingredientLeftOver
                };
                _context.RemainingStocks.Add(remaining);
            }
            else
            {
                remaining.Stock = stockValue;
                remaining.UsedUnit = usedUnit;
                remaining.IngredientLeftOver = ingredientLeftOver;
                _context.RemainingStocks.Update(remaining);
            }
        }
        _context.SaveChanges();
    }
}
