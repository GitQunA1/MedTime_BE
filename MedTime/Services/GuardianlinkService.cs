using AutoMapper;
using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Enums;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Repositories;
using Microsoft.EntityFrameworkCore;

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
        /// Kiểm tra xem guardianId có phải là guardian của patientId không
        /// Dùng để validate quyền truy cập data của patient
        /// </summary>
        public async Task<bool> IsGuardianOfPatientAsync(int guardianId, int patientId)
        {
            var link = await _repo.GetByIdAsync(guardianId, patientId);
            return link != null;
        }

        /// <summary>
        /// Lấy danh sách tất cả patients mà guardian đang theo dõi
        /// </summary>
        public async Task<List<GuardianlinkDto>> GetPatientsByGuardianAsync(int guardianId)
        {
            var links = await _repo.GetAllQuery()
                .Where(g => g.Guardianid == guardianId)
                .ToListAsync();
            return _mapper.Map<List<GuardianlinkDto>>(links);
        }

        /// <summary>
        /// Lấy danh sách tất cả guardians đang theo dõi patient
        /// </summary>
        public async Task<List<GuardianlinkDto>> GetGuardiansByPatientAsync(int patientId)
        {
            var links = await _repo.GetAllQuery()
                .Where(g => g.Patientid == patientId)
                .ToListAsync();
            return _mapper.Map<List<GuardianlinkDto>>(links);
        }

        /// <summary>
        /// Lấy danh sách patient IDs mà guardian đang theo dõi
        /// Dùng để check quyền truy cập nhanh
        /// </summary>
        public async Task<List<int>> GetPatientIdsByGuardianAsync(int guardianId)
        {
            return await _repo.GetAllQuery()
                .Where(g => g.Guardianid == guardianId)
                .Select(g => g.Patientid)
                .ToListAsync();
        }

        /// <summary>
        /// Kiểm tra xem userId có quyền truy cập data của targetUserId không
        /// Trả về true nếu:
        /// - userId == targetUserId (chính mình)
        /// - userId là guardian của targetUserId
        /// </summary>
        public async Task<bool> CanAccessPatientDataAsync(int userId, int targetUserId)
        {
            // Chính mình
            if (userId == targetUserId)
                return true;

            // Là guardian của target
            return await IsGuardianOfPatientAsync(userId, targetUserId);
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

        /// <summary>
        /// Xóa guardian link với composite key
        /// </summary>
        public async Task<bool> DeleteAsync(int guardianId, int patientId)
        {
            var existing = await _repo.GetByIdAsync(guardianId, patientId);
            if (existing == null) return false;

            return await _repo.DeleteByCompositeKeyAsync(guardianId, patientId);
        }
    }
}
