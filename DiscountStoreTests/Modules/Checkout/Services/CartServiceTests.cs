using System.Collections.Generic;
using DiscountStore.Modules.Checkout.Services;
using DiscountStore.Modules.Checkout.ViewModels;
using DiscountStore.Modules.Products.ViewModels;
using Xunit;

namespace DiscountStoreTests.Modules.Checkout.Services
{
    public class CartServiceTests
    {
        [Fact]
        public void GivenNullCart_Returns0()
        {
            var sut = new CartService();

            var result = sut.GetTotal(null);

            Assert.Equal(0, result);
        }

        [Fact]
        public void GivenEmptyCart_Returns0()
        {
            var cart = new List<ItemViewModel>();
            var sut = new CartService();

            var result = sut.GetTotal(cart);

            Assert.Equal(0, result);
        }

        [Fact]
        public void GivenSingleItemInCart_ReturnsThatItemSubTotal()
        {
            var cart = new List<ItemViewModel> { new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null) };
            var sut = new CartService();

            var result = sut.GetTotal(cart);

            Assert.Equal(1, result);
        }

        [Fact]
        public void GivenTwoItemsInCart_ReturnsSumOfTheseItemsSubTotal()
        {
            var cart = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null),
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 2, null)
            };
            var sut = new CartService();

            var result = sut.GetTotal(cart);

            Assert.Equal(3, result);
        }
    }
}