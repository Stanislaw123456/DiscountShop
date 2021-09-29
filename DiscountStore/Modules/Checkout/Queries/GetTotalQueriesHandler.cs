using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscountStore.Modules.Checkout.ViewModels;
using MediatR;

namespace DiscountStore.Modules.Checkout.Queries
{
    public record GetTotalQueries(IEnumerable<ItemViewModel> Cart)
        : IRequest<double>;

    public class GetTotalQueriesHandler : IRequestHandler<GetTotalQueries, double>
    {
        public Task<double> Handle(GetTotalQueries request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.Cart?.Sum(item => item.SubTotal) ?? 0);
        }
    }
}