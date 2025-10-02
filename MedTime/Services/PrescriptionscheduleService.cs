using AutoMapper;
using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MedTime.Services
{
    public class PrescriptionscheduleService
    {
        private readonly PrescriptionscheduleRepo _repo;
        private readonly IMapper _mapper;

        public PrescriptionscheduleService(PrescriptionscheduleRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<PrescriptionscheduleDto>> GetAllAsync(int pageNumber, int pageSize, int? filterUserId = null)
        {
            var query = _repo.GetAllQuery();
            
            // Filter theo Prescription.Userid
            if (filterUserId.HasValue)
            {
                query = query.Where(s => s.Prescription.Userid == filterUserId.Value);
            }

            var paginatedEntities = await query.ToPaginatedListAsync(pageNumber, pageSize);
            var dtoItems = _mapper.Map<List<PrescriptionscheduleDto>>(paginatedEntities.Items);

            return new PaginatedResult<PrescriptionscheduleDto>(
                dtoItems,
                paginatedEntities.TotalCount,
                paginatedEntities.PageNumber,
                paginatedEntities.PageSize
            );
        }

        /// <summary>
        /// Kiểm tra user có quyền truy cập schedule này không (qua Prescription.Userid)
        /// </summary>
        public async Task<bool> CheckUserAccessAsync(int scheduleId, int userId)
        {
            var schedule = await _repo.GetByIdAsync(scheduleId);
            if (schedule == null) return false;
            
            // Cần load Prescription để check Userid
            var query = _repo.GetAllQuery().Where(s => s.Scheduleid == scheduleId);
            var scheduleWithPrescription = await query.FirstOrDefaultAsync();
            
            return scheduleWithPrescription?.Prescription?.Userid == userId;
        }

        public async Task<PrescriptionscheduleDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<PrescriptionscheduleDto>(entity);
        }

        public async Task<PrescriptionscheduleDto> CreateAsync(PrescriptionscheduleCreate request)
        {
            var entity = _mapper.Map<Prescriptionschedule>(request);
            var createdEntity = await _repo.CreateAsync(entity);
            return _mapper.Map<PrescriptionscheduleDto>(createdEntity);
        }

        public async Task<bool> UpdateAsync(int id, PrescriptionscheduleUpdate request)
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
