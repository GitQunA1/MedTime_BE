using NpgsqlTypes;

namespace MedTime.Models.Enums
{
    public enum DeviceTypeEnum
    {
        [PgName("ANDROID")]
        ANDROID,
        [PgName("IOS")]
        IOS,
        [PgName("WEB")]
        WEB
    }
}
