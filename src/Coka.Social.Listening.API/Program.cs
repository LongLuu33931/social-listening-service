using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Coka.Social.Listening.Core.Interfaces;
using Coka.Social.Listening.Infra.Data;
using Coka.Social.Listening.Infra.Repositories;
using Coka.Social.Listening.Infra.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── AutoMapper ────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(Coka.Social.Listening.API.Mappings.MappingProfile));

// ─── Database ──────────────────────────────────────────────────────────
builder.Services.AddSingleton<DbConnectionFactory>();

// ─── Repositories & Services ───────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

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
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new Coka.Social.Listening.API.Converters.DateOnlyJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new Coka.Social.Listening.API.Converters.NullableDateOnlyJsonConverter());
    });

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

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
