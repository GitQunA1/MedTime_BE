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

    public virtual DbSet<Devicetoken> Devicetokens { get; set; }

    public virtual DbSet<Notificationhistory> Notificationhistories { get; set; }

    public virtual DbSet<Premiumplan> Premiumplans { get; set; }

    public virtual DbSet<Paymenthistory> Paymenthistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<CallStatusEnum>("call_status")
            .HasPostgresEnum<ConfirmedByEnum>("confirmed_by") 
            .HasPostgresEnum<DayOfWeekEnum>("day_of_week")
            .HasPostgresEnum<DeviceTypeEnum>("device_type")
            .HasPostgresEnum<IntakeActionEnum>("intake_action")
            .HasPostgresEnum<MedicineTypeEnum>("medicine_type")
            .HasPostgresEnum<MedicineUnitEnum>("medicine_unit")
            .HasPostgresEnum<NotificationStatusEnum>("notification_status")
            .HasPostgresEnum<PaymentStatusEnum>("payment_status")
            .HasPostgresEnum<PremiumPlanTypeEnum>("premium_plan_type")
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
            entity.HasIndex(e => e.UserName, "User_username_key").IsUnique();
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
            entity.Property(e => e.UserName)
                .HasMaxLength(255)
                .HasColumnName("username");
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

        modelBuilder.Entity<Devicetoken>(entity =>
        {
            entity.HasKey(e => e.Tokenid).HasName("devicetoken_pkey");

            entity.ToTable("devicetoken");

            entity.HasIndex(e => e.Userid, "idx_devicetoken_user");

            entity.Property(e => e.Tokenid).HasColumnName("tokenid");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .HasColumnName("token");
            entity.Property(e => e.DeviceType)
                .HasColumnName("devicetype")
                .HasColumnType("device_type");
            entity.Property(e => e.DeviceName)
                .HasMaxLength(100)
                .HasColumnName("devicename");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");

            entity.HasOne(d => d.User).WithMany(p => p.Devicetokens)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("devicetoken_userid_fkey");
        });

        modelBuilder.Entity<Notificationhistory>(entity =>
        {
            entity.HasKey(e => e.Notificationid).HasName("notificationhistory_pkey");

            entity.ToTable("notificationhistory");

            entity.HasIndex(e => e.Userid, "idx_notificationhistory_user");
            entity.HasIndex(e => e.SentAt, "idx_notificationhistory_sentat");

            entity.Property(e => e.Notificationid).HasColumnName("notificationid");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Prescriptionid).HasColumnName("prescriptionid");
            entity.Property(e => e.Scheduleid).HasColumnName("scheduleid");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Message)
                .HasColumnName("message");
            entity.Property(e => e.SentAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sentat");
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasColumnType("notification_status")
                .HasDefaultValue(NotificationStatusEnum.PENDING);
            entity.Property(e => e.ErrorMessage)
                .HasColumnName("errormessage");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("isread");
            entity.Property(e => e.ReadAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("readat");

            entity.HasOne(d => d.User).WithMany(p => p.Notificationhistories)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("notificationhistory_userid_fkey");

            entity.HasOne(d => d.Prescription).WithMany(p => p.Notificationhistories)
                .HasForeignKey(d => d.Prescriptionid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notificationhistory_prescriptionid_fkey");

            entity.HasOne(d => d.Schedule).WithMany(p => p.Notificationhistories)
                .HasForeignKey(d => d.Scheduleid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notificationhistory_scheduleid_fkey");
        });

        modelBuilder.Entity<Premiumplan>(entity =>
        {
            entity.HasKey(e => e.Planid).HasName("premiumplan_pkey");

            entity.ToTable("premiumplan");

            entity.HasIndex(e => e.Isactive, "idx_premiumplan_isactive");
            entity.HasIndex(e => e.Plantype, "idx_premiumplan_plantype");

            entity.Property(e => e.Planid).HasColumnName("planid");
            entity.Property(e => e.Plantype)
                .HasColumnName("plantype")
                .HasColumnType("premium_plan_type");
            entity.Property(e => e.Planname)
                .HasMaxLength(100)
                .HasColumnName("planname");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Durationdays).HasColumnName("durationdays");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Discountpercent)
                .HasPrecision(5, 2)
                .HasColumnName("discountpercent");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
        });

        modelBuilder.Entity<Paymenthistory>(entity =>
        {
            entity.HasKey(e => e.Paymentid).HasName("paymenthistory_pkey");

            entity.ToTable("paymenthistory");

            entity.HasIndex(e => e.Userid, "idx_paymenthistory_userid");
            entity.HasIndex(e => e.Orderid, "idx_paymenthistory_orderid").IsUnique();
            entity.HasIndex(e => e.Status, "idx_paymenthistory_status");
            entity.HasIndex(e => e.Paidat, "idx_paymenthistory_paidat");

            entity.Property(e => e.Paymentid).HasColumnName("paymentid");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Planid).HasColumnName("planid");
            entity.Property(e => e.Orderid)
                .HasMaxLength(50)
                .HasColumnName("orderid");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasColumnType("payment_status")
                .HasDefaultValue(PaymentStatusEnum.PENDING);
            entity.Property(e => e.Payosresponse).HasColumnName("payosresponse");
            entity.Property(e => e.Transactionid)
                .HasMaxLength(100)
                .HasColumnName("transactionid");
            entity.Property(e => e.Paidat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("paidat");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.User).WithMany(p => p.Paymenthistories)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_payment_user");

            entity.HasOne(d => d.Plan).WithMany(p => p.Paymenthistories)
                .HasForeignKey(d => d.Planid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_payment_plan");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
