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
        private readonly IMapper _mapper;

        public PrescriptionService(PrescriptionRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<PrescriptionDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var query = _repo.GetAllQuery();
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

        public async Task<PrescriptionDto> CreateAsync(PrescriptionCreate request, int userId)
        {
            var entity = _mapper.Map<Prescription>(request);
            entity.Userid = userId; // Set từ JWT token
            var createdEntity = await _repo.CreateAsync(entity);
            return _mapper.Map<PrescriptionDto>(createdEntity);
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
