using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using TodoApi.Exceptions;
using TodoApi.Models;
using TodoApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// ----------------------
// API Performance & Caching
// ----------------------

// Enables caching of HTTP responses based on request headers (e.g., Cache-Control, Vary)
// Improves performance by avoiding repeated execution for identical requests
builder.Services.AddResponseCaching();

// Adds support for automatic response compression (e.g., Gzip, Brotli)
// Reduces payload size to improve bandwidth efficiency and speed
builder.Services.AddResponseCompression();

// Registers in-memory caching services
// Useful for caching data like database results or frequently accessed configuration values at server-side
builder.Services.AddMemoryCache();

// Adds support for discovering endpoint metadata for Swagger/OpenAPI
// Required for tools like Swashbuckle to generate API documentation
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Add versioning support (see below)
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Todo API",
        Version = "v1",
        Description = "An API for managing tasks, deadlines, and priorities.",
        Contact = new OpenApiContact
        {
            Name = "Ahmed Hassan",
            Email = "ahmed.hassan398@outlook.com"
        }
    });
});
// -------------------------
// Custom Exception Handlers
// -------------------------

// Registers a custom exception handler for DateException
// Returns a 400 Bad Request response when a date format or value is invalid
builder.Services.AddExceptionHandler<DateExceptionHandler>();

// Registers a custom exception handler for ResourceNotFoundException
// Returns a 404 Not Found response when a requested resource is not found
builder.Services.AddExceptionHandler<ResourceNotFoundExceptionHandler>();

// Registers a custom exception handler for BusinessRuleViolationException
// Returns a 422 Unprocessable Entity response for domain-specific business logic violations
builder.Services.AddExceptionHandler<BusinessRuleViolationExceptionHandler>();

// Registers a global fallback exception handler for all unhandled exceptions
// Returns a 500 Internal Server Error response with a generic error structure
builder.Services.AddExceptionHandler<ApiException>();


// Build a temporary service provider to access the configuration system
// This is used to retrieve the connection string before the final service provider is built
var provider = builder.Services.BuildServiceProvider();
var config = provider.GetRequiredService<IConfiguration>();

// Register the Entity Framework DbContext using SQL Server
// The connection string is retrieved from appsettings.json under "ConnectionStrings:dbcs"
builder.Services.AddDbContext<TodoApiContext>(options =>
    options.UseSqlServer(config.GetConnectionString("dbcs"))
);

// Register a hosted background service that periodically checks for overdue tasks
builder.Services.AddHostedService<OverdueCheckerService>();

// Build the final application pipeline
var app = builder.Build();

// Register the centralized exception handling middleware
// Note: This requires proper exception handlers to be registered with DI
app.UseExceptionHandler(_ => { }); // Exception handling logic is added through registered handlers

// Configure the Swagger UI and documentation, only in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Serve the generated Swagger document
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
        options.DocumentTitle = "Todo API Docs"; // Title of the Swagger UI page
    });
}

// Redirect all HTTP requests to HTTPS for secure communication
app.UseHttpsRedirection();

// Enables response compression middleware (e.g., gzip, brotli) to reduce payload size
app.UseResponseCompression();

// Enables response caching based on cache-control headers to improve performance
app.UseResponseCaching();

// Applies the default authorization policy, if any (can be extended with app.UseAuthentication if needed)
app.UseAuthorization();

// Maps controller routes (e.g., api/[controller]) to available endpoints
app.MapControllers();

// Starts the application and begins listening for incoming HTTP requests
app.Run();
