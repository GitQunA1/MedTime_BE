namespace MedTime.Models.Enums
{
    public enum PaymentStatusEnum
    {
        PENDING = 0,      // Đang chờ thanh toán
        PAID = 1,         // Đã thanh toán
        CANCELLED = 2,    // Đã hủy
        FAILED = 3        // Thanh toán thất bại
    }
}
