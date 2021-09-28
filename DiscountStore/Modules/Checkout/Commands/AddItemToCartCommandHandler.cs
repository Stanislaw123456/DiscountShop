using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscountStore.Infrastructure;
using DiscountStore.Modules.Checkout.Services;
using DiscountStore.Modules.Checkout.ViewModels;
using DiscountStore.Modules.Discounts.Providers;
using DiscountStore.Modules.Products.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DiscountStore.Modules.Checkout.Commands
{
    public record AddItemToCartCommand(IEnumerable<ItemViewModel> CurrentCart, int ProductId)
        : IRequest<IEnumerable<ItemViewModel>>;
        
    public class AddItemToCartCommandHandler: IRequestHandler<AddItemToCartCommand, IEnumerable<ItemViewModel>>
    {
        private readonly DiscountStoreDbContext _discountStoreDbContext;
        private readonly IEnumerable<IDiscountProvider> _discountProviders;
        private readonly IItemsManipulationService _itemsManipulationService;

        public AddItemToCartCommandHandler(DiscountStoreDbContext discountStoreDbContext, IEnumerable<IDiscountProvider> discountProviders, IItemsManipulationService itemsManipulationService)
        {
            _discountStoreDbContext = discountStoreDbContext;
            _discountProviders = discountProviders;
            _itemsManipulationService = itemsManipulationService;
        }

        public async Task<IEnumerable<ItemViewModel>> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
        {
            if (request.CurrentCart == null)
            {
                throw new ArgumentNullException(paramName: nameof(request.CurrentCart));
            }
            
            var product = _discountStoreDbContext.Products
                .AsNoTracking()
                .Select(product => new ProductViewModel(product.Name, product.Price, product.Id))
                .FirstOrDefault(d => d.Id == request.ProductId);

            if (product == null)
            {
                throw new InvalidOperationException($"Product with id: {request.ProductId} doesn't exist!");
            }

            var newCart = new List<ItemViewModel>(request.CurrentCart) { new ItemViewModel(product, 1, null) };
            newCart = _itemsManipulationService.MergeDuplicatesItems(newCart).ToList();
            
            foreach (var discountProvider in _discountProviders)
            {
                newCart = await discountProvider.ApplyDiscountsAsync(newCart, cancellationToken).ToListAsync(cancellationToken: cancellationToken);
            }

            return _itemsManipulationService.MergeDuplicatesItems(newCart);
        }
    }
}