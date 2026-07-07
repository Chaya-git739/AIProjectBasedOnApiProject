using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

var serviceName = builder.Configuration["ServiceName"] ?? "ApiGateway";
var seqUrl = builder.Configuration["Serilog:SeqUrl"] ?? "http://seq:5341";

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", serviceName)
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.Seq(seqUrl)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiGateway");
    options.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "AuthenticationService");
    options.SwaggerEndpoint("/orders/swagger/v1/swagger.json", "OrderService");
    options.SwaggerEndpoint("/catalog/swagger/v1/swagger.json", "CatalogService");
    options.SwaggerEndpoint("/notification/swagger/v1/swagger.json", "NotificationService");
    options.RoutePrefix = "swagger";
});

app.Use(async (context, next) =>
{
    const string correlationHeader = "x-correlation-id";
    var correlationId = context.Request.Headers[correlationHeader].FirstOrDefault();

    if (string.IsNullOrWhiteSpace(correlationId))
    {
        correlationId = Guid.NewGuid().ToString();
        context.Request.Headers[correlationHeader] = correlationId;
    }

    context.TraceIdentifier = correlationId;

    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[correlationHeader] = correlationId;
            return Task.CompletedTask;
        });

        await next();
    }
});

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ApiGateway" }));
app.MapGet("/", () => Results.Ok(new { service = "ApiGateway", status = "running" }));

app.MapReverseProxy();

app.Run();