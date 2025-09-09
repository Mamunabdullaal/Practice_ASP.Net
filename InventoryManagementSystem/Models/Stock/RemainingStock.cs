namespace InventoryManagementSystem.Models
{
    public class RemainingStock
    {
        public int RemainingStockID { get; set; }
        public string IngredientName { get; set; }
        public decimal Stock { get; set; }
        public decimal UsedUnit { get; set; }
        public decimal IngredientLeftOver { get; set; }
    }
}

