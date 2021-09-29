using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscountStore.Infrastructure;
using DiscountStore.Modules.Checkout.ViewModels;
using DiscountStore.Modules.Discounts.Providers;
using DiscountStore.Modules.Products.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DiscountStore.Modules.Checkout.Commands
{
    public record AddItemToCartCommand(IEnumerable<ItemViewModel> CurrentCart, int ProductId)
        : IRequest<IEnumerable<ItemViewModel>>;

    public class AddItemToCartCommandHandler : IRequestHandler<AddItemToCartCommand, IEnumerable<ItemViewModel>>
    {
        private readonly DiscountStoreDbContext _discountStoreDbContext;
        private readonly IEnumerable<IDiscountProvider> _discountProviders;
        private readonly IMediator _mediator;

        public AddItemToCartCommandHandler(DiscountStoreDbContext discountStoreDbContext, IEnumerable<IDiscountProvider> discountProviders, IMediator mediator)
        {
            _discountStoreDbContext = discountStoreDbContext;
            _discountProviders = discountProviders;
            _mediator = mediator;
        }

        public async Task<IEnumerable<ItemViewModel>> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
        {
            if (request.CurrentCart == null)
            {
                throw new ArgumentNullException(paramName: nameof(request.CurrentCart));
            }

            var product = _discountStoreDbContext.Products
                .AsNoTracking()
                .Select(product => new ProductViewModel(product.Id, product.Name, product.Price))
                .FirstOrDefault(d => d.Id == request.ProductId);

            if (product == null)
            {
                throw new InvalidOperationException($"Product with id: {request.ProductId} doesn't exist!");
            }

            var newCart = new List<ItemViewModel>(request.CurrentCart) { new ItemViewModel(product, 1, null) };
            
            var command = new MergeDuplicatedItemsCommand(newCart);
            newCart = (await _mediator.Send(command, cancellationToken)).ToList();

            foreach (var discountProvider in _discountProviders)
            {
                newCart = await discountProvider.ApplyDiscountsAsync(newCart, cancellationToken)
                    .ToListAsync(cancellationToken: cancellationToken);
            }

            command = new MergeDuplicatedItemsCommand(newCart);
            return await _mediator.Send(command, cancellationToken);
        }
    }
}