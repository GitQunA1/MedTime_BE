using MedTime.Models.Enums;
using System;
using System.Collections.Generic;

namespace MedTime.Models.Entities;

public partial class User
{
    public int Userid { get; set; }

    public string Fullname { get; set; } = null!;

    public DateOnly? Dateofbirth { get; set; }

    public string? Gender { get; set; }

    public string? Phonenumber { get; set; }

    public string Email { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public UserRoleEnum Role { get; set; } = UserRoleEnum.USER;

    public string? Uniquecode { get; set; }

    public string? Timezone { get; set; }

    public bool? Ispremium { get; set; }

    public DateTime? Premiumstart { get; set; }

    public DateTime? Premiumend { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Calllog> Calllogs { get; set; } = new List<Calllog>();

    public virtual ICollection<Emergencycontact> Emergencycontacts { get; set; } = new List<Emergencycontact>();

    public virtual ICollection<Guardianlink> GuardianlinkGuardians { get; set; } = new List<Guardianlink>();

    public virtual ICollection<Guardianlink> GuardianlinkPatients { get; set; } = new List<Guardianlink>();

    public virtual ICollection<Intakelog> Intakelogs { get; set; } = new List<Intakelog>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
