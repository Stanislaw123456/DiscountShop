using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscountStore.Modules.Checkout.ViewModels;
using DiscountStore.Modules.Discounts.Enums;
using DiscountStore.Modules.Discounts.Models;
using DiscountStore.Modules.Discounts.Providers;
using DiscountStore.Modules.Discounts.Queries;
using DiscountStore.Modules.Products.ViewModels;
using MediatR;
using NSubstitute;
using Xunit;

namespace DiscountStoreTests.Modules.Discounts.Providers
{
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    public class TwoForXDiscountTypeProviderTests
    {
        private readonly IMediator _mediator;

        public TwoForXDiscountTypeProviderTests()
        {
            _mediator = Substitute.For<IMediator>();
        }
        
        [Fact]
        public async Task GivenNullItems_ReturnEmptyCollection()
        {
            var sut = CreateSut(_mediator);

            var result =  await sut.ApplyDiscountsAsync(null, CancellationToken.None).ToListAsync();
            
            Assert.Empty(result);
        }
        
        [Fact]
        public async Task GivenEmptyItems_ReturnEmptyCollection()
        {
            var sut = CreateSut(_mediator);
            var items = new List<ItemViewModel>();

            var result =  await sut.ApplyDiscountsAsync(items, CancellationToken.None).ToListAsync();
            
            Assert.Empty(result);
        }

        [Fact]
        public async Task HavingNoDiscounts_GivenSingleItem_ReturnCollectionWithThatItem()
        {
            _mediator.Send(Arg.Any<FindDiscountsByDiscountTypeQuery>(), Arg.Any<CancellationToken>())
                .Returns(Enumerable.Empty<Discount>());
            
            var sut = CreateSut(_mediator);
            var items = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel("Product1", 1.0, 1), 2, null)
            };

            var result =  await sut.ApplyDiscountsAsync(items, CancellationToken.None).ToListAsync();

            Assert.Single(result);
        }
        
        [Fact]
        public async Task HavingDiscountNotMatching_GivenSingleItem_ReturnCollectionWithThatItemWithoutDiscount()
        {
            _mediator.Send(Arg.Any<FindDiscountsByDiscountTypeQuery>(), Arg.Any<CancellationToken>())
                .Returns(new List<Discount>{ new Discount{ DiscountType = DiscountType.TwoForX, ProductId = 9}});
            
            var sut = CreateSut(_mediator);
            var items = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel("Product1", 1.0, 1), 2, null)
            };

            var result =  await sut.ApplyDiscountsAsync(items, CancellationToken.None).ToListAsync();

            Assert.Single(result);
            Assert.Null(result.First().AppliedDiscount);
        }
        
        [Fact]
        public async Task HavingDiscountMatching_GivenSingleItem_ReturnCollectionWithThatItemWithDiscount()
        {
            _mediator.Send(Arg.Any<FindDiscountsByDiscountTypeQuery>(), Arg.Any<CancellationToken>())
                .Returns(new List<Discount>{ new Discount{ DiscountType = DiscountType.TwoForX, ProductId = 1}});
            
            var sut = CreateSut(_mediator);
            var items = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel("Product1", 1.0, 1), 2, null)
            };

            var result =  await sut.ApplyDiscountsAsync(items, CancellationToken.None).ToListAsync();

            Assert.Single(result);
            Assert.Equal(DiscountType.TwoForX, result.First().AppliedDiscount.DiscountType);
        }
        
        [Fact]
        public async Task HavingDiscountMatching_GivenTwoItems_ReturnCollectionWithThatItemsWithDiscount()
        {
            _mediator.Send(Arg.Any<FindDiscountsByDiscountTypeQuery>(), Arg.Any<CancellationToken>())
                .Returns(new List<Discount>{ new Discount{ DiscountType = DiscountType.TwoForX, ProductId = 1}});
            
            var sut = CreateSut(_mediator);
            var items = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel("Product1", 1.0, 1), 2, null),
                new ItemViewModel(new ProductViewModel("Product2", 1.0, 1), 2, null)
            };

            var result =  await sut.ApplyDiscountsAsync(items, CancellationToken.None).ToListAsync();

            Assert.Collection(result, item1 =>
            {
                Assert.Equal("Product1", item1.Product.Name);
                Assert.Equal(DiscountType.TwoForX, item1.AppliedDiscount.DiscountType);
            }, item2 =>
            {
                Assert.Equal("Product2", item2.Product.Name);
                Assert.Equal(DiscountType.TwoForX, item2.AppliedDiscount.DiscountType);
            });
        }
        
        [Fact]
        public async Task HavingDiscountMatching_GivenTwoItemMatchingDiscountAndOneNot_ReturnCollectionItemsWithSingleDiscount()
        {
            _mediator.Send(Arg.Any<FindDiscountsByDiscountTypeQuery>(), Arg.Any<CancellationToken>())
                .Returns(new List<Discount>{ new Discount{ DiscountType = DiscountType.TwoForX, ProductId = 1}});
            
            var sut = CreateSut(_mediator);
            var items = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel("Product1", 1.0, 1), 2, null),
                new ItemViewModel(new ProductViewModel("Product2", 1.0, 1), 1, null)
            };

            var result =  await sut.ApplyDiscountsAsync(items, CancellationToken.None).ToListAsync();

            Assert.Collection(result, item1 =>
            {
                Assert.Equal("Product1", item1.Product.Name);
                Assert.Equal(DiscountType.TwoForX, item1.AppliedDiscount.DiscountType);
            }, item2 =>
            {
                Assert.Equal("Product2", item2.Product.Name);
                Assert.Null(item2.AppliedDiscount);
            });
        }
        
        [Fact]
        public async Task HavingDiscountMatching_GivenSingleItemWithNotFullyMatchingQuantity_ReturnCollectionWithThatItemWithDiscountAndSecondItemWithoutDiscount()
        {
            _mediator.Send(Arg.Any<FindDiscountsByDiscountTypeQuery>(), Arg.Any<CancellationToken>())
                .Returns(new List<Discount>{ new Discount{ DiscountType = DiscountType.TwoForX, ProductId = 1}});
            
            var sut = CreateSut(_mediator);
            var items = new List<ItemViewModel>
            {
                new ItemViewModel(new ProductViewModel("Product1", 1.0, 1), 3, null)
            };

            var result =  await sut.ApplyDiscountsAsync(items, CancellationToken.None).ToListAsync();

            Assert.Collection(result, item1 =>
            {
                Assert.Equal("Product1", item1.Product.Name);
                Assert.Equal(DiscountType.TwoForX, item1.AppliedDiscount.DiscountType);
            }, item2 =>
            {
                Assert.Equal("Product1", item2.Product.Name);
                Assert.Null(item2.AppliedDiscount);
            });
        }
        
        private static TwoForXDiscountTypeProvider CreateSut(IMediator mediator)
        {
            return new TwoForXDiscountTypeProvider(mediator);
        }
    }
}