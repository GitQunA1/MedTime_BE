using NpgsqlTypes;

namespace MedTime.Models.Enums
{
    public enum MedicineTypeEnum
    {
        [PgName("TABLET")]
        TABLET,
        [PgName("CAPSULE")]
        CAPSULE,
        [PgName("SYRUP")]
        SYRUP,
        [PgName("SOLUTION")]
        SOLUTION,
        [PgName("SUSPENSION")]
        SUSPENSION,
        [PgName("POWDER")]
        POWDER,
        [PgName("SACHET")]
        SACHET,
        [PgName("INJECTION")]
        INJECTION,
        [PgName("AMPULE")]
        AMPULE,
        [PgName("VIAL")]
        VIAL,
        [PgName("EYE_DROPS")]
        EYE_DROPS,
        [PgName("EAR_DROPS")]
        EAR_DROPS,
        [PgName("NASAL_SPRAY")]
        NASAL_SPRAY,
        [PgName("INHALER")]
        INHALER,
        [PgName("OINTMENT")]
        OINTMENT,
        [PgName("CREAM")]
        CREAM,
        [PgName("GEL")]
        GEL,
        [PgName("PATCH")]
        PATCH,
        [PgName("SUPPOSITORY")]
        SUPPOSITORY,
        [PgName("OTHER")]
        OTHER
    }
}
