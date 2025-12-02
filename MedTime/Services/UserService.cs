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
    public class UserService
    {
        private readonly UserRepo _repo;
        private readonly IMapper _mapper;

        public UserService(UserRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy danh sách users có phân trang
        /// </summary>
        public async Task<PaginatedResult<UserDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var query = _repo.GetAllQuery();
            var paginatedEntities = await query.ToPaginatedListAsync(pageNumber, pageSize);
            var dtoItems = _mapper.Map<List<UserDto>>(paginatedEntities.Items);

            return new PaginatedResult<UserDto>(
                dtoItems,
                paginatedEntities.TotalCount,
                paginatedEntities.PageNumber,
                paginatedEntities.PageSize
            );
        }

        /// <summary>
        /// Lấy thông tin user theo ID
        /// </summary>
        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<UserDto>(entity);
        }

        /// <summary>
        /// Cập nhật thông tin user
        /// </summary>
        public async Task<bool> UpdateAsync(int id, UserUpdate request)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            // Dùng AutoMapper để map các field không null
            _mapper.Map(request, existing);

            await _repo.UpdateAsync(id, existing);
            return true;
        }

        /// <summary>
        /// Xóa user
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.Delete(id);
        }

        /// <summary>
        /// Cập nhật role của user (USER → ADMIN)
        /// </summary>
        public async Task<bool> UpdateRoleAsync(int id, UserRoleEnum newRole)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Role = newRole;
            await _repo.UpdateAsync(id, existing);
            return true;
        }

        /// <summary>
        /// Cập nhật trạng thái Premium
        /// </summary>
        public async Task<bool> UpdatePremiumAsync(int id, UpdatePremiumRequest request)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Ispremium = request.IsPremium;

            if (request.IsPremium)
            {
                // Nếu set Premium = true, cần có ngày bắt đầu và kết thúc
                if (request.PremiumStart.HasValue)
                {
                    existing.Premiumstart = DateTime.SpecifyKind(request.PremiumStart.Value, DateTimeKind.Unspecified);
                }
                else
                {
                    // Mặc định bắt đầu từ hôm nay
                    existing.Premiumstart = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                }

                if (request.PremiumEnd.HasValue)
                {
                    existing.Premiumend = DateTime.SpecifyKind(request.PremiumEnd.Value, DateTimeKind.Unspecified);
                }
                else
                {
                    // Mặc định 1 tháng
                    existing.Premiumend = DateTime.SpecifyKind(DateTime.Now.AddMonths(1), DateTimeKind.Unspecified);
                }
            }
            else
            {
                // Nếu hủy Premium
                existing.Premiumstart = null;
                existing.Premiumend = null;
            }

            await _repo.UpdateAsync(id, existing);
            return true;
        }
    }
}
