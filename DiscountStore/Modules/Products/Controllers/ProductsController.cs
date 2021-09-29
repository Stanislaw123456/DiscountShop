using System.Threading;
using System.Threading.Tasks;
using DiscountStore.Modules.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DiscountStore.Modules.Products.Controllers
{
    [Route("products")]
    public class ProductsController : Controller
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Route("")]
        [Route("~/")]
        [Route("index")]
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var query = new FindAllProductsQuery();
            var result = await _mediator.Send(query, cancellationToken);

            return View(result);
        }
    }
}