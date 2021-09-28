using System.Collections.Generic;
using System.Threading;
using DiscountStore.Modules.Checkout.ViewModels;

namespace DiscountStore.Modules.Discounts.Providers
{
    public interface IDiscountProvider
    {
        IAsyncEnumerable<ItemViewModel> ApplyDiscountsAsync(ICollection<ItemViewModel> items, CancellationToken cancellationToken);
    }
}