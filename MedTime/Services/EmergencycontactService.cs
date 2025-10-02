using AutoMapper;
using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Repositories;

namespace MedTime.Services
{
    public class EmergencycontactService
    {
        private readonly EmergencycontactRepo _repo;
        private readonly IMapper _mapper;

        public EmergencycontactService(EmergencycontactRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<EmergencycontactDto>> GetAllAsync(int pageNumber, int pageSize, int? filterUserId = null)
        {
            var query = _repo.GetAllQuery();
            
            if (filterUserId.HasValue)
            {
                query = query.Where(e => e.Userid == filterUserId.Value);
            }

            var paginatedEntities = await query.ToPaginatedListAsync(pageNumber, pageSize);
            var dtoItems = _mapper.Map<List<EmergencycontactDto>>(paginatedEntities.Items);

            return new PaginatedResult<EmergencycontactDto>(
                dtoItems,
                paginatedEntities.TotalCount,
                paginatedEntities.PageNumber,
                paginatedEntities.PageSize
            );
        }

        public async Task<EmergencycontactDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<EmergencycontactDto>(entity);
        }

        public async Task<EmergencycontactDto> CreateAsync(EmergencycontactCreate request, int userId)
        {
            var entity = _mapper.Map<Emergencycontact>(request);
            entity.Userid = userId; // Set từ JWT token
            var createdEntity = await _repo.CreateAsync(entity);
            return _mapper.Map<EmergencycontactDto>(createdEntity);
        }

        public async Task<bool> UpdateAsync(int id, EmergencycontactUpdate request)
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
