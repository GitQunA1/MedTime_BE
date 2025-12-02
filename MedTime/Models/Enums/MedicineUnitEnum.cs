using NpgsqlTypes;

namespace MedTime.Models.Enums
{
    public enum MedicineUnitEnum
    {
        [PgName("MG")]
        MG,
        [PgName("G")]
        G,
        [PgName("MCG")]
        MCG,
        [PgName("IU")]
        IU,
        [PgName("UNIT")]
        UNIT,
        [PgName("ML")]
        ML,
        [PgName("L")]
        L,
        [PgName("DROPS")]
        DROPS,
        [PgName("TABLET")]
        TABLET,
        [PgName("CAPSULE")]
        CAPSULE,
        [PgName("PATCH")]
        PATCH,
        [PgName("SACHET")]
        SACHET,
        [PgName("AMPULE")]
        AMPULE,
        [PgName("VIAL")]
        VIAL,
        [PgName("MG_PER_ML")]
        MG_PER_ML,
        [PgName("MG_PER_5ML")]
        MG_PER_5ML,
        [PgName("IU_PER_ML")]
        IU_PER_ML,
        [PgName("PERCENT")]
        PERCENT,
        [PgName("OTHER")]
        OTHER
    }
}
