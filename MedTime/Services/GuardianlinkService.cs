using AutoMapper;
using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Repositories;

namespace MedTime.Services
{
    public class GuardianlinkService
    {
        private readonly GuardianlinkRepo _repo;
        private readonly IMapper _mapper;

        public GuardianlinkService(GuardianlinkRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<GuardianlinkDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var query = _repo.GetAllQuery();
            var paginatedEntities = await query.ToPaginatedListAsync(pageNumber, pageSize);
            var dtoItems = _mapper.Map<List<GuardianlinkDto>>(paginatedEntities.Items);

            return new PaginatedResult<GuardianlinkDto>(
                dtoItems,
                paginatedEntities.TotalCount,
                paginatedEntities.PageNumber,
                paginatedEntities.PageSize
            );
        }

        public async Task<GuardianlinkDto?> GetByIdAsync(int guardianId, int patientId)
        {
            var entity = await _repo.GetByIdAsync(guardianId, patientId);
            if (entity == null) return null;
            return _mapper.Map<GuardianlinkDto>(entity);
        }

        public async Task<GuardianlinkDto> CreateAsync(GuardianlinkCreate request)
        {
            var entity = _mapper.Map<Guardianlink>(request);
            var createdEntity = await _repo.CreateAsync(entity);
            return _mapper.Map<GuardianlinkDto>(createdEntity);
        }

        public async Task<bool> UpdateAsync(int guardianId, int patientId, GuardianlinkUpdate request)
        {
            var existing = await _repo.GetByIdAsync(guardianId, patientId);
            if (existing == null) return false;

            _mapper.Map(request, existing);
            // Guardianlink composite key nên dùng guardianId
            await _repo.UpdateAsync(guardianId, existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int guardianId, int patientId)
        {
            // Guardianlink composite key nên dùng guardianId
            return await _repo.Delete(guardianId);
        }
    }
}
