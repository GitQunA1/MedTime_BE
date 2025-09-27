using AutoMapper;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Enums;
using MedTime.Models.Requests;

namespace MedTime.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Convert cho DateTime, DataTime?
            CreateMap<DateTime, DateTime>()
                .ConvertUsing(src => DateTime.SpecifyKind(src, DateTimeKind.Unspecified));
            CreateMap<DateTime?, DateTime?>()
                .ConvertUsing(src => src.HasValue
                    ? DateTime.SpecifyKind(src.Value, DateTimeKind.Unspecified)
                    : (DateTime?)null);

            // Enum conversions
            CreateMap<string, UserRoleEnum>().ConvertUsing(str =>
                Enum.Parse<UserRoleEnum>(str.ToUpper(), true));
            CreateMap<UserRoleEnum, string>().ConvertUsing(role =>
                role.ToString().ToUpper());

            // Các map khác
            CreateMap<Appointment, AppointmentDto>();
            CreateMap<AppointmentDto, Appointment>();
            CreateMap<Appointment, AppointmentCreate>();
            CreateMap<AppointmentCreate, Appointment>();
            CreateMap<Appointment, AppointmentUpdate>();
            CreateMap<AppointmentUpdate, Appointment>();
            CreateMap<User, UserDto>();

        }
    }

}
