using MedTime.Models.Enums;
using System;

namespace MedTime.Models.Entities;

/// <summary>
/// Lưu FCM device token của user để gửi push notification
/// </summary>
public partial class Devicetoken
{
    public int Tokenid { get; set; }

    public int Userid { get; set; }

    /// <summary>
    /// FCM Device Token từ Firebase
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// Loại thiết bị: ANDROID, IOS, WEB
    /// </summary>
    public DeviceTypeEnum? DeviceType { get; set; }

    /// <summary>
    /// Tên thiết bị (optional)
    /// </summary>
    public string? DeviceName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Token còn active không (user có thể logout → inactive)
    /// </summary>
    public bool IsActive { get; set; } = true;

    public virtual User User { get; set; } = null!;
}
