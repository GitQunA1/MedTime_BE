using AutoMapper;
using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Repositories;

namespace MedTime.Services
{
    public class PrescriptionService
    {
        private readonly PrescriptionRepo _repo;
        private readonly PrescriptionscheduleRepo _scheduleRepo;
        private readonly IMapper _mapper;

        public PrescriptionService(
            PrescriptionRepo repo, 
            PrescriptionscheduleRepo scheduleRepo,
            IMapper mapper)
        {
            _repo = repo;
            _scheduleRepo = scheduleRepo;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<PrescriptionDto>> GetAllAsync(int pageNumber, int pageSize, int? filterUserId = null)
        {
            var query = _repo.GetAllQuery();
            
            // validate Prescription of user
            if (filterUserId.HasValue)
            {
                query = query.Where(p => p.Userid == filterUserId.Value);
            }

            var paginatedEntities = await query.ToPaginatedListAsync(pageNumber, pageSize);
            var dtoItems = _mapper.Map<List<PrescriptionDto>>(paginatedEntities.Items);

            return new PaginatedResult<PrescriptionDto>(
                dtoItems,
                paginatedEntities.TotalCount,
                paginatedEntities.PageNumber,
                paginatedEntities.PageSize
            );
        }

        public async Task<PrescriptionDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<PrescriptionDto>(entity);
        }

        /// <summary>
        /// Kiểm tra prescription có thuộc về user không (để validate ownership)
        /// </summary>
        public async Task<bool> CheckPrescriptionOwnershipAsync(int prescriptionId, int userId)
        {
            var prescription = await _repo.GetByIdAsync(prescriptionId);
            return prescription?.Userid == userId;
        }

        public async Task<PrescriptionDto> CreateAsync(PrescriptionCreate request, int userId)
        {
            var entity = _mapper.Map<Prescription>(request);
            entity.Userid = userId;
            var createdEntity = await _repo.CreateAsync(entity);

            // Auto-generate PrescriptionSchedule nếu có FrequencyPerDay
            if (createdEntity.Frequencyperday.HasValue && createdEntity.Frequencyperday.Value > 0 && createdEntity.Frequencyperday.Value < 6)
            {
                await GenerateDefaultSchedulesAsync(createdEntity.Prescriptionid, createdEntity.Frequencyperday.Value);
            }

            return _mapper.Map<PrescriptionDto>(createdEntity);
        }

        /// <summary>
        /// Tự động tạo PrescriptionSchedule dựa trên FrequencyPerDay
        /// Ví dụ: FrequencyPerDay = 3 → Tạo 3 schedules (8h, 14h, 20h)
        /// </summary>
        private async Task GenerateDefaultSchedulesAsync(int prescriptionId, int frequencyPerDay)
        {
            var schedules = new List<Prescriptionschedule>();

            // Giờ bắt đầu: 8h sáng
            var startHour = 8;
            
            // Khoảng cách giữa các lần uống (giờ)
            // 1 lần/ngày → 8h
            // 2 lần/ngày → 8h, 20h (interval = 12h)
            // 3 lần/ngày → 8h, 14h, 20h (interval = 6h)
            // 4 lần/ngày → 8h, 12h, 16h, 20h (interval = 4h)
            var intervalHours = frequencyPerDay > 1 ? 12 / (frequencyPerDay - 1) : 0;

            for (int i = 0; i < frequencyPerDay; i++)
            {
                var hour = startHour + (i * intervalHours);
                
                // Đảm bảo không quá 23h
                if (hour > 23) hour = 23;

                var schedule = new Prescriptionschedule
                {
                    Prescriptionid = prescriptionId,
                    Timeofday = new TimeOnly(hour, 0),
                    RepeatPattern = Models.Enums.RepeatPatternEnum.DAILY,
                    Notificationenabled = true // Mặc định bật notification
                };

                schedules.Add(schedule);
            }

            // Lưu tất cả schedules vào database
            foreach (var schedule in schedules)
            {
                await _scheduleRepo.CreateAsync(schedule);
            }
        }

        public async Task<bool> UpdateAsync(int id, PrescriptionUpdate request)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(request, existing);
            await _repo.UpdateAsync(id, existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.Delete(id);
        }
    }
}
