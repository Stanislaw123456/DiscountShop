using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DiscountStore.Modules.Checkout.ViewModels;
using DiscountStore.Modules.Discounts.Enums;
using DiscountStore.Modules.Discounts.Models;
using DiscountStore.Modules.Discounts.Queries;
using DiscountStore.Modules.Discounts.ViewModels;
using MediatR;

namespace DiscountStore.Modules.Discounts.Providers
{
    public class TwoForXDiscountTypeProvider : IDiscountProvider
    {
        private readonly IMediator _mediator;

        public TwoForXDiscountTypeProvider(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async IAsyncEnumerable<ItemViewModel> ApplyDiscountsAsync(ICollection<ItemViewModel> items,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (items == null || !items.Any())
            {
                yield break;
            }

            var query = new FindDiscountsByDiscountTypeQuery(DiscountType.TwoForX);
            var discounts = (await _mediator.Send(query, cancellationToken)).ToList();

            if (!discounts.Any())
            {
                foreach (var item in items)
                {
                    yield return item;
                }

                yield break;
            }

            foreach (var item in items)
            {
                var possibleDiscount = discounts.FirstOrDefault(discount => discount.ProductId == item.Product.Id);
                await foreach (var itemViewModel in TryApplyDiscountAsync(possibleDiscount, item)
                    .WithCancellation(cancellationToken))
                {
                    yield return itemViewModel;
                }
            }
        }

        private static async IAsyncEnumerable<ItemViewModel> TryApplyDiscountAsync(Discount possibleDiscount, ItemViewModel item)
        {
            if (possibleDiscount == null || item.Quantity < 2)
            {
                yield return item;
                yield break;
            }

            if (item.Quantity % 2 == 0)
            {
                item.AppliedDiscount =
                    new DiscountViewModel(possibleDiscount.DiscountType, possibleDiscount.DiscountedUnitPrice);

                yield return item;
            }
            else
            {
                item.Quantity--;
                item.AppliedDiscount =
                    new DiscountViewModel(possibleDiscount.DiscountType, possibleDiscount.DiscountedUnitPrice);
                yield return item;

                yield return new ItemViewModel(item.Product, 1, null);
            }
        }
    }
}