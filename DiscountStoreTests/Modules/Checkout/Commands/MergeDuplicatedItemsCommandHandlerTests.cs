using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscountStore.Modules.Checkout.Commands;
using DiscountStore.Modules.Checkout.ViewModels;
using DiscountStore.Modules.Discounts.Enums;
using DiscountStore.Modules.Discounts.ViewModels;
using DiscountStore.Modules.Products.ViewModels;
using Xunit;

namespace DiscountStoreTests.Modules.Checkout.Commands
{
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    public class MergeDuplicatedItemsCommandHandlerTests
    {
        [Fact]
        public async Task GivenNullCollection_ReturnsEmptyCollection()
        {
            var sut = CreateSut();
            var command = new MergeDuplicatedItemsCommand(null);

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GivenEmptyCollection_ReturnsEmptyCollection()
        {
            var itemsToMerge = new List<ItemViewModel>();
            var sut = CreateSut();
            var command = new MergeDuplicatedItemsCommand(itemsToMerge);

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GivenSingleItem_ReturnsCollectionWithThatItem()
        {
            var itemsToMerge = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null)
            };
            var sut = CreateSut();
            var command = new MergeDuplicatedItemsCommand(itemsToMerge);

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.Single(result);
        }

        [Fact]
        public async Task GivenTwoItemsValidForMerge_ReturnsCollectionWithMergedItem()
        {
            var itemsToMerge = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null),
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null)
            };
            var sut = CreateSut();
            var command = new MergeDuplicatedItemsCommand(itemsToMerge);

            var result = (await sut.Handle(command, CancellationToken.None)).ToList();

            Assert.Single(result);
            Assert.Equal(2, result.First().Quantity);
        }

        [Fact]
        public async Task GivenTwoItemsWithDifferentProductId_ReturnsCollectionWithTheseItems()
        {
            var itemsToMerge = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null),
                new ItemViewModel(new ProductViewModel(2, "Product1", 1.0), 1, null)
            };
            var sut = CreateSut();
            var command = new MergeDuplicatedItemsCommand(itemsToMerge);

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.Collection(result,
                item1 => { Assert.Equal(1, item1.Product.Id); },
                item2 => { Assert.Equal(2, item2.Product.Id); });
        }

        [Fact]
        public async Task GivenTwoItemsWithSameProductIdButOneWithDiscount_ReturnsCollectionWithTheseItems()
        {
            var itemsToMerge = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null),
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1,
                    new DiscountViewModel(DiscountType.TwoForX, 0.5))
            };
            var sut = CreateSut();
            var command = new MergeDuplicatedItemsCommand(itemsToMerge);

            var result = await sut.Handle(command, CancellationToken.None);

            Assert.Collection(result,
                item1 => { Assert.Equal(1, item1.Product.Id); },
                item2 => { Assert.Equal(1, item2.Product.Id); });
        }

        [Fact]
        public async Task GivenTwoItemsWithSameProductIdAndDiscountAndOneWithoutDiscount_ReturnsCollectionWithMergedDiscountedItemsAndNotMergedItemWithoutDiscountTheseItems()
        {
            var itemsToMerge = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null),
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1,
                    new DiscountViewModel(DiscountType.TwoForX, 0.5)),
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1,
                    new DiscountViewModel(DiscountType.TwoForX, 0.5))
            };
            var sut = CreateSut();
            var command = new MergeDuplicatedItemsCommand(itemsToMerge);

            var result = await sut.Handle(command, CancellationToken.None);

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

        private static MergeDuplicatedItemsCommandHandler CreateSut()
        {
            return new MergeDuplicatedItemsCommandHandler();
        }
    }
}