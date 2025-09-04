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
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ISender _sender;

        public ProductsController(IMediator mediator, ISender sender)
        {
            _mediator = mediator;
            _sender = sender;
        }

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
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class GetProductByIdQuery : IRequest<ProductDto>
    {
        public int ProductId { get; set; }
    }

    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
    {
        public Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Handling GetProductByIdQuery for ProductId: {request.ProductId}");
            var product = new ProductDto { Id = request.ProductId, Name = $"Product {request.ProductId}", Price = 10.50m };
            return Task.FromResult(product);
        }
    }

    public class CreateProductCommand : IRequest
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand>
    {
        public Task Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Creating product: {request.ProductName} with ID: {request.ProductId}");
            return Task.CompletedTask;
        }
    }

    public class ProductCreatedNotification : INotification
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }

    public class SendEmailOnProductCreated : INotificationHandler<ProductCreatedNotification>
    {
        public Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Sending email for new product: {notification.ProductName} (Id: {notification.ProductId})");
            return Task.CompletedTask;
        }
    }

    public class LogProductCreation : INotificationHandler<ProductCreatedNotification>
    {
        public Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Logging product creation: {notification.ProductName} (Id: {notification.ProductId}) created at {DateTime.Now}");
            return Task.CompletedTask;
        }
    }

    public class HelloCommand : IRequest<string>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class HelloCommandHandler : IRequestHandler<HelloCommand, string>
    {
        public Task<string> Handle(HelloCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Hello, {request.Name}!");
        }
    }
}