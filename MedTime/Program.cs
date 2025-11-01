using MedTime.Data;
using MedTime.Helpers;
using MedTime.Models.Entities;
using MedTime.Models.Enums;
using MedTime.Repositories;
using MedTime.Services;
using MedTime.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));

// Existing enums
dataSourceBuilder.MapEnum<UserRoleEnum>("user_role");
dataSourceBuilder.MapEnum<MedicineTypeEnum>("medicine_type");
dataSourceBuilder.MapEnum<MedicineUnitEnum>("medicine_unit");
dataSourceBuilder.MapEnum<RepeatPatternEnum>("repeat_pattern");
dataSourceBuilder.MapEnum<DayOfWeekEnum>("day_of_week");
dataSourceBuilder.MapEnum<IntakeActionEnum>("intake_action");
dataSourceBuilder.MapEnum<ConfirmedByEnum>("confirmed_by");
dataSourceBuilder.MapEnum<CallStatusEnum>("call_status");
dataSourceBuilder.MapEnum<DeviceTypeEnum>("device_type");
dataSourceBuilder.MapEnum<NotificationStatusEnum>("notification_status");

// Payment enums - Use name translator to keep UPPERCASE format
dataSourceBuilder.MapEnum<PaymentStatusEnum>("payment_status", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<PremiumPlanTypeEnum>("premium_plan_type", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());

var dataSource = dataSourceBuilder.Build();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://medtime.app",
                "https://www.medtime.app"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Cấu hình để ENUMs serialize/deserialize dưới dạng STRING thay vì NUMBER
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter(
                System.Text.Json.JsonNamingPolicy.CamelCase, 
                allowIntegerValues: false
            )
        );
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true; // Tắt automatic model validation
    });
    
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MedTime API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Repositories
builder.Services.AddScoped<AppointmentRepo>();
builder.Services.AddScoped<CalllogRepo>();
builder.Services.AddScoped<EmergencycontactRepo>();
builder.Services.AddScoped<GuardianlinkRepo>();
builder.Services.AddScoped<IntakelogRepo>();
builder.Services.AddScoped<MedicineRepo>();
builder.Services.AddScoped<PrescriptionRepo>();
builder.Services.AddScoped<PrescriptionscheduleRepo>();
builder.Services.AddScoped<UserRepo>();
builder.Services.AddScoped<AuthRepo>();
builder.Services.AddScoped<DevicetokenRepo>();
builder.Services.AddScoped<NotificationhistoryRepo>();
builder.Services.AddScoped<ReportRepo>();
builder.Services.AddScoped<PremiumplanRepo>();
builder.Services.AddScoped<PaymenthistoryRepo>();

// Services
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<CalllogService>();
builder.Services.AddScoped<EmergencycontactService>();
builder.Services.AddScoped<GuardianlinkService>();
builder.Services.AddScoped<IntakelogService>();
builder.Services.AddScoped<MedicineService>();
builder.Services.AddScoped<PrescriptionService>();
builder.Services.AddScoped<PrescriptionscheduleService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenCacheService>();
builder.Services.AddSingleton<FirebaseService>();
builder.Services.AddScoped<DevicetokenService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<PaymentAnalyticsService>();

// Helpers & Auth
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddMemoryCache();

builder.Services.AddAutoMapper(typeof(MappingProfile));

// Dùng dataSource đã được config với enum mapping
builder.Services.AddDbContext<MedTimeDBContext>(options =>
{
    options.UseNpgsql(dataSource)
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.Configure<PayOSSettings>(options =>
{
    builder.Configuration.GetSection("PayOSSettings").Bind(options);

    string? GetEnv(string key) => Environment.GetEnvironmentVariable(key);

    if (string.IsNullOrWhiteSpace(options.ClientId))
    {
        options.ClientId = GetEnv("PayOSSettings__ClientId") ?? string.Empty;
    }

    if (string.IsNullOrWhiteSpace(options.ApiKey))
    {
        options.ApiKey = GetEnv("PayOSSettings__ApiKey") ?? string.Empty;
    }

    if (string.IsNullOrWhiteSpace(options.ChecksumKey))
    {
        options.ChecksumKey = GetEnv("PayOSSettings__ChecksumKey") ?? string.Empty;
    }

    if (string.IsNullOrWhiteSpace(options.ReturnUrl))
    {
        options.ReturnUrl = GetEnv("PayOSSettings__ReturnUrl") ?? string.Empty;
    }

    if (string.IsNullOrWhiteSpace(options.CancelUrl))
    {
        options.CancelUrl = GetEnv("PayOSSettings__CancelUrl") ?? string.Empty;
    }

    if (string.IsNullOrWhiteSpace(options.WebhookUrl))
    {
        options.WebhookUrl = GetEnv("PayOSSettings__WebhookUrl") ?? string.Empty;
    }

    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        options.BaseUrl = GetEnv("PayOSSettings__BaseUrl") ?? "https://api-merchant.payos.vn";
    }
});

// Add HttpClient for PayOS API calls
builder.Services.AddHttpClient();

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
    };
});

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// To Deploy
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");
Console.WriteLine($"Listening on port {port}");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MedTime API V1");
    c.RoutePrefix = string.Empty; 
});
//

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

