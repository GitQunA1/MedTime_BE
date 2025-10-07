using System;
using System.Collections.Generic;

namespace MedTime.Models.Entities;

public partial class Prescription
{
    public int Prescriptionid { get; set; }

    public int Userid { get; set; }

    public int Medicineid { get; set; }

    public string? Dosage { get; set; }

    public int? Frequencyperday { get; set; }

    public DateOnly? Startdate { get; set; }

    public DateOnly? Enddate { get; set; }

    public int? Remainingquantity { get; set; }

    public string? Doctorname { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<Intakelog> Intakelogs { get; set; } = new List<Intakelog>();

    public virtual Medicine Medicine { get; set; } = null!;

    public virtual ICollection<Prescriptionschedule> Prescriptionschedules { get; set; } = new List<Prescriptionschedule>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Notificationhistory> Notificationhistories { get; set; } = new List<Notificationhistory>();
}
