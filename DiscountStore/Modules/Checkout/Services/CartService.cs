using System.Collections.Generic;
using System.Linq;
using DiscountStore.Modules.Checkout.ViewModels;

namespace DiscountStore.Modules.Checkout.Services
{
    public class CartService : ICartService
    {
        public double GetTotal(IEnumerable<ItemViewModel> cart)
        {
            return cart?.Sum(item => item.SubTotal) ?? 0;
        }
    }
}