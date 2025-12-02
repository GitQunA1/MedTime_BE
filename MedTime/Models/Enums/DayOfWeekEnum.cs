using NpgsqlTypes;

namespace MedTime.Models.Enums
{
    public enum DayOfWeekEnum
    {
        [PgName("MON")]
        MON,
        [PgName("TUE")]
        TUE,
        [PgName("WED")]
        WED,
        [PgName("THU")]
        THU,
        [PgName("FRI")]
        FRI,
        [PgName("SAT")]
        SAT,
        [PgName("SUN")]
        SUN
    }
}
