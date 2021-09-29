using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscountStore.Infrastructure;
using DiscountStore.Modules.Products.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DiscountStore.Modules.Products.Queries
{
    public record FindAllProductsQuery()
        : IRequest<IEnumerable<ProductViewModel>>;

    public class FindAllProductsQueryHandler : IRequestHandler<FindAllProductsQuery, IEnumerable<ProductViewModel>>
    {
        private readonly DiscountStoreDbContext _dbContext;

        public FindAllProductsQueryHandler(
            DiscountStoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<ProductViewModel>> Handle(FindAllProductsQuery request, CancellationToken cancellationToken)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .OrderBy(product => product.Id)
                .Select(product => new ProductViewModel(product.Id, product.Name, product.Price))
                .ToListAsync(cancellationToken: cancellationToken);
        }
    }
}