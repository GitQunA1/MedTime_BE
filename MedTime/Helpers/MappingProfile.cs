using AutoMapper;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Requests;

namespace MedTime.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Convert cho DateTime
            CreateMap<DateTime, DateTime>()
                .ConvertUsing(src => DateTime.SpecifyKind(src, DateTimeKind.Unspecified));

            // Convert cho DateTime?
            CreateMap<DateTime?, DateTime?>()
                .ConvertUsing(src => src.HasValue
                    ? DateTime.SpecifyKind(src.Value, DateTimeKind.Unspecified)
                    : (DateTime?)null);

            // Các map khác
            CreateMap<Appointment, AppointmentDto>();
            CreateMap<AppointmentDto, Appointment>();
            CreateMap<Appointment, AppointmentCreate>();
            CreateMap<AppointmentCreate, Appointment>();
            CreateMap<Appointment, AppointmentUpdate>();
            CreateMap<AppointmentUpdate, Appointment>();
        }
    }

}
