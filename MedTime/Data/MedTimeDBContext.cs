using System;
using System.Collections.Generic;
using MedTime.Models.Entities;
using MedTime.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedTime.Data;

public partial class MedTimeDBContext : DbContext
{
    public MedTimeDBContext()
    {
    }

    public MedTimeDBContext(DbContextOptions<MedTimeDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Calllog> Calllogs { get; set; }

    public virtual DbSet<Emergencycontact> Emergencycontacts { get; set; }

    public virtual DbSet<Guardianlink> Guardianlinks { get; set; }

    public virtual DbSet<Intakelog> Intakelogs { get; set; }

    public virtual DbSet<Medicine> Medicines { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<Prescriptionschedule> Prescriptionschedules { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<CallStatusEnum>("call_status")
            .HasPostgresEnum<ConfirmedByEnum>("confirmed_by") 
            .HasPostgresEnum<DayOfWeekEnum>("day_of_week")
            .HasPostgresEnum<IntakeActionEnum>("intake_action")
            .HasPostgresEnum<MedicineTypeEnum>("medicine_type")
            .HasPostgresEnum<MedicineUnitEnum>("medicine_unit")
            .HasPostgresEnum<RepeatPatternEnum>("repeat_pattern")
            .HasPostgresEnum<UserRoleEnum>("user_role");

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Appointmentid).HasName("appointment_pkey");

            entity.ToTable("appointment");

            entity.Property(e => e.Appointmentid).HasColumnName("appointmentid");
            entity.Property(e => e.Appointmentdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("appointmentdate");
            entity.Property(e => e.Doctorname)
                .HasMaxLength(255)
                .HasColumnName("doctorname");
            entity.Property(e => e.Hospitalname)
                .HasMaxLength(255)
                .HasColumnName("hospitalname");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("appointment_userid_fkey");
        });

        modelBuilder.Entity<Calllog>(entity =>
        {
            entity.HasKey(e => e.Callid).HasName("calllog_pkey");

            entity.ToTable("calllog");

            entity.HasIndex(e => e.Userid, "idx_calllog_user");

            entity.Property(e => e.Callid).HasColumnName("callid");
            entity.Property(e => e.Calltime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("calltime");
            entity.Property(e => e.Scheduleid).HasColumnName("scheduleid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            // Map enum properties
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasColumnType("call_status");

            entity.HasOne(d => d.Schedule).WithMany(p => p.Calllogs)
                .HasForeignKey(d => d.Scheduleid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("calllog_scheduleid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Calllogs)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("calllog_userid_fkey");
        });

        modelBuilder.Entity<Emergencycontact>(entity =>
        {
            entity.HasKey(e => e.Contactid).HasName("emergencycontact_pkey");

            entity.ToTable("emergencycontact");

            entity.Property(e => e.Contactid).HasColumnName("contactid");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(20)
                .HasColumnName("phonenumber");
            entity.Property(e => e.Relation)
                .HasMaxLength(50)
                .HasColumnName("relation");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Emergencycontacts)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("emergencycontact_userid_fkey");
        });

        modelBuilder.Entity<Guardianlink>(entity =>
        {
            entity.HasKey(e => new { e.Guardianid, e.Patientid }).HasName("guardianlink_pkey");

            entity.ToTable("guardianlink");

            entity.Property(e => e.Guardianid).HasColumnName("guardianid");
            entity.Property(e => e.Patientid).HasColumnName("patientid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");

            entity.HasOne(d => d.Guardian).WithMany(p => p.GuardianlinkGuardians)
                .HasForeignKey(d => d.Guardianid)
                .HasConstraintName("guardianlink_guardianid_fkey");

            entity.HasOne(d => d.Patient).WithMany(p => p.GuardianlinkPatients)
                .HasForeignKey(d => d.Patientid)
                .HasConstraintName("guardianlink_patientid_fkey");
        });

        modelBuilder.Entity<Intakelog>(entity =>
        {
            entity.HasKey(e => e.Logid).HasName("intakelog_pkey");

            entity.ToTable("intakelog");

            entity.HasIndex(e => e.Prescriptionid, "idx_intakelog_prescription");

            entity.HasIndex(e => e.Userid, "idx_intakelog_user");

            entity.Property(e => e.Logid).HasColumnName("logid");
            entity.Property(e => e.Actiontime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("actiontime");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Prescriptionid).HasColumnName("prescriptionid");
            entity.Property(e => e.Remindertime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("remindertime");
            entity.Property(e => e.Scheduleid).HasColumnName("scheduleid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            // Map enum properties
            entity.Property(e => e.Action)
                .HasColumnName("action")
                .HasColumnType("intake_action");
            entity.Property(e => e.ConfirmedBy)
                .HasColumnName("confirmedby")
                .HasColumnType("confirmed_by");

            entity.HasOne(d => d.Prescription).WithMany(p => p.Intakelogs)
                .HasForeignKey(d => d.Prescriptionid)
                .HasConstraintName("intakelog_prescriptionid_fkey");

            entity.HasOne(d => d.Schedule).WithMany(p => p.Intakelogs)
                .HasForeignKey(d => d.Scheduleid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("intakelog_scheduleid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Intakelogs)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("intakelog_userid_fkey");
        });

        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.HasKey(e => e.Medicineid).HasName("medicine_pkey");

            entity.ToTable("medicine");

            entity.Property(e => e.Medicineid).HasColumnName("medicineid");
            entity.Property(e => e.Imageurl).HasColumnName("imageurl");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Strengthvalue)
                .HasPrecision(10, 2)
                .HasColumnName("strengthvalue");

            // Map enum properties
            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasColumnType("medicine_type");
            entity.Property(e => e.StrengthUnit)
                .HasColumnName("strengthunit")
                .HasColumnType("medicine_unit");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Prescriptionid).HasName("prescription_pkey");

            entity.ToTable("prescription");

            entity.HasIndex(e => e.Userid, "idx_prescription_user");

            entity.Property(e => e.Prescriptionid).HasColumnName("prescriptionid");
            entity.Property(e => e.Doctorname)
                .HasMaxLength(255)
                .HasColumnName("doctorname");
            entity.Property(e => e.Dosage)
                .HasMaxLength(50)
                .HasColumnName("dosage");
            entity.Property(e => e.Enddate).HasColumnName("enddate");
            entity.Property(e => e.Frequencyperday).HasColumnName("frequencyperday");
            entity.Property(e => e.Medicineid).HasColumnName("medicineid");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Remainingquantity).HasColumnName("remainingquantity");
            entity.Property(e => e.Startdate).HasColumnName("startdate");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Medicine).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.Medicineid)
                .HasConstraintName("prescription_medicineid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("prescription_userid_fkey");
        });

        modelBuilder.Entity<Prescriptionschedule>(entity =>
        {
            entity.HasKey(e => e.Scheduleid).HasName("prescriptionschedule_pkey");

            entity.ToTable("prescriptionschedule");

            entity.HasIndex(e => e.Prescriptionid, "idx_schedule_prescription");

            entity.Property(e => e.Scheduleid).HasColumnName("scheduleid");
            entity.Property(e => e.Customringtone)
                .HasMaxLength(255)
                .HasColumnName("customringtone");
            entity.Property(e => e.Dayofmonth).HasColumnName("dayofmonth");
            entity.Property(e => e.Interval).HasColumnName("interval");
            entity.Property(e => e.Notificationenabled)
                .HasDefaultValue(true)
                .HasColumnName("notificationenabled");
            entity.Property(e => e.Prescriptionid).HasColumnName("prescriptionid");
            entity.Property(e => e.Timeofday).HasColumnName("timeofday");

            // Map enum properties
            entity.Property(e => e.RepeatPattern)
                .HasColumnName("repeatpattern")
                .HasColumnType("repeat_pattern")
                .HasDefaultValue(RepeatPatternEnum.DAILY);
            entity.Property(e => e.DayOfWeek)
                .HasColumnName("dayofweek")
                .HasColumnType("day_of_week");

            entity.HasOne(d => d.Prescription).WithMany(p => p.Prescriptionschedules)
                .HasForeignKey(d => d.Prescriptionid)
                .HasConstraintName("prescriptionschedule_prescriptionid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("User_pkey");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "User_email_key").IsUnique();

            entity.HasIndex(e => e.Uniquecode, "User_uniquecode_key").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Dateofbirth).HasColumnName("dateofbirth");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("fullname");
            entity.Property(e => e.Gender)
                .HasMaxLength(20)
                .HasColumnName("gender");
            entity.Property(e => e.Ispremium)
                .HasDefaultValue(false)
                .HasColumnName("ispremium");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(255)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(20)
                .HasColumnName("phonenumber");
            entity.Property(e => e.Premiumend)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("premiumend");
            entity.Property(e => e.Premiumstart)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("premiumstart");
            entity.Property(e => e.Timezone)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Asia/Ho_Chi_Minh'::character varying")
                .HasColumnName("timezone");
            entity.Property(e => e.Uniquecode)
                .HasMaxLength(20)
                .HasColumnName("uniquecode");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");

            // Map enum property
            entity.Property(e => e.Role)
                .HasColumnName("role")
                .HasColumnType("user_role")
                .HasDefaultValue(UserRoleEnum.USER);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
