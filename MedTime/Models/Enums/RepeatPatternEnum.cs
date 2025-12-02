using NpgsqlTypes;

namespace MedTime.Models.Enums
{
    public enum RepeatPatternEnum
    {
        [PgName("DAILY")]
        DAILY,
        [PgName("EVERY_X_DAYS")]
        EVERY_X_DAYS,
        [PgName("WEEKLY")]
        WEEKLY,
        [PgName("MONTHLY")]
        MONTHLY
    }
}
