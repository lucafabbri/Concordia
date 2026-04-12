using Synaptrix;
using Synaptrix.Examples.Web;
using Synaptrix.Examples.Web.Generated;
using Microsoft.OpenApi;
using System.Reflection; // Namespace for the generated registrations

var builder = WebApplication.CreateBuilder(args);

// Add this block to configure Swagger
// Start Swagger configuration
builder.Services.AddEndpointsApiExplorer(); // Necessary for Swagger in .NET 6+
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Synaptrix API", // You can change your API title here
        Description = "An example API using Synaptrix",
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

// Register Synaptrix and all handlers using the reflection-based AddMediator method.
// It now accepts a configuration action, similar to MediatR, but using our internal class.
builder.Services.AddSynaptrixCoreServices();
builder.Services.AddSynaptrixHandlers();
builder.Services.AddControllers();

var app = builder.Build();

// Add this block to enable Swagger UI
// Start Swagger UI enablement
if (app.Environment.IsDevelopment()) // It's good practice to enable Swagger only in development environment
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Synaptrix API v1");
        // If you want Swagger UI to be accessible directly from your root URL (e.g., http://localhost:5000 instead of http://localhost:5000/swagger):
        // options.RoutePrefix = string.Empty;
    });
}
// End Swagger UI enablement

app.MapControllers();

app.Run();