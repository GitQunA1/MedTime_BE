using NpgsqlTypes;

namespace MedTime.Models.Enums
{
    public enum CallStatusEnum
    {
        [PgName("CONNECTED")]
        CONNECTED,
        [PgName("MISSED")]
        MISSED,
        [PgName("FAILED")]
        FAILED,
        [PgName("CANCELLED")]
        CANCELLED
    }
}
