using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DiscountStore.Extensions;
using DiscountStore.Modules.Checkout.Commands;
using DiscountStore.Modules.Checkout.Queries;
using DiscountStore.Modules.Checkout.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DiscountStore.Modules.Checkout.Controllers
{
    [Route("checkout")]
    public class CheckoutController : Controller
    {
        private readonly IMediator _mediator;

        public CheckoutController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Route("index")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<ItemViewModel>>("cart") ?? new List<ItemViewModel>();

            var command = new GetTotalQueries(cart);
            var total = await _mediator.Send(command, cancellationToken);
            
            ViewBag.cart = cart;
            ViewBag.total = total;

            return View();
        }

        [Route("add/{productId}")]
        public async Task<IActionResult> Add(int productId, CancellationToken cancellationToken)
        {
            var exitingCart = HttpContext.Session.GetObjectFromJson<List<ItemViewModel>>("cart");

            var command = new AddItemToCartCommand(exitingCart ?? new List<ItemViewModel>(), productId);
            var newCart = await _mediator.Send(command, cancellationToken);

            HttpContext.Session.SetObjectAsJson("cart", newCart);

            return RedirectToAction("Index");
        }

        [Route("remove/{productId}")]
        public async Task<IActionResult> Remove(int productId, CancellationToken cancellationToken)
        {
            var exitingCart = HttpContext.Session.GetObjectFromJson<List<ItemViewModel>>("cart");

            var command = new RemoveItemFromCartCommand(exitingCart ?? new List<ItemViewModel>(), productId);
            var newCart = await _mediator.Send(command, cancellationToken);

            HttpContext.Session.SetObjectAsJson("cart", newCart);

            return RedirectToAction("Index");
        }
    }
}