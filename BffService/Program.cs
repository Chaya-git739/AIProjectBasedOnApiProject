using BffService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IOrderClient, OrderClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(1);
});

builder.Services.AddHttpClient<IProductClient, ProductClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(1);
});

builder.Services.AddScoped<IAggregationService, AggregationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
app.MapControllers();

app.Run();

public partial class Program { }
