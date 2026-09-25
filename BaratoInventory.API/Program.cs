using BaratoInventory.API.Middleware;
using BaratoInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Enable CORS for Blazor WebAssembly client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register Exception Handler & Problem Details
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Barato Product Inventory App",
        Version = "v1",
        Description = "Enterprise RESTful Web API for Barato Product Inventory App with SQL Server & Redis caching."
    });
});

// Register DbContext with specific Barato connection string
var connectionString = builder.Configuration.GetConnectionString("BaratoDbConnection") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// Register Redis Cache
var redisConfig = builder.Configuration.GetSection("Redis:Configuration").Value;
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConfig ?? "localhost:6379,abortConnect=false"));

builder.Services.AddSingleton<BaratoInventory.Core.Interfaces.ICacheService, BaratoInventory.Infrastructure.Services.RedisCacheService>();

// Register ProductService
builder.Services.AddScoped<BaratoInventory.Core.Interfaces.IProductService, BaratoInventory.Infrastructure.Services.ProductService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Use Exception Handler
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Barato Product Inventory App v1");
        options.DocumentTitle = "Barato Product Inventory App - Swagger UI";
    });
}

app.UseCors("AllowBlazorClient");

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
