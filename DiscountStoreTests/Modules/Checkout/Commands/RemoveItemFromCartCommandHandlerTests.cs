using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using DiscountStore.Infrastructure;
using DiscountStore.Modules.Checkout.Commands;
using DiscountStore.Modules.Checkout.ViewModels;
using DiscountStore.Modules.Discounts.Enums;
using DiscountStore.Modules.Discounts.Providers;
using DiscountStore.Modules.Discounts.ViewModels;
using DiscountStore.Modules.Products.Models;
using DiscountStore.Modules.Products.ViewModels;
using DiscountStoreTests.TestsInfrastructure;
using MediatR;
using NSubstitute;
using Xunit;

namespace DiscountStoreTests.Modules.Checkout.Commands
{
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    public class RemoveItemFromCartCommandHandlerTests
    {
        private readonly IEnumerable<IDiscountProvider> _discountProviders;
        private readonly IMediator _mediator;

        public RemoveItemFromCartCommandHandlerTests()
        {
            _discountProviders = Substitute.For<IEnumerable<IDiscountProvider>>();
            _mediator = Substitute.For<IMediator>();
            _mediator.Send(Arg.Any<MergeDuplicatedItemsCommand>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => callInfo.Arg<MergeDuplicatedItemsCommand>().ItemsToMerge);
        }

        [Fact]
        public async Task GivenNullCart_ThrowsArgumentNullException()
        {
            var emptyDb = DbContexts.Empty();
            var sut = CreateSut(emptyDb, _discountProviders, _mediator);
            var command = new RemoveItemFromCartCommand(null, 1);

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task GivenEmptyCartAndNotExistingProductId_ThrowsInvalidOperationException()
        {
            var emptyDb = DbContexts.Empty();
            var sut = CreateSut(emptyDb, _discountProviders, _mediator);
            var cart = new List<ItemViewModel>();
            var command = new RemoveItemFromCartCommand(cart, 1);

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task GivenEmptyCartAndExistingProductId_ThrowsInvalidOperationException()
        {
            var product = new Product { Id = 1 };
            var dbContext = DbContexts.For(product);
            var sut = CreateSut(dbContext, _discountProviders, _mediator);
            var cart = new List<ItemViewModel>();
            var command = new RemoveItemFromCartCommand(cart, 1);

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task
            GivenCartWithExistingProductIdAndExistingProductIdThatIsNotInCart_ThrowsInvalidOperationException()
        {
            var products = new List<Product>
            {
                new Product { Id = 1 },
                new Product { Id = 2 },
            }.AsEnumerable();
            var dbContext = DbContexts.For(products);
            var sut = CreateSut(dbContext, _discountProviders, _mediator);
            var cart = new List<ItemViewModel> { new ItemViewModel(new ProductViewModel(2, "Product1", 1.0), 1, null) };
            var command = new RemoveItemFromCartCommand(cart, 1);

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task GivenCartWithNotExistingProductIdAndMatchingProductId_ThrowsInvalidOperationException()
        {
            var dbContext = DbContexts.Empty();
            var sut = CreateSut(dbContext, _discountProviders, _mediator);
            var cart = new List<ItemViewModel> { new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null) };
            var command = new RemoveItemFromCartCommand(cart, 1);

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task GivenCartWithItemWithSingleQuantityAndExistingProductId_ReturnsEmptyCollection()
        {
            var products = new List<Product>
            {
                new Product { Id = 1 }
            }.AsEnumerable();
            var dbContext = DbContexts.For(products);
            var sut = CreateSut(dbContext, _discountProviders, _mediator);
            var cart = new List<ItemViewModel> { new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null) };
            var command = new RemoveItemFromCartCommand(cart, 1);

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GivenCartWithItemWithTwoQuantityAndExistingProductId_ReturnsCollectionWithSingleItemWithQuantityOne()
        {
            var products = new List<Product>
            {
                new Product { Id = 1 }
            }.AsEnumerable();
            var dbContext = DbContexts.For(products);
            var sut = CreateSut(dbContext, _discountProviders, _mediator);
            var cart = new List<ItemViewModel> { new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 2, null) };
            var command = new RemoveItemFromCartCommand(cart, 1);

            var result = (await sut.Handle(command, CancellationToken.None)).ToList();

            Assert.Single(result);
            Assert.Equal(1, result.First().Quantity);
        }

        [Fact]
        public async Task GivenCartWithDiscountedItemWithTwoQuantityAndExistingProductId_ReturnsCollectionWithSingleItemWithQuantityOne()
        {
            var products = new List<Product>
            {
                new Product { Id = 1 }
            }.AsEnumerable();
            var dbContext = DbContexts.For(products);
            var sut = CreateSut(dbContext, _discountProviders, _mediator);
            var cart = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 2,
                    new DiscountViewModel(DiscountType.TwoForX, 0.5))
            };
            var command = new RemoveItemFromCartCommand(cart, 1);

            var result = (await sut.Handle(command, CancellationToken.None)).ToList();

            Assert.Single(result);
            Assert.Equal(1, result.First().Quantity);
        }

        [Fact]
        public async Task GivenCartWithDiscountedAndNotDiscountedItemsWithTwoQuantityAndExistingProductId_ReturnsCollectionWithNotDiscountedItemDeducedQuantity()
        {
            var products = new List<Product>
            {
                new Product { Id = 1 }
            }.AsEnumerable();
            var dbContext = DbContexts.For(products);
            var sut = CreateSut(dbContext, _discountProviders, _mediator);
            var cart = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 2,
                    new DiscountViewModel(DiscountType.TwoForX, 0.5)),
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 2, null)
            };
            var command = new RemoveItemFromCartCommand(cart, 1);

            var result = (await sut.Handle(command, CancellationToken.None)).ToList();

            Assert.Collection(result, item1 =>
            {
                Assert.Equal(2, item1.Quantity);
                Assert.NotNull(item1.AppliedDiscount);
            }, item2 =>
            {
                Assert.Equal(1, item2.Quantity);
                Assert.Null(item2.AppliedDiscount);
            });
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
            var sut = CreateSut(dbContext, discountsProvider, _mediator);
            var cart = new List<ItemViewModel> { new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null) };
            var command = new RemoveItemFromCartCommand(cart, 1);

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

        private static RemoveItemFromCartCommandHandler CreateSut(DiscountStoreDbContext discountStoreDbContext,
            IEnumerable<IDiscountProvider> discountProviders, IMediator mediator)
        {
            return new RemoveItemFromCartCommandHandler(discountStoreDbContext, discountProviders, mediator);
        }
    }
}