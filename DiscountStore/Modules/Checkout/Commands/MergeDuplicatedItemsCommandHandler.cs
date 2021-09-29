using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscountStore.Modules.Checkout.ViewModels;
using MediatR;

namespace DiscountStore.Modules.Checkout.Commands
{
    public record MergeDuplicatedItemsCommand(IEnumerable<ItemViewModel> ItemsToMerge)
        : IRequest<IEnumerable<ItemViewModel>>;
    
    public class MergeDuplicatedItemsCommandHandler : IRequestHandler<MergeDuplicatedItemsCommand, IEnumerable<ItemViewModel>>
    {
        public Task<IEnumerable<ItemViewModel>> Handle(MergeDuplicatedItemsCommand request, CancellationToken cancellationToken)
        {
            if (request.ItemsToMerge == null)
            {
                return Task.FromResult(Enumerable.Empty<ItemViewModel>());
            }

            var mergedItems = MergeDuplicatedItems(request.ItemsToMerge);

            return Task.FromResult(mergedItems);
        }
        
        private static IEnumerable<ItemViewModel> MergeDuplicatedItems(IEnumerable<ItemViewModel> itemsToMerge)
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
                    var mergedItem = new ItemViewModel(regularPricedItem.Product, regularPricedItem.Quantity, regularPricedItem.AppliedDiscount)
                    {
                        Quantity = regularPricedItems.Sum(item => item.Quantity)
                    };

                    yield return mergedItem;
                }
            }
        }
    }
}