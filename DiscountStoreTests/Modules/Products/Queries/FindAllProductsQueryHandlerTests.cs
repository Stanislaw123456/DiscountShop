using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscountStore.Infrastructure;
using DiscountStore.Modules.Products.Models;
using DiscountStore.Modules.Products.Queries;
using DiscountStoreTests.TestsInfrastructure;
using Xunit;

namespace DiscountStoreTests.Modules.Products.Queries
{
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    public class FindAllProductsQueryHandlerTests
    {
        [Fact]
        public async Task DatabaseDoesNotContainAnyProducts_ReturnsEmptyCollection()
        {
            var emptyDb = DbContexts.Empty();
            var sut = CreateSut(emptyDb);
            var query = new FindAllProductsQuery();

            var result = await sut.Handle(query, CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task DatabaseContainSingleProduct_ReturnsCollectionWithThatProduct()
        {
            var product = new Product { Id = 9 };
            var dbContext = DbContexts.For(product);
            var sut = CreateSut(dbContext);
            var query = new FindAllProductsQuery();

            var result = (await sut.Handle(query, CancellationToken.None)).ToList();

            Assert.Single(result);
            Assert.Equal(9, result.First().Id);
        }

        [Fact]
        public async Task DatabaseContainTwoProducts_ReturnsCollectionWithThatProductsOrderedByIdAsc()
        {
            var products = new List<Product>
            {
                new Product { Id = 9 },
                new Product { Id = 5 }
            }.AsEnumerable();
            var dbContext = DbContexts.For(products);
            var sut = CreateSut(dbContext);
            var query = new FindAllProductsQuery();

            var result = (await sut.Handle(query, CancellationToken.None)).ToList();

            Assert.Collection(result, item1 => { Assert.Equal(5, item1.Id); }, item2 => { Assert.Equal(9, item2.Id); });
        }

        private static FindAllProductsQueryHandler CreateSut(DiscountStoreDbContext dbContext)
        {
            return new FindAllProductsQueryHandler(dbContext);
        }
    }
}