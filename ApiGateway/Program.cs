var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.Use(async (context, next) =>
{
    const string correlationHeader = "x-correlation-id";
    var correlationId = context.Request.Headers[correlationHeader].FirstOrDefault();

    if (string.IsNullOrWhiteSpace(correlationId))
    {
        correlationId = Guid.NewGuid().ToString();
        context.Request.Headers[correlationHeader] = correlationId;
    }

    context.Response.Headers[correlationHeader] = correlationId;
    await next();
});

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ApiGateway" }));
app.MapGet("/", () => Results.Ok(new { service = "ApiGateway", status = "running" }));

app.MapReverseProxy();

app.Run();