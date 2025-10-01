using AutoMapper;
using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Repositories;

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

        public async Task<PaginatedResult<PrescriptionscheduleDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var query = _repo.GetAllQuery();
            var paginatedEntities = await query.ToPaginatedListAsync(pageNumber, pageSize);
            var dtoItems = _mapper.Map<List<PrescriptionscheduleDto>>(paginatedEntities.Items);

            return new PaginatedResult<PrescriptionscheduleDto>(
                dtoItems,
                paginatedEntities.TotalCount,
                paginatedEntities.PageNumber,
                paginatedEntities.PageSize
            );
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
