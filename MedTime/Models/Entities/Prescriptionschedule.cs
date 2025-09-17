using System;
using System.Collections.Generic;

namespace MedTime.Models.Entities;

public partial class Prescriptionschedule
{
    public int Scheduleid { get; set; }

    public int Prescriptionid { get; set; }

    public TimeOnly Timeofday { get; set; }

    public int? Interval { get; set; }

    public int? Dayofmonth { get; set; }

    public bool? Notificationenabled { get; set; }

    public string? Customringtone { get; set; }

    public virtual ICollection<Calllog> Calllogs { get; set; } = new List<Calllog>();

    public virtual ICollection<Intakelog> Intakelogs { get; set; } = new List<Intakelog>();

    public virtual Prescription Prescription { get; set; } = null!;
}
