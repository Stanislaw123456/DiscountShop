using System.ComponentModel.DataAnnotations;

namespace DiscountStore.Modules.Discounts.Enums
{
    public enum DiscountType
    {
        [Display(Name = "Two for X")]
        TwoForX = 1,
        [Display(Name = "Three for X")]
        ThreeForX = 2
    }
}