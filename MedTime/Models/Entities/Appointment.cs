using System;
using System.Collections.Generic;

namespace MedTime.Models.Entities;

public partial class Appointment
{
    public int Appointmentid { get; set; }

    public int Userid { get; set; }

    public string? Doctorname { get; set; }

    public string? Hospitalname { get; set; }

    public DateTime? Appointmentdate { get; set; }

    public string? Notes { get; set; }

    public virtual User User { get; set; } = null!;
}
