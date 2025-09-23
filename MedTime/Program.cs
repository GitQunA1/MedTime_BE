using MedTime.Models.Enums;
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

