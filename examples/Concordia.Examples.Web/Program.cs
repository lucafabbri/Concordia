using Concordia.MediatR; // Namespace for the AddMediator method
using Microsoft.AspNetCore.Mvc;
using System.Reflection; // Needed for Assembly.GetExecutingAssembly()
using Microsoft.OpenApi.Models; // Add this using for OpenApiInfo
using Concordia.Examples.Web; // Namespace for the generated registrations

var builder = WebApplication.CreateBuilder(args);

// Add this block to configure Swagger
// Start Swagger configuration
builder.Services.AddEndpointsApiExplorer(); // Necessary for Swagger in .NET 6+
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Concordia API", // You can change your API title here
        Description = "An example API using Concordia.MediatR",
        Contact = new OpenApiContact
        {
            Name = "Your Name/Company",
            Url = new Uri("https://example.com/contact") // Replace with your URL
        }
    });

    // Optional: Enable XML comments for endpoint documentation
    // To make this work, you need to enable XML file generation
    // in your project properties (Build -> Output -> XML documentation file).
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
// End Swagger configuration

// Register Concordia and all handlers using the reflection-based AddMediator method.
// It now accepts a configuration action, similar to MediatR, but using our internal class.
builder.Services.AddMediator(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

    // Example: Register all services as Scoped
    cfg.Lifetime = ServiceLifetime.Scoped;

    // Example: Register a custom notification publisher
    // cfg.NotificationPublisherType = typeof(MyCustomNotificationPublisher);

    // Example: Add an explicit pre-processor
    // cfg.AddRequestPreProcessor<MyCustomPreProcessor>();

    // Example: Add an explicit post-processor
    // cfg.AddRequestPostProcessor<MyCustomPostProcessor>();

    // Example: Add an explicit stream behavior
    // cfg.AddStreamBehavior<MyCustomStreamBehavior>();
});
builder.Services.AddControllers();
builder.Services.AddMyCustomHandlers();  

var app = builder.Build();

// Add this block to enable Swagger UI
// Start Swagger UI enablement
if (app.Environment.IsDevelopment()) // It's good practice to enable Swagger only in development environment
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Concordia API v1");
        // If you want Swagger UI to be accessible directly from your root URL (e.g., http://localhost:5000 instead of http://localhost:5000/swagger):
        // options.RoutePrefix = string.Empty;
    });
}
// End Swagger UI enablement

app.MapControllers();

app.Run();

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
    }

    // Examples of requests, commands, notifications, and handlers for the web project
    /// <summary>
    /// The product dto class
    /// </summary>
    public class ProductDto
    {
        /// <summary>
        /// Gets or sets the value of the id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the value of the name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the value of the price
        /// </summary>
        public decimal Price { get; set; }
    }

    /// <summary>
    /// The get product by id query class
    /// </summary>
    /// <seealso cref="IRequest{ProductDto}"/>
    public class GetProductByIdQuery : IRequest<ProductDto>
    {
        /// <summary>
        /// Gets or sets the value of the product id
        /// </summary>
        public int ProductId { get; set; }
    }

    /// <summary>
    /// The get product by id query handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{GetProductByIdQuery, ProductDto}"/>
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the product dto</returns>
        public Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Handling GetProductByIdQuery for ProductId: {request.ProductId}");
            var product = new ProductDto { Id = request.ProductId, Name = $"Product {request.ProductId}", Price = 10.50m };
            return Task.FromResult(product);
        }
    }

    /// <summary>
    /// The create product command class
    /// </summary>
    /// <seealso cref="IRequest"/>
    public class CreateProductCommand : IRequest
    {
        /// <summary>
        /// Gets or sets the value of the product id
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Gets or sets the value of the product name
        /// </summary>
        public string ProductName { get; set; }
    }

    /// <summary>
    /// The create product command handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{CreateProductCommand}"/>
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Creating product: {request.ProductName} with ID: {request.ProductId}");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// The product created notification class
    /// </summary>
    /// <seealso cref="INotification"/>
    public class ProductCreatedNotification : INotification
    {
        /// <summary>
        /// Gets or sets the value of the product id
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Gets or sets the value of the product name
        /// </summary>
        public string ProductName { get; set; }
    }

    /// <summary>
    /// The send email on product created class
    /// </summary>
    /// <seealso cref="INotificationHandler{ProductCreatedNotification}"/>
    public class SendEmailOnProductCreated : INotificationHandler<ProductCreatedNotification>
    {
        /// <summary>
        /// Handles the notification
        /// </summary>
        /// <param name="notification">The notification</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Sending email for new product: {notification.ProductName} (Id: {notification.ProductId})");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// The log product creation class
    /// </summary>
    /// <seealso cref="INotificationHandler{ProductCreatedNotification}"/>
    public class LogProductCreation : INotificationHandler<ProductCreatedNotification>
    {
        /// <summary>
        /// Handles the notification
        /// </summary>
        /// <param name="notification">The notification</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Logging product creation: {notification.ProductName} (Id: {notification.ProductId}) created at {DateTime.Now}");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// The hello command class
    /// </summary>
    /// <seealso cref="IRequest{string}"/>
    public class HelloCommand : IRequest<string>
    {
        /// <summary>
        /// Gets or sets the value of the name
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// The hello command handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{HelloCommand, string}"/>
    public class HelloCommandHandler : IRequestHandler<HelloCommand, string>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the string</returns>
        public Task<string> Handle(HelloCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Hello, {request.Name}!");
        }
    }
}