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

var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
dataSourceBuilder.MapEnum<UserRoleEnum>("user_role");
dataSourceBuilder.MapEnum<MedicineTypeEnum>("medicine_type");
dataSourceBuilder.MapEnum<MedicineUnitEnum>("medicine_unit");
dataSourceBuilder.MapEnum<RepeatPatternEnum>("repeat_pattern");
dataSourceBuilder.MapEnum<DayOfWeekEnum>("day_of_week");
dataSourceBuilder.MapEnum<IntakeActionEnum>("intake_action");
dataSourceBuilder.MapEnum<ConfirmedByEnum>("confirmed_by");
dataSourceBuilder.MapEnum<CallStatusEnum>("call_status");
var dataSource = dataSourceBuilder.Build();

builder.Services.AddControllers()
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
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
builder.Services.AddScoped<AuthRepo>();

// Services
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<CalllogService>();
builder.Services.AddScoped<EmergencycontactService>();
builder.Services.AddScoped<GuardianlinkService>();
builder.Services.AddScoped<IntakelogService>();
builder.Services.AddScoped<MedicineService>();
builder.Services.AddScoped<PrescriptionService>();
builder.Services.AddScoped<PrescriptionscheduleService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenCacheService>();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

