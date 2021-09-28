using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscountStore.Infrastructure;
using DiscountStore.Modules.Discounts.Enums;
using DiscountStore.Modules.Discounts.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DiscountStore.Modules.Discounts.Queries
{
    public record FindDiscountsByDiscountTypeQuery(DiscountType DiscountType)
        : IRequest<IEnumerable<Discount>>;

    public class
        FindDiscountsByDiscountTypeQueryHandler : IRequestHandler<FindDiscountsByDiscountTypeQuery,
            IEnumerable<Discount>>
    {
        private readonly DiscountStoreDbContext _dbContext;

        public FindDiscountsByDiscountTypeQueryHandler(
            DiscountStoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Discount>> Handle(FindDiscountsByDiscountTypeQuery request,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Discounts
                .AsNoTracking()
                .Where(discount => discount.DiscountType == request.DiscountType)
                .OrderBy(discount => discount.Id)
                .ToListAsync(cancellationToken: cancellationToken);
        }
    }
}