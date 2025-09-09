using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Controllers
{
    public class RemainingStockController : Controller
    {
        private readonly IMSContext _context;

        public RemainingStockController(IMSContext context)
        {
            _context = context;
        }

        // GET: RemainingStock
        public async Task<IActionResult> Index()
        {
            var data = await _context.RemainingStocks
                                     .OrderBy(r => r.IngredientName)
                                     .ToListAsync();
            return View(data);
        }
    }
}
