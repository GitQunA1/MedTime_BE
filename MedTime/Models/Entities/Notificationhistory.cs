using MedTime.Models.Enums;
using System;

namespace MedTime.Models.Entities;

/// <summary>
/// Lưu lịch sử notifications đã gửi
/// </summary>
public partial class Notificationhistory
{
    public int Notificationid { get; set; }

    public int Userid { get; set; }

    public int? Prescriptionid { get; set; }

    public int? Scheduleid { get; set; }

    /// <summary>
    /// Tiêu đề notification
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Nội dung notification
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Thời gian gửi notification
    /// </summary>
    public DateTime SentAt { get; set; }

    /// <summary>
    /// Trạng thái: SENT, FAILED, PENDING
    /// </summary>
    public NotificationStatusEnum Status { get; set; } = NotificationStatusEnum.PENDING;

    /// <summary>
    /// Lỗi khi gửi (nếu có)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// User đã đọc notification chưa
    /// </summary>
    public bool IsRead { get; set; } = false;

    public DateTime? ReadAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Prescription? Prescription { get; set; }
    public virtual Prescriptionschedule? Schedule { get; set; }
}
