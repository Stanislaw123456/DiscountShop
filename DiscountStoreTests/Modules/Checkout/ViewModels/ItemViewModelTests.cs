using DiscountStore.Modules.Checkout.ViewModels;
using DiscountStore.Modules.Discounts.Enums;
using DiscountStore.Modules.Discounts.ViewModels;
using DiscountStore.Modules.Products.ViewModels;
using Xunit;

namespace DiscountStoreTests.Modules.Checkout.ViewModels
{
    public class ItemViewModelTests
    {
        [Fact]
        public void GivenNoDiscountAndSingleQuantity_ReturnsNotMultipliedPriceAsSubTotal()
        {
            var product = new ProductViewModel(1, "Product1", 1);
            var sut = new ItemViewModel(product, 1, null);

            var result = sut.SubTotal;

            Assert.Equal(1, result);
        }

        [Fact]
        public void GivenNoDiscountAndTripleQuantity_ReturnsPriceMultipliedAsSubTotal()
        {
            var product = new ProductViewModel(1, "Product1", 1);
            var sut = new ItemViewModel(product, 3, null);

            var result = sut.SubTotal;

            Assert.Equal(3, result);
        }

        [Fact]
        public void GivenDiscountAndSingleQuantity_ReturnsNotMultipliedDiscountedUnitPriceAsSubTotal()
        {
            var product = new ProductViewModel(1, "Product1", 1);
            var discount = new DiscountViewModel(DiscountType.TwoForX, 0.5);
            var sut = new ItemViewModel(product, 1, discount);

            var result = sut.SubTotal;

            Assert.Equal(0.5, result);
        }

        [Fact]
        public void GivenDiscountAndTriplyQuantity_ReturnsMultipliedDiscountedUnitPriceAsSubTotal()
        {
            var product = new ProductViewModel(1, "Product1", 1);
            var discount = new DiscountViewModel(DiscountType.TwoForX, 0.5);
            var sut = new ItemViewModel(product, 3, discount);

            var result = sut.SubTotal;

            Assert.Equal(1.5, result);
        }
    }
}