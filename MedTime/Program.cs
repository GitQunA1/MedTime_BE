using MedTime.Data;
using MedTime.Helpers;
using MedTime.Models.Enums;
using MedTime.Repositories;
using MedTime.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

NpgsqlConnection.GlobalTypeMapper.MapEnum<UserRoleEnum>();
NpgsqlConnection.GlobalTypeMapper.MapEnum<MedicineTypeEnum>();
NpgsqlConnection.GlobalTypeMapper.MapEnum<MedicineUnitEnum>();
NpgsqlConnection.GlobalTypeMapper.MapEnum<RepeatPatternEnum>();
NpgsqlConnection.GlobalTypeMapper.MapEnum<DayOfWeekEnum>();
NpgsqlConnection.GlobalTypeMapper.MapEnum<IntakeActionEnum>();
NpgsqlConnection.GlobalTypeMapper.MapEnum<ConfirmedByEnum>();
NpgsqlConnection.GlobalTypeMapper.MapEnum<CallStatusEnum>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<AppointmentRepo>();

builder.Services.AddScoped<AppointmentService>();

builder.Services.AddAutoMapper(typeof(MappingProfile));



builder.Services.AddDbContext<MedTimeDBContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

