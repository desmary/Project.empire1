using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ImperialHR.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// =====================
// 1) Controllers + Swagger
// =====================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ImperialHR.Api", Version = "v1" });

    // ✅ Swagger Authorize (Bearer token)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Встав так: Bearer {твій JWT токен}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =====================
// 2) CORS (щоб фронт міг стукатись в API)
// =====================
// Найпростіший варіант: дозволяємо все (для курсової/локально ОК)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            // якщо не використовуєш cookies — можна прибрати AllowCredentials
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

// =====================
// 3) Database (EF Core)
// =====================
// appsettings.json має містити ConnectionStrings:Default
// приклад: "Default": "Server=(localdb)\\MSSQLLocalDB;Database=ImperialHR;Trusted_Connection=True;TrustServerCertificate=True"
builder.Services.AddDbContext<ImperialHrDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

// =====================
// 4) JWT Auth
// =====================
// appsettings.json має містити:
// "Jwt": { "Key": "...", "Issuer": "ImperialHR", "Audience": "ImperialHR" }
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new Exception("Jwt:Key не заданий в appsettings.json");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // локально ок
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// =====================
// Build app
// =====================
var app = builder.Build();

// =====================
// Pipeline
// =====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ порядок важливий
app.UseRouting();

app.UseCors("Frontend");          // <-- ДО auth

app.UseAuthentication();          // <-- спочатку Auth
app.UseAuthorization();           // <-- потім Authorization

app.MapControllers();

app.Run();
