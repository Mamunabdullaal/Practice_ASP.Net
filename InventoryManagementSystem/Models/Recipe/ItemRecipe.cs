
namespace InventoryManagementSystem.Models
{
    public class ItemRecipe
    {
        public int ItemRecipeID { get; set; }
        public string ItemName { get; set; }
        public string IngredientName { get; set; }
        public decimal ProductionQuantity { get; set; }
        public decimal QuantityGmPerPc { get; set; }
        public decimal QuantityPcPerPc { get; set; }
    }
}

