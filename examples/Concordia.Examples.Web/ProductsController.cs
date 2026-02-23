using Microsoft.AspNetCore.Mvc;

// Example Controller for usage (remains unchanged)
namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The products controller class
    /// </summary>
    /// <seealso cref="ControllerBase"/>
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        /// <summary>
        /// The mediator
        /// </summary>
        private readonly IMediator _mediator;
        /// <summary>
        /// The sender
        /// </summary>
        private readonly ISender _sender;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductsController"/> class
        /// </summary>
        /// <param name="mediator">The mediator</param>
        /// <param name="sender">The sender</param>
        public ProductsController(IMediator mediator, ISender sender)
        {
            _mediator = mediator;
            _sender = sender;
        }

        /// <summary>
        /// Gets all products
        /// </summary>
        /// <returns>A task containing the action result</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllProductsQuery();
            var products = await _sender.Send(query);
            return Ok(products);
        }

        /// <summary>
        /// Gets the id
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>A task containing the action result</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var query = new GetProductByIdQuery { ProductId = id };
            var product = await _sender.Send(query);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        /// <summary>
        /// Creates the product using the specified command
        /// </summary>
        /// <param name="command">The command</param>
        /// <returns>A task containing the action result</returns>
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
        {
            await _sender.Send(command);

            var notification = new ProductCreatedNotification
            {
                ProductId = command.ProductId,
                ProductName = command.ProductName
            };
            await _mediator.Publish(notification);

            return CreatedAtAction(nameof(Get), new { id = command.ProductId }, null);
        }

        /// <summary>
        /// Updates the product using the specified command
        /// </summary>
        /// <param name="id">The product id</param>
        /// <param name="command">The command</param>
        /// <returns>A task containing the action result</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductCommand command)
        {
            command.ProductId = id;
            var product = await _sender.Send(command);
            return Ok(product);
        }

        /// <summary>
        /// Deletes the product by id
        /// </summary>
        /// <param name="id">The product id</param>
        /// <returns>A task containing the action result</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var command = new DeleteProductCommand { ProductId = id };
            await _sender.Send(command);
            return NoContent();
        }
    }
}