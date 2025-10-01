using AutoMapper;
using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Repositories;

namespace MedTime.Services
{
    public class CalllogService
    {
        private readonly CalllogRepo _repo;
        private readonly IMapper _mapper;

        public CalllogService(CalllogRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<CalllogDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var query = _repo.GetAllQuery();
            var paginatedEntities = await query.ToPaginatedListAsync(pageNumber, pageSize);
            var dtoItems = _mapper.Map<List<CalllogDto>>(paginatedEntities.Items);

            return new PaginatedResult<CalllogDto>(
                dtoItems,
                paginatedEntities.TotalCount,
                paginatedEntities.PageNumber,
                paginatedEntities.PageSize
            );
        }

        public async Task<CalllogDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<CalllogDto>(entity);
        }

        public async Task<CalllogDto> CreateAsync(CalllogCreate request, int userId)
        {
            var entity = _mapper.Map<Calllog>(request);
            entity.Userid = userId; // Set từ JWT token
            var createdEntity = await _repo.CreateAsync(entity);
            return _mapper.Map<CalllogDto>(createdEntity);
        }

        public async Task<bool> UpdateAsync(int id, CalllogUpdate request)
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
