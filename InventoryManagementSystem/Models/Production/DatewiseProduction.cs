using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class DatewiseProduction
    {
        [Key]
        public int ProdID { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        public int Production { get; set; }

        public int Stock { get; set; }

        [Required]
        public int Sell { get; set; }

        [Required]
        public int Waste { get; set; }

        public int LeftOver { get; set; }
    }
}
