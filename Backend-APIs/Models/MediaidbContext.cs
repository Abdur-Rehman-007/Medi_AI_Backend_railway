using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Backend_APIs.Models;

public partial class MediaidbContext : DbContext
{
    public MediaidbContext()
    {
    }

    public MediaidbContext(DbContextOptions<MediaidbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activemedicinereminder> Activemedicinereminders { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Auditlog> Auditlogs { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<Doctorleaf> Doctorleaves { get; set; }

    public virtual DbSet<Doctorperformancesummary> Doctorperformancesummaries { get; set; }

    public virtual DbSet<Doctorreview> Doctorreviews { get; set; }

    public virtual DbSet<Doctorschedule> Doctorschedules { get; set; }

    public virtual DbSet<Emailverificationotp> Emailverificationotps { get; set; }

    public virtual DbSet<Emergencycontact> Emergencycontacts { get; set; }

    public virtual DbSet<Medicalhistory> Medicalhistories { get; set; }

    public virtual DbSet<Medicinereminder> Medicinereminders { get; set; }

    public virtual DbSet<Medicinereminderlog> Medicinereminderlogs { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Passwordresettoken> Passwordresettokens { get; set; }
    public virtual DbSet<Refreshtoken> RefreshTokens { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<Prescriptionmedicine> Prescriptionmedicines { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Symptomcheck> Symptomchecks { get; set; }

    public virtual DbSet<Systemsetting> Systemsettings { get; set; }

    public virtual DbSet<Todaysappointment> Todaysappointments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseMySql("server=localhost;port=3306;database=mediaidb;user=<your-user>;password=<your-password>", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.44-mysql"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Activemedicinereminder>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("activemedicinereminders");

            entity.Property(e => e.Dosage).HasMaxLength(100);
            entity.Property(e => e.Frequency).HasColumnType("enum('Once','Twice','Thrice','Four times','Custom')");
            entity.Property(e => e.MedicineName).HasMaxLength(200);
            entity.Property(e => e.StudentEmail).HasMaxLength(100);
            entity.Property(e => e.StudentName).HasMaxLength(100);
            entity.Property(e => e.Times).HasColumnType("json");
        });

        modelBuilder.Entity<Doctorperformancesummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("doctorperformancesummary");

            entity.Property(e => e.DoctorName).HasMaxLength(100);
            entity.Property(e => e.Specialization).HasMaxLength(100);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("appointments");

            entity.HasIndex(e => e.CancelledBy, "CancelledBy");

            entity.HasIndex(e => new { e.DoctorId, e.AppointmentDate, e.Status }, "idx_appointment_doctor_date_status");

            entity.HasIndex(e => new { e.PatientId, e.AppointmentDate, e.Status }, "idx_appointment_patient_date_status");

            entity.HasIndex(e => e.AppointmentDate, "idx_date");

            entity.HasIndex(e => e.DoctorId, "idx_doctor");

            entity.HasIndex(e => e.PatientId, "idx_patient");

            entity.HasIndex(e => e.Status, "idx_status");

            entity.Property(e => e.AppointmentTime).HasColumnType("time");
            entity.Property(e => e.CancellationReason).HasColumnType("text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Duration).HasDefaultValueSql("'30'");
            entity.Property(e => e.Notes).HasColumnType("text");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Pending'")
                .HasColumnType("enum('Pending','Confirmed','InProgress','Completed','Cancelled','NoShow')");
            entity.Property(e => e.Symptoms).HasColumnType("text");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.CancelledByNavigation).WithMany(p => p.AppointmentCancelledByNavigations)
                .HasForeignKey(d => d.CancelledBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("appointments_ibfk_3");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("appointments_ibfk_2");

            entity.HasOne(d => d.Patient).WithMany(p => p.AppointmentPatients)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("appointments_ibfk_1");
        });

        modelBuilder.Entity<Auditlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("auditlogs");

            entity.HasIndex(e => e.CreatedAt, "idx_created");

            entity.HasIndex(e => new { e.EntityType, e.EntityId }, "idx_entity");

            entity.HasIndex(e => new { e.UserId, e.Action }, "idx_user_action");

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.NewValues).HasColumnType("json");
            entity.Property(e => e.OldValues).HasColumnType("json");
            entity.Property(e => e.UserAgent).HasColumnType("text");

            entity.HasOne(d => d.User).WithMany(p => p.Auditlogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("auditlogs_ibfk_1");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("doctors");

            entity.HasIndex(e => e.LicenseNumber, "LicenseNumber").IsUnique();

            entity.HasIndex(e => e.UserId, "UserId").IsUnique();

            entity.HasIndex(e => e.IsAvailable, "idx_available");

            entity.HasIndex(e => e.Specialization, "idx_specialization");

            entity.Property(e => e.AverageRating)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.Bio).HasColumnType("text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Experience).HasDefaultValueSql("'0'");
            entity.Property(e => e.IsAvailable).HasDefaultValueSql("'1'");
            entity.Property(e => e.LicenseNumber).HasMaxLength(50);
            entity.Property(e => e.Qualification).HasMaxLength(200);
            entity.Property(e => e.RoomNumber).HasMaxLength(20);
            entity.Property(e => e.Specialization).HasMaxLength(100);
            entity.Property(e => e.TotalRatings).HasDefaultValueSql("'0'");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.User).WithOne(p => p.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId)
                .HasConstraintName("doctors_ibfk_1");
        });

        modelBuilder.Entity<Doctorleaf>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("doctorleaves");

            entity.HasIndex(e => new { e.DoctorId, e.StartDate, e.EndDate }, "idx_doctor_dates");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Reason).HasMaxLength(200);

            entity.HasOne(d => d.Doctor).WithMany(p => p.Doctorleaves)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("doctorleaves_ibfk_1");
        });

        modelBuilder.Entity<Doctorschedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("doctorschedules");

            entity.HasIndex(e => new { e.DoctorId, e.DayOfWeek }, "idx_doctorschedules_day");

            entity.HasIndex(e => new { e.DoctorId, e.DayOfWeek }, "uq_doctorschedules_doctor_day")
                .IsUnique();

            entity.Property(e => e.DayOfWeek)
                .HasColumnType("enum('Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday')");
            entity.Property(e => e.StartTime).HasColumnType("time");
            entity.Property(e => e.EndTime).HasColumnType("time");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Doctorschedules)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("fk_doctorschedules_doctor");
        });

        modelBuilder.Entity<Emailverificationotp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("emailverificationotps");

            entity.HasIndex(e => new { e.UserId, e.Otp, e.ExpiresAt }, "idx_user_otp");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.ExpiresAt).HasColumnType("timestamp");
            entity.Property(e => e.IsUsed).HasDefaultValueSql("'0'");
            entity.Property(e => e.Otp)
                .HasMaxLength(6)
                .HasColumnName("OTP");

            entity.HasOne(d => d.User).WithMany(p => p.Emailverificationotps)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("emailverificationotps_ibfk_1");
        });

        modelBuilder.Entity<Emergencycontact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("emergencycontacts");

            entity.HasIndex(e => new { e.UserId, e.IsPrimary }, "idx_user_primary");

            entity.Property(e => e.Address).HasColumnType("text");
            entity.Property(e => e.ContactName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.IsPrimary).HasDefaultValueSql("'0'");
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Relationship).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.User).WithMany(p => p.Emergencycontacts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("emergencycontacts_ibfk_1");
        });

        modelBuilder.Entity<Medicalhistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("medicalhistory");

            entity.HasIndex(e => new { e.PatientId, e.RecordType }, "idx_patient_type");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Notes).HasColumnType("text");
            entity.Property(e => e.RecordType).HasColumnType("enum('Allergy','ChronicCondition','Surgery','Vaccination','FamilyHistory','Other')");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.Patient).WithMany(p => p.Medicalhistories)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("medicalhistory_ibfk_1");
        });

        modelBuilder.Entity<Medicinereminder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("medicinereminders");

            entity.HasIndex(e => new { e.StudentId, e.IsActive }, "idx_student_active");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.CustomFrequency).HasMaxLength(100);
            entity.Property(e => e.Dosage).HasMaxLength(100);
            entity.Property(e => e.Frequency).HasColumnType("enum('Once','Twice','Thrice','Four times','Custom')");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.MedicineName).HasMaxLength(200);
            entity.Property(e => e.Notes).HasColumnType("text");
            entity.Property(e => e.Times).HasColumnType("json");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.Student).WithMany(p => p.Medicinereminders)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("medicinereminders_ibfk_1");
        });

        modelBuilder.Entity<Medicinereminderlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("medicinereminderlogs");

            entity.HasIndex(e => new { e.ReminderId, e.Status }, "idx_reminder_status");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Notes).HasColumnType("text");
            entity.Property(e => e.ScheduledTime).HasColumnType("timestamp");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Pending'")
                .HasColumnType("enum('Pending','Taken','Missed','Skipped')");
            entity.Property(e => e.TakenTime).HasColumnType("timestamp");

            entity.HasOne(d => d.Reminder).WithMany(p => p.Medicinereminderlogs)
                .HasForeignKey(d => d.ReminderId)
                .HasConstraintName("medicinereminderlogs_ibfk_1");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notifications");

            entity.HasIndex(e => e.CreatedAt, "idx_created");

            entity.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt }, "idx_notification_user_read_created");

            entity.HasIndex(e => new { e.UserId, e.IsRead }, "idx_user_unread");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.IsRead).HasDefaultValueSql("'0'");
            entity.Property(e => e.Message).HasColumnType("text");
            entity.Property(e => e.ReadAt).HasColumnType("timestamp");
            entity.Property(e => e.RelatedEntityType).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type).HasColumnType("enum('Appointment','Reminder','System','Health','General')");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("notifications_ibfk_1");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("feedbacks");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "idx_feedback_user_date");

            entity.Property(e => e.AdminResponse).HasColumnType("text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Message).HasColumnType("text");
            entity.Property(e => e.RespondedAt).HasColumnType("timestamp");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Pending'")
                .HasColumnType("enum('Pending','Responded')");
            entity.Property(e => e.Subject).HasMaxLength(200);

            entity.HasOne(e => e.User)
                .WithMany(p => p.Feedbacks)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("feedbacks_ibfk_1");
        });

        modelBuilder.Entity<Passwordresettoken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("passwordresettokens");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.HasIndex(e => e.Token, "idx_token").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.ExpiresAt).HasColumnType("timestamp");
            entity.Property(e => e.IsUsed).HasDefaultValueSql("'0'");

            entity.HasOne(d => d.User).WithMany(p => p.Passwordresettokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("passwordresettokens_ibfk_1");
        });

        modelBuilder.Entity<Refreshtoken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("refreshtokens");

            entity.HasIndex(e => e.Token, "idx_refreshtokens_token");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.Property(e => e.ExpiresAt).HasColumnType("timestamp");

            entity.Property(e => e.IsRevoked).HasDefaultValueSql("'0'");

            entity.Property(e => e.ReplacedByToken).HasMaxLength(200);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("refreshtokens_ibfk_1");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("prescriptions");

            entity.HasIndex(e => e.AppointmentId, "AppointmentId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Diagnosis).HasColumnType("text");
            entity.Property(e => e.Notes).HasColumnType("text");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("prescriptions_ibfk_1");
        });

        modelBuilder.Entity<Prescriptionmedicine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("prescriptionmedicines");

            entity.HasIndex(e => e.PrescriptionId, "PrescriptionId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Dosage).HasMaxLength(100);
            entity.Property(e => e.Duration).HasMaxLength(50);
            entity.Property(e => e.Frequency).HasMaxLength(100);
            entity.Property(e => e.Instructions).HasColumnType("text");
            entity.Property(e => e.MedicineName).HasMaxLength(200);

            entity.HasOne(d => d.Prescription).WithMany(p => p.Prescriptionmedicines)
                .HasForeignKey(d => d.PrescriptionId)
                .HasConstraintName("prescriptionmedicines_ibfk_1");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("reports");

            entity.HasIndex(e => e.GeneratedBy, "GeneratedBy");

            entity.HasIndex(e => new { e.ReportType, e.CreatedAt }, "idx_type_date");

            entity.Property(e => e.CompletedAt).HasColumnType("timestamp");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.Parameters).HasColumnType("json");
            entity.Property(e => e.ReportType).HasColumnType("enum('UserActivity','Appointments','DoctorPerformance','SystemUsage','HealthTrends')");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Pending'")
                .HasColumnType("enum('Pending','Processing','Completed','Failed')");

            entity.HasOne(d => d.GeneratedByNavigation).WithMany(p => p.Reports)
                .HasForeignKey(d => d.GeneratedBy)
                .HasConstraintName("reports_ibfk_1");
        });

        modelBuilder.Entity<Symptomcheck>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("symptomchecks");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "idx_user_date");

            entity.Property(e => e.Airesponse)
                .HasColumnType("json")
                .HasColumnName("AIResponse");
            entity.Property(e => e.Confidence).HasPrecision(5, 2);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Duration).HasMaxLength(50);
            entity.Property(e => e.RecommendedAction).HasMaxLength(200);
            entity.Property(e => e.Severity).HasColumnType("enum('Mild','Moderate','Severe')");
            entity.Property(e => e.Symptoms).HasColumnType("json");

            entity.HasOne(d => d.User).WithMany(p => p.Symptomchecks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("symptomchecks_ibfk_1");
        });

        modelBuilder.Entity<Systemsetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("systemsettings");

            entity.HasIndex(e => e.UpdatedBy, "UpdatedBy");

            entity.HasIndex(e => e.SettingKey, "idx_key").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.DataType)
                .HasDefaultValueSql("'String'")
                .HasColumnType("enum('String','Integer','Boolean','JSON')");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.SettingKey).HasMaxLength(100);
            entity.Property(e => e.SettingValue).HasColumnType("text");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.Systemsettings)
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("systemsettings_ibfk_1");
        });

        modelBuilder.Entity<Todaysappointment>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("todaysappointments");

            entity.Property(e => e.AppointmentTime).HasColumnType("time");
            entity.Property(e => e.DoctorName).HasMaxLength(100);
            entity.Property(e => e.PatientEmail).HasMaxLength(100);
            entity.Property(e => e.PatientName).HasMaxLength(100);
            entity.Property(e => e.PatientPhone).HasMaxLength(20);
            entity.Property(e => e.Specialization).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Pending'")
                .HasColumnType("enum('Pending','Confirmed','InProgress','Completed','Cancelled','NoShow')");
            entity.Property(e => e.Symptoms).HasColumnType("text");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.IsActive, "idx_active");

            entity.HasIndex(e => e.Role, "idx_role");

            entity.Property(e => e.Address).HasColumnType("text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasColumnType("enum('Male','Female','Other')");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.IsEmailVerified).HasDefaultValueSql("'0'");
            entity.Property(e => e.LastLoginAt).HasColumnType("timestamp");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(50);
            entity.Property(e => e.Role).HasColumnType("enum('Student','Faculty','Doctor','Admin')");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
