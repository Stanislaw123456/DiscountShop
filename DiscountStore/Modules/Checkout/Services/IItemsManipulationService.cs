using System.Collections.Generic;
using DiscountStore.Modules.Checkout.ViewModels;

namespace DiscountStore.Modules.Checkout.Services
{
    public interface IItemsManipulationService
    {
        IEnumerable<ItemViewModel> MergeDuplicatesItems(IEnumerable<ItemViewModel> itemsToMerge);
    }
}