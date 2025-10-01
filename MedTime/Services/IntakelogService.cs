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
        private readonly IMapper _mapper;

        public IntakelogService(IntakelogRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<IntakelogDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var query = _repo.GetAllQuery();
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
            entity.Userid = userId; // Set từ JWT token
            var createdEntity = await _repo.CreateAsync(entity);
            return _mapper.Map<IntakelogDto>(createdEntity);
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
