using CatalogService.Data;
using CatalogService.Mappings;
using CatalogService.Services;
using MongoDB.Driver;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Jwt:SecretKey is not configured. Set it via environment variable Jwt__SecretKey.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
if (string.IsNullOrWhiteSpace(jwtIssuer))
    throw new InvalidOperationException("Jwt:Issuer is not configured.");
var jwtAudience = builder.Configuration["Jwt:Audience"];
if (string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("Jwt:Audience is not configured.");
var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog Service", Version = "v1" });
});

builder.Services.AddHealthChecks();
builder.Services.AddAutoMapper(cfg => { }, typeof(CatalogMappingProfile));

var mongoConnectionString = builder.Configuration.GetConnectionString("CatalogMongo")
    ?? builder.Configuration["Mongo:ConnectionString"]
    ?? "mongodb://mongo:27017";
var mongoDatabaseName = builder.Configuration["Mongo:Database"] ?? "CatalogDb";

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));

builder.Services.AddScoped<ICategoryRepository, MongoCategoryRepository>();
builder.Services.AddScoped<IDonorRepository, MongoDonorRepository>();
builder.Services.AddScoped<IGiftRepository, MongoGiftRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IGiftService, GiftService>();
builder.Services.AddScoped<IDonorService, DonorService>();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var instanceId = Environment.GetEnvironmentVariable("HOSTNAME") ?? Environment.MachineName;
    context.Response.Headers["X-Instance-Id"] = instanceId;
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/ready", () => Results.Ok(new { status = "ready" }));

app.Run();
