namespace MedTime.Models.DTOs
{
    /// <summary>
    /// DTO cho báo cáo tỷ lệ tuân thủ uống thuốc
    /// </summary>
    public class AdherenceReportDto
    {
        public double AdherenceRate { get; set; }
        public int TotalScheduled { get; set; }
        public int Taken { get; set; }
        public int Missed { get; set; }
        public int Postponed { get; set; }
        public int NoResponse { get; set; }
        public List<MedicineAdherenceDto>? ByMedicine { get; set; }
        public TimeOfDayAdherenceDto? ByTimeOfDay { get; set; }
    }

    public class MedicineAdherenceDto
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = null!;
        public double AdherenceRate { get; set; }
        public int TotalScheduled { get; set; }
        public int Taken { get; set; }
        public int Missed { get; set; }
    }

    public class TimeOfDayAdherenceDto
    {
        public double Morning { get; set; }  // 5:00 - 11:59
        public double Afternoon { get; set; } // 12:00 - 17:59
        public double Evening { get; set; }  // 18:00 - 4:59
    }

    /// <summary>
    /// DTO cho thống kê số lần bỏ uống thuốc
    /// </summary>
    public class MissedDosesReportDto
    {
        public int TotalMissedDoses { get; set; }
        public List<MissedDoseDetailDto> Details { get; set; } = new();
    }

    public class MissedDoseDetailDto
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = null!;
        public int MissedCount { get; set; }
        public DateTime? LastMissedDate { get; set; }
    }

    /// <summary>
    /// DTO cho thống kê sử dụng thuốc theo loại
    /// </summary>
    public class MedicineUsageReportDto
    {
        public int TotalMedicines { get; set; }
        public List<MedicineUsageDetailDto> UsageByMedicine { get; set; } = new();
        public Dictionary<string, int>? UsageByType { get; set; }
    }

    public class MedicineUsageDetailDto
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = null!;
        public string? MedicineType { get; set; }
        public int TotalIntakes { get; set; }
        public int TakenCount { get; set; }
        public int MissedCount { get; set; }
        public DateTime? LastTakenDate { get; set; }
    }

    /// <summary>
    /// DTO cho dashboard tổng quan
    /// </summary>
    public class DashboardStatisticsDto
    {
        public int TotalPrescriptions { get; set; }
        public int ActivePrescriptions { get; set; }
        public int TotalMedicines { get; set; }
        public double OverallAdherenceRate { get; set; }
        public int TotalIntakesToday { get; set; }
        public int CompletedIntakesToday { get; set; }
        public int MissedIntakesToday { get; set; }
        public int UpcomingIntakesToday { get; set; }
        public RecentActivityDto? RecentActivity { get; set; }
    }

    public class RecentActivityDto
    {
        public DateTime? LastIntakeTime { get; set; }
        public string? LastMedicineTaken { get; set; }
        public int ConsecutiveDaysCompliant { get; set; }
    }

    /// <summary>
    /// DTO cho xu hướng theo thời gian
    /// </summary>
    public class TrendReportDto
    {
        public string Period { get; set; } = null!; // "daily", "weekly", "monthly"
        public List<TrendDataPointDto> TrendData { get; set; } = new();
        public TrendSummaryDto? Summary { get; set; }
    }

    public class TrendDataPointDto
    {
        public DateTime Date { get; set; }
        public double AdherenceRate { get; set; }
        public int TotalScheduled { get; set; }
        public int Taken { get; set; }
        public int Missed { get; set; }
    }

    public class TrendSummaryDto
    {
        public double AverageAdherence { get; set; }
        public double HighestAdherence { get; set; }
        public double LowestAdherence { get; set; }
        public string? Trend { get; set; } // "improving", "declining", "stable"
    }
}
