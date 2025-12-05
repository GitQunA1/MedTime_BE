using AutoMapper;
using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Repositories;

namespace MedTime.Services
{
    public class IntakelogService
    {
        private readonly IntakelogRepo _repo;
        private readonly PrescriptionRepo _prescriptionRepo;
        private readonly PrescriptionscheduleRepo _scheduleRepo;
        private readonly IMapper _mapper;

        public IntakelogService(
            IntakelogRepo repo, 
            PrescriptionRepo prescriptionRepo,
            PrescriptionscheduleRepo scheduleRepo,
            IMapper mapper)
        {
            _repo = repo;
            _prescriptionRepo = prescriptionRepo;
            _scheduleRepo = scheduleRepo;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<IntakelogDto>> GetAllAsync(int pageNumber, int pageSize, int? filterUserId = null, List<int>? additionalUserIds = null)
        {
            var query = _repo.GetAllQuery();
            
            // Filter theo Userid hoặc guardian's patients
            if (filterUserId.HasValue)
            {
                if (additionalUserIds != null && additionalUserIds.Count > 0)
                {
                    // Include cả user hiện tại và các patients của guardian
                    var allUserIds = new List<int> { filterUserId.Value };
                    allUserIds.AddRange(additionalUserIds);
                    query = query.Where(i => allUserIds.Contains(i.Userid));
                }
                else
                {
                    query = query.Where(i => i.Userid == filterUserId.Value);
                }
            }

            // Order by newest first
            query = query.OrderByDescending(i => i.Remindertime);

            var paginatedEntities = await query.ToPaginatedListAsync(pageNumber, pageSize);
            var dtoItems = _mapper.Map<List<IntakelogDto>>(paginatedEntities.Items);

            return new PaginatedResult<IntakelogDto>(
                dtoItems,
                paginatedEntities.TotalCount,
                paginatedEntities.PageNumber,
                paginatedEntities.PageSize
            );
        }

        public async Task<IntakelogDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<IntakelogDto>(entity);
        }

        public async Task<IntakelogDto> CreateAsync(IntakelogCreate request, int userId)
        {
            var entity = _mapper.Map<Intakelog>(request);
            entity.Userid = userId; 

            //get ReminderTime from PrescriptionSchedule
            if (!request.Scheduleid.HasValue)
            {
                throw new ArgumentException("ScheduleId is required. Please select a schedule time.");
            }

            entity.Remindertime = await CalculateReminderTimeAsync(request.Scheduleid.Value);

            var createdEntity = await _repo.CreateAsync(entity);
            return _mapper.Map<IntakelogDto>(createdEntity);
        }

        /// <summary>
        /// Lấy ReminderTime từ PrescriptionSchedule
        /// (Đơn giản hơn vì Prescription đã auto-generate schedules)
        /// </summary>
        private async Task<DateTime> CalculateReminderTimeAsync(int scheduleId)
        {
            var schedule = await _scheduleRepo.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                throw new ArgumentException($"PrescriptionSchedule with ID {scheduleId} not found.");
            }

            // Lấy ngày hôm nay + TimeOfDay từ schedule
            var today = DateOnly.FromDateTime(DateTime.Now);
            var reminderTime = today.ToDateTime(schedule.Timeofday);
            
            // Convert sang Unspecified để tương thích với PostgreSQL
            return DateTime.SpecifyKind(reminderTime, DateTimeKind.Unspecified);
        }

        public async Task<bool> UpdateAsync(int id, IntakelogUpdate request)
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
