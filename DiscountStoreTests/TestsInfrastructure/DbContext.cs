using System;
using System.Collections.Generic;
using DiscountStore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DiscountStoreTests.TestsInfrastructure
{
    public static class DbContexts
    {
        public static DiscountStoreDbContext Empty()
        {
            var options = new DbContextOptionsBuilder<DiscountStoreDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;

            return new DiscountStoreDbContext(options);
        }

        public static DiscountStoreDbContext For<TEntity>(params TEntity[] entities)
            where TEntity : class
        {
            var dbContext = Empty();
            dbContext.Set<TEntity>().AddRange(entities);
            dbContext.SaveChanges();

            return dbContext;
        }

        public static DiscountStoreDbContext For<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            var dbContext = Empty();
            dbContext.Set<TEntity>().AddRange(entities);
            dbContext.SaveChanges();

            return dbContext;
        }
    }
}