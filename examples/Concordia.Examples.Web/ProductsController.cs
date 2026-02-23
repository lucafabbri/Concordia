using Microsoft.AspNetCore.Mvc;

// REST API controller demonstrating full CRUD database management via Concordia mediator
namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// REST API controller for product database management.
    /// Demonstrates full CRUD operations using the Concordia mediator pattern.
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
        /// Returns all products from the database.
        /// </summary>
        /// <returns>A task containing the action result with the product list</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _sender.Send(new ListProductsQuery());
            return Ok(products);
        }

        /// <summary>
        /// Returns the product with the specified identifier.
        /// </summary>
        /// <param name="id">The product identifier</param>
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
        /// Creates a new product in the database.
        /// </summary>
        /// <param name="command">The create product command</param>
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
        /// Updates an existing product in the database.
        /// </summary>
        /// <param name="id">The product identifier</param>
        /// <param name="command">The update product command</param>
        /// <returns>A task containing the action result</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductCommand command)
        {
            command.ProductId = id;
            await _sender.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Deletes the product with the specified identifier from the database.
        /// </summary>
        /// <param name="id">The product identifier</param>
        /// <returns>A task containing the action result</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _sender.Send(new DeleteProductCommand { ProductId = id });
            return NoContent();
        }
    }
}