using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    /// <summary>
    /// Request để lấy báo cáo tuân thủ uống thuốc
    /// </summary>
    public class AdherenceReportRequest
    {
        public int? UserId { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }

        public int? MedicineId { get; set; }
    }

    /// <summary>
    /// Request để lấy báo cáo bỏ uống thuốc
    /// </summary>
    public class MissedDosesRequest
    {
        public int? UserId { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Request để lấy báo cáo sử dụng thuốc
    /// </summary>
    public class MedicineUsageRequest
    {
        public int? UserId { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Request để lấy thống kê dashboard
    /// </summary>
    public class DashboardRequest
    {
        public int? UserId { get; set; }
    }

    /// <summary>
    /// Request để lấy xu hướng theo thời gian
    /// </summary>
    public class TrendReportRequest
    {
        public int? UserId { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        public string Period { get; set; } = "daily"; // "daily", "weekly", "monthly"
    }
}
