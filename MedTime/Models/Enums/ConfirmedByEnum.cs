using NpgsqlTypes;

namespace MedTime.Models.Enums
{
    public enum ConfirmedByEnum
    {
        [PgName("USER")]
        USER,
        [PgName("GUARDIAN")]
        GUARDIAN
    }
}
