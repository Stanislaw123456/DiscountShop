using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DiscountStore.Modules.Checkout.Services;
using DiscountStore.Modules.Checkout.ViewModels;
using DiscountStore.Modules.Discounts.Enums;
using DiscountStore.Modules.Discounts.ViewModels;
using DiscountStore.Modules.Products.ViewModels;
using Xunit;

namespace DiscountStoreTests.Modules.Checkout.Services
{
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    public class CartServiceTests
    {
        [Fact]
        public void GetTotal_GivenNullCart_Returns0()
        {
            var sut = new CartService();

            var result = sut.GetTotal(null);

            Assert.Equal(0, result);
        }

        [Fact]
        public void GetTotal_GivenEmptyCart_Returns0()
        {
            var cart = new List<ItemViewModel>();
            var sut = new CartService();

            var result = sut.GetTotal(cart);

            Assert.Equal(0, result);
        }

        [Fact]
        public void GetTotal_GivenSingleItemInCart_ReturnsThatItemSubTotal()
        {
            var cart = new List<ItemViewModel> { new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null) };
            var sut = new CartService();

            var result = sut.GetTotal(cart);

            Assert.Equal(1, result);
        }

        [Fact]
        public void GetTotal_GivenTwoItemsInCart_ReturnsSumOfTheseItemsSubTotal()
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
        
        [Fact]
        public void MergeDuplicatedItems_GivenNullCollection_ReturnsEmptyCollection()
        {
            var sut = new CartService();

            var result = sut.MergeDuplicatedItems(null);

            Assert.Empty(result);
        }

        [Fact]
        public void MergeDuplicatedItems_GivenEmptyCollection_ReturnsEmptyCollection()
        {
            var itemsToMerge = new List<ItemViewModel>();
            var sut = new CartService();

            var result = sut.MergeDuplicatedItems(itemsToMerge);

            Assert.Empty(result);
        }

        [Fact]
        public void MergeDuplicatedItems_GivenSingleItem_ReturnsCollectionWithThatItem()
        {
            var itemsToMerge = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null)
            };
            var sut = new CartService();

            var result = sut.MergeDuplicatedItems(itemsToMerge);

            Assert.Single(result);
        }

        [Fact]
        public void MergeDuplicatedItems_GivenTwoItemsValidForMerge_ReturnsCollectionWithMergedItem()
        {
            var itemsToMerge = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null),
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null)
            };
            var sut = new CartService();

            var result = sut.MergeDuplicatedItems(itemsToMerge).ToList();

            Assert.Single(result);
            Assert.Equal(2, result.First().Quantity);
        }

        [Fact]
        public void MergeDuplicatedItems_GivenTwoItemsWithDifferentProductId_ReturnsCollectionWithTheseItems()
        {
            var itemsToMerge = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null),
                new ItemViewModel(new ProductViewModel(2, "Product1", 1.0), 1, null)
            };
            var sut = new CartService();

            var result = sut.MergeDuplicatedItems(itemsToMerge);

            Assert.Collection(result, 
                item1 => { Assert.Equal(1, item1.Product.Id); },
                item2 => { Assert.Equal(2, item2.Product.Id); });
        }

        [Fact]
        public void MergeDuplicatedItems_GivenTwoItemsWithSameProductIdButOneWithDiscount_ReturnsCollectionWithTheseItems()
        {
            var itemsToMerge = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null),
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1,
                    new DiscountViewModel(DiscountType.TwoForX, 0.5))
            };
            var sut = new CartService();

            var result = sut.MergeDuplicatedItems(itemsToMerge);

            Assert.Collection(result, 
                item1 => { Assert.Equal(1, item1.Product.Id); },
                item2 => { Assert.Equal(1, item2.Product.Id); });
        }

        [Fact]
        public void MergeDuplicatedItems_GivenTwoItemsWithSameProductIdAndDiscountAndOneWithoutDiscount_ReturnsCollectionWithMergedDiscountedItemsAndNotMergedItemWithoutDiscountTheseItems()
        {
            var itemsToMerge = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1, null),
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1,
                    new DiscountViewModel(DiscountType.TwoForX, 0.5)),
                new ItemViewModel(new ProductViewModel(1, "Product1", 1.0), 1,
                    new DiscountViewModel(DiscountType.TwoForX, 0.5))
            };
            var sut = new CartService();

            var result = sut.MergeDuplicatedItems(itemsToMerge);

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