using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DiscountStore.Modules.Discounts.Enums;
using DiscountStore.Modules.Products.Models;

namespace DiscountStore.Modules.Discounts.Models
{
    public class Discount
    {
        [Key]
        public int Id { get; set; }
        
        public DiscountType DiscountType { get; set; }
        
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        
        public Product Product { get; set; }
        
        public double DiscountedUnitPrice { get; set; }
    }
}