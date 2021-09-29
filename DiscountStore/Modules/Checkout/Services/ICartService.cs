using System.Collections.Generic;
using DiscountStore.Modules.Checkout.ViewModels;

namespace DiscountStore.Modules.Checkout.Services
{
    public interface ICartService
    {
        double GetTotal(IEnumerable<ItemViewModel> cart);
        IEnumerable<ItemViewModel> MergeDuplicatedItems(IEnumerable<ItemViewModel> itemsToMerge);
    }
}