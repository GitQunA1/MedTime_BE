using MedTime.Data;
using MedTime.Models.Entities;
using MedTime.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedTime.Repositories
{
    public class ReportRepo
    {
        private readonly MedTimeDBContext _context;

        public ReportRepo(MedTimeDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả intake logs theo filters
        /// </summary>
        public IQueryable<Intakelog> GetIntakeLogsQuery(int? userId, DateTime? startDate, DateTime? endDate, int? medicineId)
        {
            var query = _context.Intakelogs
                .Include(i => i.Prescription)
                    .ThenInclude(p => p.Medicine)
                .Include(i => i.Schedule)
                .AsNoTracking();

            if (userId.HasValue)
            {
                query = query.Where(i => i.Userid == userId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(i => i.Remindertime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(i => i.Remindertime <= endDate.Value);
            }

            if (medicineId.HasValue)
            {
                query = query.Where(i => i.Prescription.Medicineid == medicineId.Value);
            }

            return query;
        }

        /// <summary>
        /// Đếm intake logs theo action
        /// </summary>
        public async Task<Dictionary<IntakeActionEnum?, int>> CountByActionAsync(IQueryable<Intakelog> query)
        {
            var result = await query
                .GroupBy(i => i.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .ToListAsync();
            
            return result.ToDictionary(x => x.Action, x => x.Count);
        }

        /// <summary>
        /// Lấy thống kê theo thuốc
        /// </summary>
        public async Task<List<MedicineStatistic>> GetMedicineStatisticsAsync(IQueryable<Intakelog> query)
        {
            return await query
                .GroupBy(i => new { i.Prescription.Medicineid, i.Prescription.Medicine.Name })
                .Select(g => new MedicineStatistic
                {
                    MedicineId = g.Key.Medicineid,
                    MedicineName = g.Key.Name,
                    TotalScheduled = g.Count(),
                    Taken = g.Count(i => i.Action == IntakeActionEnum.TAKEN),
                    Missed = g.Count(i => i.Action == IntakeActionEnum.SKIPPED || i.Action == IntakeActionEnum.NO_RESPONSE),
                    Postponed = g.Count(i => i.Action == IntakeActionEnum.POSTPONED)
                })
                .ToListAsync();
        }

        /// <summary>
        /// Lấy thống kê theo thời gian trong ngày
        /// </summary>
        public async Task<List<TimeOfDayStatistic>> GetTimeOfDayStatisticsAsync(IQueryable<Intakelog> query)
        {
            var logs = await query.ToListAsync();

            var grouped = logs
                .Where(i => i.Schedule != null)
                .GroupBy(i => GetTimeOfDayPeriod(i.Schedule!.Timeofday))
                .Select(g => new TimeOfDayStatistic
                {
                    Period = g.Key,
                    TotalScheduled = g.Count(),
                    Taken = g.Count(i => i.Action == IntakeActionEnum.TAKEN)
                })
                .ToList();

            return grouped;
        }

        /// <summary>
        /// Lấy active prescriptions
        /// </summary>
        public async Task<int> GetActivePrescriptionsCountAsync(int userId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            return await _context.Prescriptions
                .Where(p => p.Userid == userId 
                    && (p.Enddate == null || p.Enddate >= today)
                    && (p.Startdate == null || p.Startdate <= today))
                .CountAsync();
        }

        /// <summary>
        /// Lấy tổng số prescriptions
        /// </summary>
        public async Task<int> GetTotalPrescriptionsCountAsync(int userId)
        {
            return await _context.Prescriptions
                .Where(p => p.Userid == userId)
                .CountAsync();
        }

        /// <summary>
        /// Lấy tổng số medicines đang dùng
        /// </summary>
        public async Task<int> GetTotalMedicinesCountAsync(int userId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            return await _context.Prescriptions
                .Where(p => p.Userid == userId 
                    && (p.Enddate == null || p.Enddate >= today)
                    && (p.Startdate == null || p.Startdate <= today))
                .Select(p => p.Medicineid)
                .Distinct()
                .CountAsync();
        }

        /// <summary>
        /// Lấy intake logs hôm nay
        /// </summary>
        public async Task<List<Intakelog>> GetTodayIntakeLogsAsync(int userId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _context.Intakelogs
                .Include(i => i.Prescription)
                    .ThenInclude(p => p.Medicine)
                .Where(i => i.Userid == userId 
                    && i.Remindertime >= today 
                    && i.Remindertime < tomorrow)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Lấy intake log gần nhất đã taken
        /// </summary>
        public async Task<Intakelog?> GetLastTakenIntakeAsync(int userId)
        {
            return await _context.Intakelogs
                .Include(i => i.Prescription)
                    .ThenInclude(p => p.Medicine)
                .Where(i => i.Userid == userId && i.Action == IntakeActionEnum.TAKEN)
                .OrderByDescending(i => i.Actiontime)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy thống kê theo loại thuốc
        /// </summary>
        public async Task<Dictionary<string, int>> GetUsageByMedicineTypeAsync(int? userId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Intakelogs
                .Include(i => i.Prescription)
                    .ThenInclude(p => p.Medicine)
                .Where(i => i.Action == IntakeActionEnum.TAKEN)
                .AsNoTracking();

            if (userId.HasValue)
            {
                query = query.Where(i => i.Userid == userId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(i => i.Remindertime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(i => i.Remindertime <= endDate.Value);
            }

            return await query
                .GroupBy(i => i.Prescription.Medicine.Type)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Type ?? "UNKNOWN", x => x.Count);
        }

        /// <summary>
        /// Lấy chi tiết sử dụng từng loại thuốc
        /// </summary>
        public async Task<List<MedicineUsageDetail>> GetMedicineUsageDetailsAsync(int? userId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Intakelogs
                .Include(i => i.Prescription)
                    .ThenInclude(p => p.Medicine)
                .AsNoTracking();

            if (userId.HasValue)
            {
                query = query.Where(i => i.Userid == userId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(i => i.Remindertime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(i => i.Remindertime <= endDate.Value);
            }

            return await query
                .GroupBy(i => new { i.Prescription.Medicineid, i.Prescription.Medicine.Name, i.Prescription.Medicine.Type })
                .Select(g => new MedicineUsageDetail
                {
                    MedicineId = g.Key.Medicineid,
                    MedicineName = g.Key.Name,
                    MedicineType = g.Key.Type.ToString(),
                    TotalIntakes = g.Count(),
                    TakenCount = g.Count(i => i.Action == IntakeActionEnum.TAKEN),
                    MissedCount = g.Count(i => i.Action == IntakeActionEnum.SKIPPED || i.Action == IntakeActionEnum.NO_RESPONSE),
                    LastTakenDate = g.Where(i => i.Action == IntakeActionEnum.TAKEN)
                                     .Max(i => (DateTime?)i.Actiontime)
                })
                .ToListAsync();
        }

        /// <summary>
        /// Helper: Xác định period trong ngày (morning, afternoon, evening)
        /// </summary>
        private string GetTimeOfDayPeriod(TimeOnly time)
        {
            var hour = time.Hour;
            if (hour >= 5 && hour < 12)
                return "morning";
            else if (hour >= 12 && hour < 18)
                return "afternoon";
            else
                return "evening";
        }
    }

    // Helper classes for query results
    public class MedicineStatistic
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = null!;
        public int TotalScheduled { get; set; }
        public int Taken { get; set; }
        public int Missed { get; set; }
        public int Postponed { get; set; }
    }

    public class TimeOfDayStatistic
    {
        public string Period { get; set; } = null!;
        public int TotalScheduled { get; set; }
        public int Taken { get; set; }
    }

    public class MedicineUsageDetail
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = null!;
        public string? MedicineType { get; set; }
        public int TotalIntakes { get; set; }
        public int TakenCount { get; set; }
        public int MissedCount { get; set; }
        public DateTime? LastTakenDate { get; set; }
    }
}
