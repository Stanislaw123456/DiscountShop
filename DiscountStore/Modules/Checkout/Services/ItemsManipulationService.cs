using System.Collections.Generic;
using System.Linq;
using DiscountStore.Modules.Checkout.ViewModels;

namespace DiscountStore.Modules.Checkout.Services
{
    public class ItemsManipulationService : IItemsManipulationService
    {
        public IEnumerable<ItemViewModel> MergeDuplicatesItems(IEnumerable<ItemViewModel> itemsToMerge)
        {
            if (itemsToMerge == null)
            {
                yield break;
            }
            
            foreach (var itemGroup in itemsToMerge.GroupBy(item => item.Product.Id))
            {
                var discountedItems = itemGroup.Where(item => item.AppliedDiscount != null).ToList();
                if (discountedItems.Any())
                {
                    var discountedItem = discountedItems.First();
                    var mergedItem = new ItemViewModel(discountedItem.Product, discountedItem.Quantity,
                        discountedItem.AppliedDiscount)
                    {
                        Quantity = discountedItems.Sum(item => item.Quantity)
                    };

                    yield return mergedItem;
                }
                
                var regularPricedItems = itemGroup.Where(item => item.AppliedDiscount == null).ToList();
                if (regularPricedItems.Any())
                {
                    var regularPricedItem = regularPricedItems.First();
                    var mergedItem = new ItemViewModel(regularPricedItem.Product, regularPricedItem.Quantity,
                        regularPricedItem.AppliedDiscount)
                    {
                        Quantity = regularPricedItems.Sum(item => item.Quantity)
                    };
                    
                    yield return mergedItem;
                }
            }
        }
    }
}