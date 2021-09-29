using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using DiscountStore.Infrastructure;
using DiscountStore.Modules.Discounts.Enums;
using DiscountStore.Modules.Discounts.Queries;
using DiscountStore.Modules.Discounts.Models;
using DiscountStoreTests.TestsInfrastructure;
using Xunit;

namespace DiscountStoreTests.Modules.Discounts.Queries
{
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    public class FindDiscountsByDiscountTypeQueryHandlerTests
    {
        [Fact]
        public async Task DatabaseDoesNotContainAnyDiscounts_GivenTwoForX__ReturnsEmptyCollection()
        {
            var emptyDb = DbContexts.Empty();
            var sut = CreateSut(emptyDb);
            var query = new FindDiscountsByDiscountTypeQuery(DiscountType.TwoForX);

            var result = await sut.Handle(query, CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task DatabaseContainSingleDiscountTwoForX_GivenTwoForX__ReturnsCollectionWithThatDiscount()
        {
            var discount = new Discount { DiscountType = DiscountType.TwoForX };
            var dbContext = DbContexts.For(discount);
            var sut = CreateSut(dbContext);
            var query = new FindDiscountsByDiscountTypeQuery(DiscountType.TwoForX);

            var result = (await sut.Handle(query, CancellationToken.None)).ToList();

            Assert.Single(result);
            Assert.Equal(DiscountType.TwoForX, result.First().DiscountType);
        }

        [Fact]
        public async Task
            DatabaseContainTwoDiscountsTwoForX_GivenTwoForX_ReturnsCollectionWithThatDiscountsOrderedByIdAsc()
        {
            var discounts = new List<Discount>
            {
                new Discount { DiscountType = DiscountType.TwoForX, ProductId = 1 },
                new Discount { DiscountType = DiscountType.TwoForX, ProductId = 2 }
            }.AsEnumerable();
            var dbContext = DbContexts.For(discounts);
            var sut = CreateSut(dbContext);
            var query = new FindDiscountsByDiscountTypeQuery(DiscountType.TwoForX);

            var result = (await sut.Handle(query, CancellationToken.None)).ToList();

            Assert.Collection(result, item1 =>
            {
                Assert.Equal(DiscountType.TwoForX, item1.DiscountType);
                Assert.Equal(1, item1.Id);
            }, item2 =>
            {
                Assert.Equal(DiscountType.TwoForX, item2.DiscountType);
                Assert.Equal(2, item2.Id);
            });
        }

        [Fact]
        public async Task
            DatabaseContainTwoDiscountsTwoForXAndSingleThreeForX_GivenTwoForX__ReturnsCollectionWithTwoForXDiscountsOrderedByIdAsc()
        {
            var discounts = new List<Discount>
            {
                new Discount { DiscountType = DiscountType.TwoForX, ProductId = 1 },
                new Discount { DiscountType = DiscountType.TwoForX, ProductId = 2 },
                new Discount { DiscountType = DiscountType.ThreeForX, ProductId = 2 }
            }.AsEnumerable();
            var dbContext = DbContexts.For(discounts);
            var sut = CreateSut(dbContext);
            var query = new FindDiscountsByDiscountTypeQuery(DiscountType.TwoForX);

            var result = (await sut.Handle(query, CancellationToken.None)).ToList();

            Assert.Collection(result, item1 =>
            {
                Assert.Equal(DiscountType.TwoForX, item1.DiscountType);
                Assert.Equal(1, item1.Id);
            }, item2 =>
            {
                Assert.Equal(DiscountType.TwoForX, item2.DiscountType);
                Assert.Equal(2, item2.Id);
            });
        }

        private static FindDiscountsByDiscountTypeQueryHandler CreateSut(DiscountStoreDbContext dbContext)
        {
            return new FindDiscountsByDiscountTypeQueryHandler(dbContext);
        }
    }
}