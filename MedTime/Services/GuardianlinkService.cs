using AutoMapper;
using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Enums;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Repositories;

namespace MedTime.Services
{
    public class GuardianlinkService
    {
        private readonly GuardianlinkRepo _repo;
        private readonly UserRepo _userRepo;
        private readonly IMapper _mapper;

        public GuardianlinkService(GuardianlinkRepo repo, UserRepo userRepo, IMapper mapper)
        {
            _repo = repo;
            _userRepo = userRepo;
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

        /// <summary>
        /// Tạo guardian link mới với validation:
        /// 1. Patient không được là ADMIN
        /// 2. Uniquecode phải tồn tại và thuộc USER role
        /// 3. Guardian không thể tự theo dõi chính mình
        /// </summary>
        public async Task<GuardianlinkDto> CreateAsync(int guardianId, GuardianlinkCreate request)
        {
            // 1. Tìm Patient bằng Uniquecode
            var allUsers = await _userRepo.GetAllAsync();
            var patient = allUsers.FirstOrDefault(u => u.Uniquecode == request.Uniquecode);

            if (patient == null)
            {
                throw new InvalidOperationException($"User with Uniquecode '{request.Uniquecode}' not found.");
            }

            // 2. Check Patient không được là ADMIN
            if (patient.Role == UserRoleEnum.ADMIN)
            {
                throw new InvalidOperationException("Cannot add guardian link for ADMIN users.");
            }

            // 3. Check Guardian không tự theo dõi chính mình
            if (patient.Userid == guardianId)
            {
                throw new InvalidOperationException("You cannot add yourself as a guardian link.");
            }

            // 4. Check link đã tồn tại chưa
            var existingLink = await _repo.GetByIdAsync(guardianId, patient.Userid);
            if (existingLink != null)
            {
                throw new InvalidOperationException("Guardian link already exists.");
            }

            // 5. Tạo guardian link
            var entity = new Guardianlink
            {
                Guardianid = guardianId,
                Patientid = patient.Userid,
                Createdat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            await _repo.CreateAsync(entity);

            // 6. Load lại entity với navigation properties để map đầy đủ thông tin
            var createdEntity = await _repo.GetByIdAsync(guardianId, patient.Userid);
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
