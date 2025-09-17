using System;
using System.Collections.Generic;

namespace MedTime.Models.Entities;

public partial class Intakelog
{
    public int Logid { get; set; }

    public int Prescriptionid { get; set; }

    public int Userid { get; set; }

    public int? Scheduleid { get; set; }

    public DateTime Remindertime { get; set; }

    public DateTime? Actiontime { get; set; }

    public string? Notes { get; set; }

    public virtual Prescription Prescription { get; set; } = null!;

    public virtual Prescriptionschedule? Schedule { get; set; }

    public virtual User User { get; set; } = null!;
}
