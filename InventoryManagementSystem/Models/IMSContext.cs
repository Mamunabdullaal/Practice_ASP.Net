using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

public class IMSContext : DbContext
{
    public IMSContext(DbContextOptions<IMSContext> options) : base(options) { }

    public DbSet<DatewiseProduction> DatewiseProductions { get; set; }
    public DbSet<MTDProduction> MTDProductions { get; set; }
    public DbSet<ItemRecipe> ItemRecipes { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<RemainingStock> RemainingStocks { get; set; }
}
