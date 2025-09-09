using Microsoft.AspNetCore.Mvc;
using InventoryManagementSystem.Models;
using System.Linq;

namespace InventoryManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IMSContext _context;

        public DashboardController(IMSContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // MTD KPI totals
            var totalProduction = _context.MTDProductions.Sum(m => m.TotalProduction);
            var totalSell = _context.MTDProductions.Sum(m => m.TotalSell);
            var totalWaste = _context.MTDProductions.Sum(m => m.TotalWaste);
            var totalLeftOver = _context.MTDProductions.Sum(m => m.TotalLeftOver);

            // Remaining Stock
            var remainingStocks = _context.RemainingStocks.ToList();

            ViewBag.TotalProduction = totalProduction;
            ViewBag.TotalSell = totalSell;
            ViewBag.TotalWaste = totalWaste;
            ViewBag.TotalLeftOver = totalLeftOver;
            ViewBag.RemainingStocks = remainingStocks;

            return View();
        }
    }
}
