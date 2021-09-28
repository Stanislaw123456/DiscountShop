#nullable enable
using DiscountStore.Modules.Discounts.ViewModels;
using DiscountStore.Modules.Products.ViewModels;

namespace DiscountStore.Modules.Checkout.ViewModels
{
    public class ItemViewModel
    {
        public ItemViewModel(ProductViewModel product, int quantity, DiscountViewModel? appliedDiscount)
        {
            Product = product;
            Quantity = quantity;
            AppliedDiscount = appliedDiscount;
        }

        public ProductViewModel Product { get; }
        public int Quantity { get; set; }
        public DiscountViewModel? AppliedDiscount { get; set; }

        public double SubTotal
        {
            get
            {
                if (AppliedDiscount == null)
                {
                    return Product.Price * Quantity;
                }

                return AppliedDiscount.DiscountedUnitPrice * Quantity;
            }
        }
    }
}