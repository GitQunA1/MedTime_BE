using System;
using System.Collections.Generic;

namespace MedTime.Models.Entities;

public partial class Guardianlink
{
    public int Guardianid { get; set; }

    public int Patientid { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User Guardian { get; set; } = null!;

    public virtual User Patient { get; set; } = null!;
}
