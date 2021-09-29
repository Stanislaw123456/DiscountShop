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
    public record RemoveItemFromCartCommand(IEnumerable<ItemViewModel> CurrentCart, int ProductId)
        : IRequest<IEnumerable<ItemViewModel>>;

    public class
        RemoveItemFromCartCommandHandler : IRequestHandler<RemoveItemFromCartCommand, IEnumerable<ItemViewModel>>
    {
        private readonly DiscountStoreDbContext _discountStoreDbContext;
        private readonly IEnumerable<IDiscountProvider> _discountProviders;
        private readonly IMediator _mediator;

        public RemoveItemFromCartCommandHandler(DiscountStoreDbContext discountStoreDbContext,
            IEnumerable<IDiscountProvider> discountProviders, IMediator mediator)
        {
            _discountStoreDbContext = discountStoreDbContext;
            _discountProviders = discountProviders;
            _mediator = mediator;
        }

        public async Task<IEnumerable<ItemViewModel>> Handle(RemoveItemFromCartCommand request,
            CancellationToken cancellationToken)
        {
            if (request.CurrentCart == null)
            {
                throw new ArgumentNullException(paramName: nameof(request.CurrentCart));
            }

            if (request.CurrentCart.All(item => item.Product.Id != request.ProductId))
            {
                throw new InvalidOperationException($"Product with id: {request.ProductId} isn't in the cart!");
            }

            var product = _discountStoreDbContext.Products
                .AsNoTracking()
                .Select(product => new ProductViewModel(product.Id, product.Name, product.Price))
                .FirstOrDefault(d => d.Id == request.ProductId);

            if (product == null)
            {
                throw new InvalidOperationException($"Product with id: {request.ProductId} doesn't exist!");
            }

            var newCart = request.CurrentCart.ToList();
            RemoveSingleProductFromCart(request, newCart);

            foreach (var discountProvider in _discountProviders)
            {
                newCart = await discountProvider.ApplyDiscountsAsync(newCart, cancellationToken)
                    .ToListAsync(cancellationToken: cancellationToken);
            }

            var command = new MergeDuplicatedItemsCommand(newCart);
            return await _mediator.Send(command, cancellationToken);
        }

        private static void RemoveSingleProductFromCart(RemoveItemFromCartCommand request, List<ItemViewModel> newCart)
        {
            var notDiscountedItem = newCart
                .Where(item => item.Product.Id == request.ProductId)
                .FirstOrDefault(item => item.AppliedDiscount == null);

            if (notDiscountedItem != null)
            {
                notDiscountedItem.Quantity--;
                if (notDiscountedItem.Quantity < 1)
                {
                    newCart.Remove(notDiscountedItem);
                }
            }
            else
            {
                var discountedItem = newCart.First(item => item.Product.Id == request.ProductId);
                discountedItem.Quantity--;
                discountedItem.AppliedDiscount = null;
                if (discountedItem.Quantity < 1)
                {
                    newCart.Remove(discountedItem);
                }
            }
        }
    }
}