using DiscountStore.Modules.Discounts.Enums;

namespace DiscountStore.Modules.Discounts.ViewModels
{
    public class DiscountViewModel
    {
        public DiscountViewModel(DiscountType discountType, double discountedUnitPrice)
        {
            DiscountType = discountType;
            DiscountedUnitPrice = discountedUnitPrice;
        }

        public DiscountType DiscountType { get; }
        public double DiscountedUnitPrice { get; }
    }
}