using AutoMapper;
using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Repositories;

namespace MedTime.Services
{
    public class AppointmentService
    {
        private readonly AppointmentRepo _repo;
        private readonly IMapper _mapper;
        public AppointmentService(AppointmentRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<AppointmentDto>> GetAllAsync(int pageNumber, int pageSize, int? filterUserId = null)
        {
            var query = _repo.GetAllQuery();
            
            if (filterUserId.HasValue)
            {
                query = query.Where(a => a.Userid == filterUserId.Value);
            }

            var paginatedEntities = await query.ToPaginatedListAsync(pageNumber, pageSize);

            var dtoItems = _mapper.Map<List<AppointmentDto>>(paginatedEntities.Items);

            return new PaginatedResult<AppointmentDto>(
                dtoItems,
                paginatedEntities.TotalCount,
                paginatedEntities.PageNumber,
                paginatedEntities.PageSize
            );
        }

        public async Task<AppointmentDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;
            var dto = _mapper.Map<AppointmentDto>(entity);
            return dto;
        }

        public async Task<AppointmentDto> CreateAsync(AppointmentCreate request, int userId)
        {
            var entity = _mapper.Map<Appointment>(request);
            entity.Userid = userId;

            var createdEntity = await _repo.CreateAsync(entity);
            var createdDto = _mapper.Map<AppointmentDto>(createdEntity);
            return createdDto;
        }

        public async Task<bool> UpdateAsync(int id, AppointmentUpdate request, int userId)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            if (existing.Userid != userId) return false;

            _mapper.Map(request, existing);

            return await _repo.UpdateAsync(id, existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            return await _repo.Delete(id);
        }
    }
}
