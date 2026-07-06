using Microsoft.EntityFrameworkCore; // מייבא EF Core
using WebApplication2.BLL; // מייבא BLL
using WebApplication2.DAL; // מייבא DAL
using WebApplication2.Models; // מייבא Models
using Microsoft.AspNetCore.Authentication.JwtBearer; // מייבא הגדרות JWT
using Microsoft.IdentityModel.Tokens; // מייבא סוגי טוקן
using Microsoft.OpenApi.Models; // מייבא סוגי OpenAPI
using System.Text; // מייבא Encoding
using AutoMapper; // מייבא AutoMapper
using WebApplication2.Extensions; // מייבא Extension Methods עבור Middlewares
using System.Text.Json.Serialization; // מייבא JsonNamingPolicy
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using WebApplication2.Services;

// -----------------------------
// Program.cs – תמצית ותפקיד הקובץ
// -----------------------------
// קובץ האתחול של היישום (entry point) ב־ASP.NET Core (Razor Pages).
// - מקים WebApplicationBuilder ומשתמש ב־DI להוספה שירותים (Services).
// - מגדיר אמצעי אימות/אבטחה (Authentication / Authorization).
// - מגדיר Swagger לפיתוח/תיעוד API.
// - בונה שרשרת מידלוואר (Middleware) ומתחיל את היישום באמצעות app.Run().
// -----------------------------

var builder = WebApplication.CreateBuilder(args);

// מפתח סימטרי לשימוש בחתימת JWT
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyHere1234567890!";
var key = Encoding.ASCII.GetBytes(jwtSecretKey);

// -----------------------------
// Authentication (אימות זהות)
// -----------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
});

// -----------------------------
// Swagger / OpenAPI
// -----------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My Auction API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "אנא הכנס את ה-Token בפורמט הזה: Bearer {your_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

// -----------------------------
// שירותים נוספים (DI) והרשאות AutoMapper/DbContext
// -----------------------------
builder.Services.AddAutoMapper(cfg => { }, typeof(WebApplication2.Mappings.GiftMappingProfile));
builder.Services.AddDbContext<StoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        );
    }
));

// -----------------------------
// רישום שירותי DAL/BLL ב־Dependency Injection
// -----------------------------
builder.Services.AddScoped<IGiftDal, GiftDAL>();
builder.Services.AddScoped<IGiftBLL, GiftServiceBLL>();
builder.Services.AddScoped<IDonorDal, DonorDAL>();
builder.Services.AddScoped<IDonorBLL, DonorServiceBLL>();
builder.Services.AddScoped<ICategoryDal, CategoryDAL>();
builder.Services.AddScoped<ICategoryBLL, CategoryServiceBLL>();
builder.Services.AddScoped<IOrderDal, OrderDAL>();
builder.Services.AddScoped<IOrderBLL, OrderServiceBLL>();
builder.Services.AddScoped<RaffleSarviceBLL>();
builder.Services.AddScoped<IWinnerBLL, WinnerBLL>();

builder.Services.AddScoped<IWinnerDAL, WinnerDal>(provider =>
{
    var context = provider.GetRequiredService<StoreContext>();
    var mapper = provider.GetRequiredService<IMapper>();
    var logger = provider.GetRequiredService<ILogger<WinnerDal>>();
    return new WinnerDal(context, mapper, logger);
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

var redisConnection = builder.Configuration.GetValue<string>("Redis:Connection") ?? "redis:6379";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "MyApi_";
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect(redisConnection);
});

builder.Services.AddSingleton<IRedisInventoryService, RedisInventoryService>();
// SalesSummaryCacheService depends on IGiftBLL (scoped). Use scoped lifetime to avoid captive dependency issues.
builder.Services.AddScoped<ISalesSummaryCacheService, SalesSummaryCacheService>();
builder.Services.AddScoped<ITicketPurchaseService, TicketPurchaseService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddRazorPages();

// הוספת CORS לאנגולר
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// -----------------------------
// בנייה והרצת היישום
// -----------------------------
var app = builder.Build();

app.UseCustomExceptionHandling();
app.UseRequestResponseLogging();

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }
// else
// {
    // app.UseExceptionHandler("/Error");
    // app.UseHsts();
// }

app.UseCors("AllowAngular");
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// קוד שמוודא שמיגרציות רצות אוטומטית כשהאפליקציה עולה
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<StoreContext>(); // שמי לב לשנות ל-DbContext שלך
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();