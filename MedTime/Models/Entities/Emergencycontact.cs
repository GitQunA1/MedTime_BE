using System;
using System.Collections.Generic;

namespace MedTime.Models.Entities;

public partial class Emergencycontact
{
    public int Contactid { get; set; }

    public int Userid { get; set; }

    public string Name { get; set; } = null!;

    public string? Phonenumber { get; set; }

    public string? Relation { get; set; }

    public virtual User User { get; set; } = null!;
}
