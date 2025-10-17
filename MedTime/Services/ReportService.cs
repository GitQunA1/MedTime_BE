using MedTime.Models.DTOs;
using MedTime.Models.Enums;
using MedTime.Models.Requests;
using MedTime.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MedTime.Services
{
    public class ReportService
    {
        private readonly ReportRepo _reportRepo;

        public ReportService(ReportRepo reportRepo)
        {
            _reportRepo = reportRepo;
        }

        /// <summary>
        /// Lấy báo cáo tuân thủ uống thuốc
        /// </summary>
        public async Task<AdherenceReportDto> GetAdherenceReportAsync(AdherenceReportRequest request)
        {
            var query = _reportRepo.GetIntakeLogsQuery(
                request.UserId,
                request.StartDate,
                request.EndDate,
                request.MedicineId
            );

            var actionCounts = await _reportRepo.CountByActionAsync(query);
            var medicineStats = await _reportRepo.GetMedicineStatisticsAsync(query);
            var timeOfDayStats = await _reportRepo.GetTimeOfDayStatisticsAsync(query);

            int totalScheduled = actionCounts.Values.Sum();
            int taken = actionCounts.GetValueOrDefault(IntakeActionEnum.TAKEN, 0);
            int missed = actionCounts.GetValueOrDefault(IntakeActionEnum.SKIPPED, 0) 
                       + actionCounts.GetValueOrDefault(IntakeActionEnum.NO_RESPONSE, 0);
            int postponed = actionCounts.GetValueOrDefault(IntakeActionEnum.POSTPONED, 0);
            int noResponse = actionCounts.GetValueOrDefault(IntakeActionEnum.NO_RESPONSE, 0);

            double adherenceRate = totalScheduled > 0 
                ? Math.Round((double)taken / totalScheduled * 100, 2) 
                : 0;

            var byMedicine = medicineStats.Select(m => new MedicineAdherenceDto
            {
                MedicineId = m.MedicineId,
                MedicineName = m.MedicineName,
                TotalScheduled = m.TotalScheduled,
                Taken = m.Taken,
                Missed = m.Missed,
                AdherenceRate = m.TotalScheduled > 0 
                    ? Math.Round((double)m.Taken / m.TotalScheduled * 100, 2) 
                    : 0
            }).ToList();

            var byTimeOfDay = new TimeOfDayAdherenceDto
            {
                Morning = CalculateAdherenceRate(timeOfDayStats.FirstOrDefault(t => t.Period == "morning")),
                Afternoon = CalculateAdherenceRate(timeOfDayStats.FirstOrDefault(t => t.Period == "afternoon")),
                Evening = CalculateAdherenceRate(timeOfDayStats.FirstOrDefault(t => t.Period == "evening"))
            };

            return new AdherenceReportDto
            {
                AdherenceRate = adherenceRate,
                TotalScheduled = totalScheduled,
                Taken = taken,
                Missed = missed,
                Postponed = postponed,
                NoResponse = noResponse,
                ByMedicine = byMedicine,
                ByTimeOfDay = byTimeOfDay
            };
        }

        /// <summary>
        /// Lấy báo cáo số lần bỏ uống thuốc
        /// </summary>
        public async Task<MissedDosesReportDto> GetMissedDosesReportAsync(MissedDosesRequest request)
        {
            var query = _reportRepo.GetIntakeLogsQuery(
                request.UserId,
                request.StartDate,
                request.EndDate,
                null
            );

            // Filter only missed doses
            query = query.Where(i => i.Action == IntakeActionEnum.SKIPPED 
                                  || i.Action == IntakeActionEnum.NO_RESPONSE);

            var medicineStats = await _reportRepo.GetMedicineStatisticsAsync(query);

            var details = medicineStats.Select(m => new MissedDoseDetailDto
            {
                MedicineId = m.MedicineId,
                MedicineName = m.MedicineName,
                MissedCount = m.Missed,
                LastMissedDate = null // Will be populated if needed
            }).ToList();

            return new MissedDosesReportDto
            {
                TotalMissedDoses = details.Sum(d => d.MissedCount),
                Details = details
            };
        }

        /// <summary>
        /// Lấy báo cáo sử dụng thuốc theo loại
        /// </summary>
        public async Task<MedicineUsageReportDto> GetMedicineUsageReportAsync(MedicineUsageRequest request)
        {
            var usageDetails = await _reportRepo.GetMedicineUsageDetailsAsync(
                request.UserId,
                request.StartDate,
                request.EndDate
            );

            var usageByType = await _reportRepo.GetUsageByMedicineTypeAsync(
                request.UserId,
                request.StartDate,
                request.EndDate
            );

            var usageByMedicine = usageDetails.Select(d => new MedicineUsageDetailDto
            {
                MedicineId = d.MedicineId,
                MedicineName = d.MedicineName,
                MedicineType = d.MedicineType,
                TotalIntakes = d.TotalIntakes,
                TakenCount = d.TakenCount,
                MissedCount = d.MissedCount,
                LastTakenDate = d.LastTakenDate
            }).ToList();

            return new MedicineUsageReportDto
            {
                TotalMedicines = usageByMedicine.Count,
                UsageByMedicine = usageByMedicine,
                UsageByType = usageByType
            };
        }

        /// <summary>
        /// Lấy thống kê dashboard tổng quan
        /// </summary>
        public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync(int userId)
        {
            var totalPrescriptions = await _reportRepo.GetTotalPrescriptionsCountAsync(userId);
            var activePrescriptions = await _reportRepo.GetActivePrescriptionsCountAsync(userId);
            var totalMedicines = await _reportRepo.GetTotalMedicinesCountAsync(userId);

            // Overall adherence (last 30 days)
            var last30Days = DateTime.Now.AddDays(-30);
            var overallQuery = _reportRepo.GetIntakeLogsQuery(userId, last30Days, null, null);
            var overallActionCounts = await _reportRepo.CountByActionAsync(overallQuery);
            int overallTotal = overallActionCounts.Values.Sum();
            int overallTaken = overallActionCounts.GetValueOrDefault(IntakeActionEnum.TAKEN, 0);
            double overallAdherence = overallTotal > 0 
                ? Math.Round((double)overallTaken / overallTotal * 100, 2) 
                : 0;

            // Today's intake logs
            var todayLogs = await _reportRepo.GetTodayIntakeLogsAsync(userId);
            var now = DateTime.Now;
            
            int totalIntakesToday = todayLogs.Count;
            int completedToday = todayLogs.Count(i => i.Action == IntakeActionEnum.TAKEN);
            int missedToday = todayLogs.Count(i => i.Action == IntakeActionEnum.SKIPPED 
                                                 || i.Action == IntakeActionEnum.NO_RESPONSE);
            int upcomingToday = todayLogs.Count(i => i.Action == null && i.Remindertime > now);

            // Recent activity
            var lastIntake = await _reportRepo.GetLastTakenIntakeAsync(userId);
            var consecutiveDays = await CalculateConsecutiveDaysCompliantAsync(userId);

            var recentActivity = new RecentActivityDto
            {
                LastIntakeTime = lastIntake?.Actiontime,
                LastMedicineTaken = lastIntake?.Prescription?.Medicine?.Name,
                ConsecutiveDaysCompliant = consecutiveDays
            };

            return new DashboardStatisticsDto
            {
                TotalPrescriptions = totalPrescriptions,
                ActivePrescriptions = activePrescriptions,
                TotalMedicines = totalMedicines,
                OverallAdherenceRate = overallAdherence,
                TotalIntakesToday = totalIntakesToday,
                CompletedIntakesToday = completedToday,
                MissedIntakesToday = missedToday,
                UpcomingIntakesToday = upcomingToday,
                RecentActivity = recentActivity
            };
        }

        /// <summary>
        /// Lấy xu hướng theo thời gian
        /// </summary>
        public async Task<TrendReportDto> GetTrendReportAsync(TrendReportRequest request)
        {
            var query = _reportRepo.GetIntakeLogsQuery(
                request.UserId,
                request.StartDate,
                request.EndDate,
                null
            );

            var logs = await query.ToListAsync();

            var trendData = new List<TrendDataPointDto>();

            if (request.Period.ToLower() == "daily")
            {
                trendData = logs
                    .GroupBy(i => i.Remindertime.Date)
                    .Select(g => new TrendDataPointDto
                    {
                        Date = g.Key,
                        TotalScheduled = g.Count(),
                        Taken = g.Count(i => i.Action == IntakeActionEnum.TAKEN),
                        Missed = g.Count(i => i.Action == IntakeActionEnum.SKIPPED 
                                           || i.Action == IntakeActionEnum.NO_RESPONSE),
                        AdherenceRate = g.Count() > 0 
                            ? Math.Round((double)g.Count(i => i.Action == IntakeActionEnum.TAKEN) / g.Count() * 100, 2)
                            : 0
                    })
                    .OrderBy(t => t.Date)
                    .ToList();
            }
            else if (request.Period.ToLower() == "weekly")
            {
                trendData = logs
                    .GroupBy(i => GetWeekStart(i.Remindertime))
                    .Select(g => new TrendDataPointDto
                    {
                        Date = g.Key,
                        TotalScheduled = g.Count(),
                        Taken = g.Count(i => i.Action == IntakeActionEnum.TAKEN),
                        Missed = g.Count(i => i.Action == IntakeActionEnum.SKIPPED 
                                           || i.Action == IntakeActionEnum.NO_RESPONSE),
                        AdherenceRate = g.Count() > 0 
                            ? Math.Round((double)g.Count(i => i.Action == IntakeActionEnum.TAKEN) / g.Count() * 100, 2)
                            : 0
                    })
                    .OrderBy(t => t.Date)
                    .ToList();
            }
            else if (request.Period.ToLower() == "monthly")
            {
                trendData = logs
                    .GroupBy(i => new DateTime(i.Remindertime.Year, i.Remindertime.Month, 1))
                    .Select(g => new TrendDataPointDto
                    {
                        Date = g.Key,
                        TotalScheduled = g.Count(),
                        Taken = g.Count(i => i.Action == IntakeActionEnum.TAKEN),
                        Missed = g.Count(i => i.Action == IntakeActionEnum.SKIPPED 
                                           || i.Action == IntakeActionEnum.NO_RESPONSE),
                        AdherenceRate = g.Count() > 0 
                            ? Math.Round((double)g.Count(i => i.Action == IntakeActionEnum.TAKEN) / g.Count() * 100, 2)
                            : 0
                    })
                    .OrderBy(t => t.Date)
                    .ToList();
            }

            var summary = CalculateTrendSummary(trendData);

            return new TrendReportDto
            {
                Period = request.Period,
                TrendData = trendData,
                Summary = summary
            };
        }

        #region Helper Methods

        private double CalculateAdherenceRate(TimeOfDayStatistic? stat)
        {
            if (stat == null || stat.TotalScheduled == 0)
                return 0;

            return Math.Round((double)stat.Taken / stat.TotalScheduled * 100, 2);
        }

        private async Task<int> CalculateConsecutiveDaysCompliantAsync(int userId)
        {
            var today = DateTime.Today;
            var consecutiveDays = 0;

            for (int i = 0; i < 365; i++) // Max check 1 year
            {
                var checkDate = today.AddDays(-i);
                var nextDate = checkDate.AddDays(1);

                var dayQuery = _reportRepo.GetIntakeLogsQuery(userId, checkDate, nextDate, null);
                var dayActionCounts = await _reportRepo.CountByActionAsync(dayQuery);

                int dayTotal = dayActionCounts.Values.Sum();
                int dayTaken = dayActionCounts.GetValueOrDefault(IntakeActionEnum.TAKEN, 0);

                if (dayTotal == 0)
                    continue; // No schedules for this day

                // Consider compliant if adherence >= 80%
                double dayAdherence = (double)dayTaken / dayTotal;
                if (dayAdherence >= 0.8)
                {
                    consecutiveDays++;
                }
                else
                {
                    break; // Streak broken
                }
            }

            return consecutiveDays;
        }

        private DateTime GetWeekStart(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        private TrendSummaryDto CalculateTrendSummary(List<TrendDataPointDto> trendData)
        {
            if (trendData.Count == 0)
            {
                return new TrendSummaryDto
                {
                    AverageAdherence = 0,
                    HighestAdherence = 0,
                    LowestAdherence = 0,
                    Trend = "stable"
                };
            }

            var adherenceRates = trendData.Select(t => t.AdherenceRate).ToList();
            double avgAdherence = Math.Round(adherenceRates.Average(), 2);
            double highestAdherence = adherenceRates.Max();
            double lowestAdherence = adherenceRates.Min();

            // Determine trend direction
            string trend = "stable";
            if (trendData.Count >= 2)
            {
                var firstHalf = trendData.Take(trendData.Count / 2).Average(t => t.AdherenceRate);
                var secondHalf = trendData.Skip(trendData.Count / 2).Average(t => t.AdherenceRate);

                if (secondHalf > firstHalf + 5)
                    trend = "improving";
                else if (secondHalf < firstHalf - 5)
                    trend = "declining";
            }

            return new TrendSummaryDto
            {
                AverageAdherence = avgAdherence,
                HighestAdherence = highestAdherence,
                LowestAdherence = lowestAdherence,
                Trend = trend
            };
        }

        #endregion
    }
}
