using AutoMapper;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Requests;
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

        public async Task<List<AppointmentDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();

            var dtoList = _mapper.Map<List<AppointmentDto>>(list);
            return dtoList;
        }

        public async Task<AppointmentDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;
            var dto = _mapper.Map<AppointmentDto>(entity);
            return dto;
        }

        public async Task<AppointmentDto> CreateAsync(AppointmentCreate request)
        {
            var entity = _mapper.Map<Appointment>(request);

            var createdEntity = await _repo.CreateAsync(entity);
            var createdDto = _mapper.Map<AppointmentDto>(createdEntity);
            return createdDto;
        }

        public async Task<bool?> UpdateAsync(int id, AppointmentUpdate request)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            var entity = _mapper.Map<Appointment>(request);
            return await _repo.UpdateAsync(id, entity);
        }

        public async Task<bool?> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            return await _repo.Delete(id);
        }
    }
}
