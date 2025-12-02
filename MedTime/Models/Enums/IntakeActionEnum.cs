using NpgsqlTypes;

namespace MedTime.Models.Enums
{
    public enum IntakeActionEnum
    {
        [PgName("TAKEN")]
        TAKEN,
        [PgName("POSTPONED")]
        POSTPONED,
        [PgName("SKIPPED")]
        SKIPPED,
        [PgName("NO_RESPONSE")]
        NO_RESPONSE
    }
}
