using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscountStore.Infrastructure;
using DiscountStore.Modules.Checkout.Commands;
using DiscountStore.Modules.Checkout.Services;
using DiscountStore.Modules.Checkout.ViewModels;
using DiscountStore.Modules.Discounts.Providers;
using DiscountStore.Modules.Products.Models;
using DiscountStore.Modules.Products.ViewModels;
using DiscountStoreTests.TestsInfrastructure;
using NSubstitute;
using Xunit;

namespace DiscountStoreTests.Modules.Checkout.Commands
{
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    public class AddItemToCartCommandHandlerTests
    {
        private readonly IEnumerable<IDiscountProvider> _discountProviders;
        private readonly ICartService _cartService;

        public AddItemToCartCommandHandlerTests()
        {
            _discountProviders = Substitute.For<IEnumerable<IDiscountProvider>>();
            _cartService = Substitute.For<ICartService>();
            _cartService.MergeDuplicatedItems(Arg.Any<IEnumerable<ItemViewModel>>())
                .Returns(callInfo => callInfo.Arg<IEnumerable<ItemViewModel>>());
        }

        [Fact]
        public async Task GivenNullCart_ThrowsArgumentNullException()
        {
            var emptyDb = DbContexts.Empty();
            var sut = CreateSut(emptyDb, _discountProviders, _cartService);
            var command = new AddItemToCartCommand(null, 1);

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task GivenEmptyCartAndNotExistingProductId_ThrowsInvalidOperationException()
        {
            var emptyDb = DbContexts.Empty();
            var sut = CreateSut(emptyDb, _discountProviders, _cartService);
            var cart = new List<ItemViewModel>();
            var command = new AddItemToCartCommand(cart, 1);

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task GivenEmptyCartAndExistingProductId_ReturnsCollectionWithThatProduct()
        {
            var product = new Product { Id = 1 };
            var dbContext = DbContexts.For(product);
            var sut = CreateSut(dbContext, _discountProviders, _cartService);
            var cart = new List<ItemViewModel>();
            var command = new AddItemToCartCommand(cart, 1);

            var result = (await sut.Handle(command, CancellationToken.None)).ToList();

            Assert.Single(result);
            Assert.Equal(1, result.First().Product.Id);
        }

        [Fact]
        public async Task GivenCartWithItemAndExistingProductId_ReturnsCollectionWithBothProducts()
        {
            var products = new List<Product>
            {
                new Product { Id = 1 },
                new Product { Id = 2 }
            }.AsEnumerable();
            var dbContext = DbContexts.For(products);
            var sut = CreateSut(dbContext, _discountProviders, _cartService);
            var cart = new List<ItemViewModel> { new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null) };
            var command = new AddItemToCartCommand(cart, 2);

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.Collection(result, item1 => { Assert.Equal(1, item1.Product.Id); },
                item2 => { Assert.Equal(2, item2.Product.Id); });
        }

        [Fact]
        public async Task GivenMatchingCartAndProductId_CallsBothDiscountsProvider()
        {
            var firstDiscountProvider = Substitute.For<IDiscountProvider>();
            var secondDiscountProvider = Substitute.For<IDiscountProvider>();
            var discountsProvider = new List<IDiscountProvider>
            {
                firstDiscountProvider,
                secondDiscountProvider
            };

            var product = new Product { Id = 1 };
            var dbContext = DbContexts.For(product);
            var sut = CreateSut(dbContext, discountsProvider, _cartService);
            var cart = new List<ItemViewModel> { new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null) };
            var command = new AddItemToCartCommand(cart, 1);

            await sut.Handle(command, CancellationToken.None);

            await firstDiscountProvider
                .Received()
                .ApplyDiscountsAsync(
                    Arg.Any<ICollection<ItemViewModel>>(),
                    Arg.Any<CancellationToken>())
                .ToListAsync();

            await secondDiscountProvider
                .Received()
                .ApplyDiscountsAsync(
                    Arg.Any<ICollection<ItemViewModel>>(),
                    Arg.Any<CancellationToken>())
                .ToListAsync();
        }

        private static AddItemToCartCommandHandler CreateSut(DiscountStoreDbContext discountStoreDbContext,
            IEnumerable<IDiscountProvider> discountProviders, ICartService cartService)
        {
            return new AddItemToCartCommandHandler(discountStoreDbContext, discountProviders, cartService);
        }
    }
}