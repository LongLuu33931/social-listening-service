using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Coka.Social.Listening.Core.Interfaces.Repositories;
using Coka.Social.Listening.Core.Interfaces.Services;
using Coka.Social.Listening.Infra.Data;
using Coka.Social.Listening.Infra.Repositories;
using Coka.Social.Listening.Infra.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── AutoMapper ────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(Coka.Social.Listening.API.Mappings.MappingProfile));


// ─── Database ──────────────────────────────────────────────────────────
builder.Services.AddSingleton<DbConnectionFactory>();

// ─── Repositories ──────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IProvinceRepository, ProvinceRepository>();

// ─── Services ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProvinceService, ProvinceService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<IMentionService, MentionService>();

// ─── Redis ─────────────────────────────────────────────────────────────
builder.Services.AddSingleton<Coka.Social.Listening.Infra.Data.RedisConnectionFactory>();
builder.Services.AddSingleton<Coka.Social.Listening.Infra.Helpers.RedisHelper>();
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

// ─── JWT Authentication ────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var secret = jwtSection["Secret"]
    ?? throw new InvalidOperationException("JWT Secret not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = jwtSection["ValidAudience"],
        ValidIssuer = jwtSection["ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ─── Controllers ───────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─── OpenAPI (.NET 10 built-in) ────────────────────────────────────────
builder.Services.AddOpenApi();

// ─── CORS ──────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ─── Middleware Pipeline ───────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Coka Social Listening API")
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// ─── Request Logging Middleware ────────────────────────────────────────
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
        .CreateLogger("RequestLogger");

    var method = context.Request.Method;
    var path = context.Request.Path;
    var query = context.Request.QueryString;

    // Read request body
    context.Request.EnableBuffering();
    var body = "";
    if (context.Request.ContentLength > 0)
    {
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;
    }

    logger.LogInformation("→ {Method} {Path}{Query}", method, path, query);
    if (!string.IsNullOrEmpty(body))
        logger.LogInformation("  Body: {Body}", body.Length > 2000 ? body[..2000] + "...(truncated)" : body);

    await next();

    logger.LogInformation("← {Method} {Path} → {StatusCode}", method, path, context.Response.StatusCode);
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
