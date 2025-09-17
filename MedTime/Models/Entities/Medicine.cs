using System;
using System.Collections.Generic;

namespace MedTime.Models.Entities;

public partial class Medicine
{
    public int Medicineid { get; set; }

    public string Name { get; set; } = null!;

    public decimal? Strengthvalue { get; set; }

    public string? Imageurl { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
