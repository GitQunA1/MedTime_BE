using Microsoft.EntityFrameworkCore;
using MedTime.Models.Requests;
using MedTime.Models.Responses;

namespace MedTime.Helpers
{
    public static class PaginationExtensions
    {
        /// <summary>
        /// Áp dụng pagination cho IQueryable
        /// </summary>
        public static async Task<PaginatedResult<T>> ToPaginatedListAsync<T>(
            this IQueryable<T> query,
            PaginationRequest pagination)
        {
            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return new PaginatedResult<T>(items, totalCount, pagination.PageNumber, pagination.PageSize);
        }

        /// <summary>
        /// Áp dụng pagination cho IQueryable với pageNumber và pageSize trực tiếp
        /// </summary>
        public static async Task<PaginatedResult<T>> ToPaginatedListAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize)
        {
            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<T>(items, totalCount, pageNumber, pageSize);
        }
    }
}
