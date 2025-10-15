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

            // Calllog mappings
            CreateMap<Calllog, CalllogDto>();
            CreateMap<CalllogDto, Calllog>();
            CreateMap<CalllogCreate, Calllog>();
            CreateMap<CalllogUpdate, Calllog>();

            // Emergencycontact mappings
            CreateMap<Emergencycontact, EmergencycontactDto>();
            CreateMap<EmergencycontactDto, Emergencycontact>();
            CreateMap<EmergencycontactCreate, Emergencycontact>();
            CreateMap<EmergencycontactUpdate, Emergencycontact>();

            // Guardianlink mappings
            CreateMap<Guardianlink, GuardianlinkDto>();
            CreateMap<GuardianlinkDto, Guardianlink>();
            CreateMap<GuardianlinkCreate, Guardianlink>()
                .ForMember(dest => dest.Createdat, opt => opt.MapFrom(src => DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)));
            CreateMap<GuardianlinkUpdate, Guardianlink>();

            // Intakelog mappings
            CreateMap<Intakelog, IntakelogDto>();
            CreateMap<IntakelogDto, Intakelog>();
            CreateMap<IntakelogCreate, Intakelog>();
            CreateMap<IntakelogUpdate, Intakelog>();

            // Medicine mappings
            CreateMap<Medicine, MedicineDto>();
            CreateMap<MedicineDto, Medicine>();
            CreateMap<MedicineCreate, Medicine>();
            CreateMap<MedicineUpdate, Medicine>();

            // Prescription mappings
            CreateMap<Prescription, PrescriptionDto>();
            CreateMap<PrescriptionDto, Prescription>();
            CreateMap<PrescriptionCreate, Prescription>();
            CreateMap<PrescriptionUpdate, Prescription>();

            // Prescriptionschedule mappings
            CreateMap<Prescriptionschedule, PrescriptionscheduleDto>();
            CreateMap<PrescriptionscheduleDto, Prescriptionschedule>();
            CreateMap<PrescriptionscheduleCreate, Prescriptionschedule>();
            CreateMap<PrescriptionscheduleUpdate, Prescriptionschedule>();

            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
            CreateMap<UserUpdate, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Devicetoken mappings
            CreateMap<Devicetoken, DevicetokenDto>();
            CreateMap<DevicetokenDto, Devicetoken>();

            // Notificationhistory mappings
            CreateMap<Notificationhistory, NotificationhistoryDto>();
            CreateMap<NotificationhistoryDto, Notificationhistory>();

        }
    }

}
