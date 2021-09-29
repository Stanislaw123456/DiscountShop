using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DiscountStore.Modules.Checkout.Queries;
using DiscountStore.Modules.Checkout.ViewModels;
using DiscountStore.Modules.Products.ViewModels;
using Xunit;

namespace DiscountStoreTests.Modules.Checkout.Queries
{
    public class GetTotalQueriesHandlerTests
    {
        [Fact]
        public async Task GivenNullCart_Returns0()
        {
            var sut = CreateSut();
            var query = new GetTotalQueries(null);

            var result = await sut.Handle(query, CancellationToken.None);

            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GivenEmptyCart_Returns0()
        {
            var cart = new List<ItemViewModel>();
            var sut = CreateSut();
            var query = new GetTotalQueries(cart);

            var result = await sut.Handle(query, CancellationToken.None);

            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GivenSingleItemInCart_ReturnsThatItemSubTotal()
        {
            var cart = new List<ItemViewModel> { new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null) };
            var sut = CreateSut();
            var query = new GetTotalQueries(cart);

            var result = await sut.Handle(query, CancellationToken.None);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task GivenTwoItemsInCart_ReturnsSumOfTheseItemsSubTotal()
        {
            var cart = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null),
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 2, null)
            };
            var sut = CreateSut();
            var query = new GetTotalQueries(cart);

            var result = await sut.Handle(query, CancellationToken.None);

            Assert.Equal(3, result);
        }

        private static GetTotalQueriesHandler CreateSut()
        {
            return new GetTotalQueriesHandler();
        }
    }
}