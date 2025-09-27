using NpgsqlTypes;

namespace MedTime.Models.Enums
{
    public enum UserRoleEnum
    {
        [PgName("USER")]
        USER,
        [PgName("ADMIN")]
        ADMIN
    }
}
