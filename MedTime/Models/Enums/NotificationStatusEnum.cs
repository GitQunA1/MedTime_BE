using NpgsqlTypes;

namespace MedTime.Models.Enums
{
    public enum NotificationStatusEnum
    {
        [PgName("PENDING")]
        PENDING,
        [PgName("SENT")]
        SENT,
        [PgName("FAILED")]
        FAILED
    }
}
