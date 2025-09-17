using System;
using System.Collections.Generic;

namespace MedTime.Models.Entities;

public partial class Calllog
{
    public int Callid { get; set; }

    public int Userid { get; set; }

    public int? Scheduleid { get; set; }

    public DateTime? Calltime { get; set; }

    public virtual Prescriptionschedule? Schedule { get; set; }

    public virtual User User { get; set; } = null!;
}
