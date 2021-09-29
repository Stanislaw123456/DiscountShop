using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DiscountStore.Modules.Checkout.Services;
using DiscountStore.Modules.Checkout.ViewModels;
using DiscountStore.Modules.Discounts.Enums;
using DiscountStore.Modules.Discounts.ViewModels;
using DiscountStore.Modules.Products.ViewModels;
using Xunit;

namespace DiscountStoreTests.Modules.Checkout.Services
{
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    public class ItemsManipulationServiceTests
    {
        [Fact]
        public void GivenNullCollection_ReturnsEmptyCollection()
        {
            var sut = new ItemsManipulationService();

            var result = sut.MergeDuplicatesItems(null);

            Assert.Empty(result);
        }

        [Fact]
        public void GivenEmptyCollection_ReturnsEmptyCollection()
        {
            var itemsToMerge = new List<ItemViewModel>();
            var sut = new ItemsManipulationService();

            var result = sut.MergeDuplicatesItems(itemsToMerge);

            Assert.Empty(result);
        }

        [Fact]
        public void GivenSingleItem_ReturnsCollectionWithThatItem()
        {
            var itemsToMerge = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null)
            };
            var sut = new ItemsManipulationService();

            var result = sut.MergeDuplicatesItems(itemsToMerge);

            Assert.Single(result);
        }

        [Fact]
        public void GivenTwoItemsValidForMerge_ReturnsCollectionWithMergedItem()
        {
            var itemsToMerge = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null),
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null)
            };
            var sut = new ItemsManipulationService();

            var result = sut.MergeDuplicatesItems(itemsToMerge).ToList();

            Assert.Single(result);
            Assert.Equal(2, result.First().Quantity);
        }

        [Fact]
        public void GivenTwoItemsWithDiffrentProductId_ReturnsCollectionWithTheseItems()
        {
            var itemsToMerge = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null),
                new ItemViewModel(new ProductViewModel(2, "Product1", 1.0), 1, null)
            };
            var sut = new ItemsManipulationService();

            var result = sut.MergeDuplicatesItems(itemsToMerge);

            Assert.Collection(result, 
                item1 => { Assert.Equal(1, item1.Product.Id); },
                item2 => { Assert.Equal(2, item2.Product.Id); });
        }

        [Fact]
        public void GivenTwoItemsWithSameProductIdButOneWithDiscount_ReturnsCollectionWithTheseItems()
        {
            var itemsToMerge = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null),
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1,
                    new DiscountViewModel(DiscountType.TwoForX, 0.5))
            };
            var sut = new ItemsManipulationService();

            var result = sut.MergeDuplicatesItems(itemsToMerge);

            Assert.Collection(result, 
                item1 => { Assert.Equal(1, item1.Product.Id); },
                item2 => { Assert.Equal(1, item2.Product.Id); });
        }

        [Fact]
        public void
            GivenTwoItemsWithSameProductIdAndDiscountAndOneWithoutDiscount_ReturnsCollectionWithMergedDiscountedItemsAndNotMergedItemWithoutDiscountTheseItems()
        {
            var itemsToMerge = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null),
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1,
                    new DiscountViewModel(DiscountType.TwoForX, 0.5)),
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1,
                    new DiscountViewModel(DiscountType.TwoForX, 0.5))
            };
            var sut = new ItemsManipulationService();

            var result = sut.MergeDuplicatesItems(itemsToMerge);

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
    }
}