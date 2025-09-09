using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class Stock
    {
        [Key]
        public int Id { get; set; } // <-- keep this

        [Required]
        public string IngredientName { get; set; }

        [Required]
        public int StockUnit { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}
