-- =====================================================
-- Medi-AI BUITEMS - MySQL Database Schema
-- Derived from the ASP.NET Core backend and Flutter frontend models
-- Database name kept as mediaidb to match the backend connection string
-- =====================================================

DROP DATABASE IF EXISTS mediaidb;
CREATE DATABASE mediaidb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE mediaidb;

SET NAMES utf8mb4;
SET time_zone = '+00:00';

-- =====================================================
-- 1. USERS & AUTHENTICATION
-- =====================================================

CREATE TABLE users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(100) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName VARCHAR(100) NOT NULL,
    Role ENUM('Student','Faculty','Doctor','Admin') NOT NULL,
    Department VARCHAR(100),
    RegistrationNumber VARCHAR(50),
    PhoneNumber VARCHAR(20),
    DateOfBirth DATE,
    Gender VARCHAR(20),
    Address TEXT,
    ProfileImageUrl VARCHAR(500),
    IsEmailVerified BOOLEAN DEFAULT FALSE,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    LastLoginAt TIMESTAMP NULL,
    FailedLoginAttempts INT DEFAULT 0,
    LockoutEnd TIMESTAMP NULL,
    UNIQUE KEY uq_users_email (Email),
    INDEX idx_users_role (Role),
    INDEX idx_users_active (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE emailverificationotps (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    OTP VARCHAR(6) NOT NULL,
    ExpiresAt TIMESTAMP NOT NULL,
    IsUsed BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_emailverificationotps_user
        FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE,
    INDEX idx_emailverificationotps_lookup (UserId, OTP, ExpiresAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE passwordresettokens (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Token VARCHAR(255) NOT NULL,
    ExpiresAt TIMESTAMP NOT NULL,
    IsUsed BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_passwordresettokens_user
        FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE,
    UNIQUE KEY uq_passwordresettokens_token (Token),
    INDEX idx_passwordresettokens_token (Token)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- 2. DOCTOR PROFILES & SCHEDULES
-- =====================================================

CREATE TABLE doctors (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Specialization VARCHAR(100) NOT NULL,
    LicenseNumber VARCHAR(50) NOT NULL,
    Qualification VARCHAR(200) NOT NULL,
    Experience INT DEFAULT 0,
    RoomNumber VARCHAR(20),
    Bio TEXT,
    AverageRating DECIMAL(3,2) DEFAULT 0.00,
    TotalRatings INT DEFAULT 0,
    IsAvailable BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY uq_doctors_user (UserId),
    UNIQUE KEY uq_doctors_license (LicenseNumber),
    INDEX idx_doctors_specialization (Specialization),
    INDEX idx_doctors_available (IsAvailable),
    CONSTRAINT fk_doctors_user
        FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE doctorschedules (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DoctorId INT NOT NULL,
    DayOfWeek ENUM('Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday') NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_doctorschedules_doctor_day (DoctorId, DayOfWeek),
    INDEX idx_doctorschedules_day (DoctorId, DayOfWeek),
    CONSTRAINT chk_doctorschedules_time_range CHECK (StartTime < EndTime),
    CONSTRAINT fk_doctorschedules_doctor
        FOREIGN KEY (DoctorId) REFERENCES doctors(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE doctorleaves (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DoctorId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Reason VARCHAR(200),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_doctorleaves_dates (DoctorId, StartDate, EndDate),
    CONSTRAINT fk_doctorleaves_doctor
        FOREIGN KEY (DoctorId) REFERENCES doctors(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- 3. APPOINTMENTS & PRESCRIPTIONS
-- =====================================================

CREATE TABLE appointments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    PatientId INT NOT NULL,
    DoctorId INT NOT NULL,
    AppointmentDate DATE NOT NULL,
    AppointmentTime TIME NOT NULL,
    Duration INT DEFAULT 30,
    Status ENUM('Pending','Confirmed','InProgress','Completed','Cancelled','NoShow') DEFAULT 'Pending',
    Symptoms TEXT,
    Notes TEXT,
    CancellationReason TEXT,
    CancelledBy INT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_appointments_patient (PatientId),
    INDEX idx_appointments_doctor (DoctorId),
    INDEX idx_appointments_date (AppointmentDate),
    INDEX idx_appointments_status (Status),
    INDEX idx_appointments_doctor_date_status (DoctorId, AppointmentDate, Status),
    INDEX idx_appointments_patient_date_status (PatientId, AppointmentDate, Status),
    CONSTRAINT fk_appointments_patient
        FOREIGN KEY (PatientId) REFERENCES users(Id) ON DELETE CASCADE,
    CONSTRAINT fk_appointments_doctor
        FOREIGN KEY (DoctorId) REFERENCES doctors(Id) ON DELETE CASCADE,
    CONSTRAINT fk_appointments_cancelledby
        FOREIGN KEY (CancelledBy) REFERENCES users(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE prescriptions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    AppointmentId INT NOT NULL,
    Diagnosis TEXT NOT NULL,
    Notes TEXT,
    FollowUpDate DATE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_prescriptions_appointment (AppointmentId),
    CONSTRAINT fk_prescriptions_appointment
        FOREIGN KEY (AppointmentId) REFERENCES appointments(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE prescriptionmedicines (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    PrescriptionId INT NOT NULL,
    MedicineName VARCHAR(200) NOT NULL,
    Dosage VARCHAR(100) NOT NULL,
    Frequency VARCHAR(100) NOT NULL,
    Duration VARCHAR(50) NOT NULL,
    Instructions TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_prescriptionmedicines_prescription (PrescriptionId),
    CONSTRAINT fk_prescriptionmedicines_prescription
        FOREIGN KEY (PrescriptionId) REFERENCES prescriptions(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE doctorreviews (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DoctorId INT NOT NULL,
    PatientId INT NOT NULL,
    AppointmentId INT NOT NULL,
    Rating INT NOT NULL,
    Review TEXT,
    IsAnonymous BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY uq_doctorreviews_appointment (AppointmentId),
    INDEX idx_doctorreviews_rating (DoctorId, Rating),
    CONSTRAINT fk_doctorreviews_doctor
        FOREIGN KEY (DoctorId) REFERENCES doctors(Id) ON DELETE CASCADE,
    CONSTRAINT fk_doctorreviews_patient
        FOREIGN KEY (PatientId) REFERENCES users(Id) ON DELETE CASCADE,
    CONSTRAINT fk_doctorreviews_appointment
        FOREIGN KEY (AppointmentId) REFERENCES appointments(Id) ON DELETE CASCADE,
    CONSTRAINT chk_doctorreviews_rating CHECK (Rating BETWEEN 1 AND 5)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- 4. MEDICINE REMINDERS
-- =====================================================

CREATE TABLE medicinereminders (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    StudentId INT NOT NULL,
    MedicineName VARCHAR(200) NOT NULL,
    Dosage VARCHAR(100) NOT NULL,
    Frequency ENUM('Once','Twice','Thrice','Four times','Custom') NOT NULL,
    CustomFrequency VARCHAR(100),
    Times JSON NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE,
    Notes TEXT,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_medicinereminders_student_active (StudentId, IsActive),
    CONSTRAINT fk_medicinereminders_student
        FOREIGN KEY (StudentId) REFERENCES users(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE medicinereminderlogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReminderId INT NOT NULL,
    ScheduledTime TIMESTAMP NOT NULL,
    TakenTime TIMESTAMP NULL,
    Status ENUM('Pending','Taken','Missed','Skipped') DEFAULT 'Pending',
    Notes TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_medicinereminderlogs_status (ReminderId, Status),
    CONSTRAINT fk_medicinereminderlogs_reminder
        FOREIGN KEY (ReminderId) REFERENCES medicinereminders(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- 5. AI SYMPTOM CHECKER
-- =====================================================

CREATE TABLE symptomchecks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Symptoms JSON NOT NULL,
    Duration VARCHAR(50),
    Severity ENUM('Mild','Moderate','Severe'),
    AIResponse JSON,
    Confidence DECIMAL(5,2),
    RecommendedAction VARCHAR(200),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_symptomchecks_user_date (UserId, CreatedAt),
    CONSTRAINT fk_symptomchecks_user
        FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- 6. HEALTH TIPS
-- =====================================================



-- =====================================================
-- 7. NOTIFICATIONS
-- =====================================================

CREATE TABLE notifications (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Message TEXT NOT NULL,
    Type ENUM('Appointment','Reminder','System','Health','General') NOT NULL,
    RelatedEntityId INT,
    RelatedEntityType VARCHAR(50),
    IsRead BOOLEAN DEFAULT FALSE,
    ReadAt TIMESTAMP NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_notifications_user_unread (UserId, IsRead),
    INDEX idx_notifications_created (CreatedAt),
    CONSTRAINT fk_notifications_user
        FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- 8. MEDICAL HISTORY & EMERGENCY CONTACTS
-- =====================================================

CREATE TABLE medicalhistories (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    PatientId INT NOT NULL,
    RecordType ENUM('Allergy','ChronicCondition','Surgery','Vaccination','FamilyHistory','Other') NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    DiagnosisDate DATE,
    Notes TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_medicalhistories_patient_type (PatientId, RecordType),
    CONSTRAINT fk_medicalhistories_patient
        FOREIGN KEY (PatientId) REFERENCES users(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE emergencycontacts (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    ContactName VARCHAR(100) NOT NULL,
    Relationship VARCHAR(50) NOT NULL,
    PhoneNumber VARCHAR(20) NOT NULL,
    Email VARCHAR(100),
    Address TEXT,
    IsPrimary BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_emergencycontacts_user_primary (UserId, IsPrimary),
    CONSTRAINT fk_emergencycontacts_user
        FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- 9. SYSTEM SETTINGS, AUDIT LOGS, REPORTS
-- =====================================================

CREATE TABLE systemsettings (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SettingKey VARCHAR(100) NOT NULL,
    SettingValue TEXT NOT NULL,
    Description TEXT,
    DataType ENUM('String','Integer','Boolean','JSON') DEFAULT 'String',
    UpdatedBy INT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY uq_systemsettings_key (SettingKey),
    INDEX idx_systemsettings_key (SettingKey),
    CONSTRAINT fk_systemsettings_updatedby
        FOREIGN KEY (UpdatedBy) REFERENCES users(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE auditlogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NULL,
    Action VARCHAR(100) NOT NULL,
    EntityType VARCHAR(50),
    EntityId INT,
    OldValues JSON,
    NewValues JSON,
    IpAddress VARCHAR(45),
    UserAgent TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_auditlogs_user_action (UserId, Action),
    INDEX idx_auditlogs_entity (EntityType, EntityId),
    INDEX idx_auditlogs_created (CreatedAt),
    CONSTRAINT fk_auditlogs_user
        FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE reports (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReportType ENUM('UserActivity','Appointments','DoctorPerformance','SystemUsage','HealthTrends') NOT NULL,
    GeneratedBy INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Parameters JSON,
    FileUrl VARCHAR(500),
    Status ENUM('Pending','Processing','Completed','Failed') DEFAULT 'Pending',
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CompletedAt TIMESTAMP NULL,
    INDEX idx_reports_type_date (ReportType, CreatedAt),
    CONSTRAINT fk_reports_generatedby
        FOREIGN KEY (GeneratedBy) REFERENCES users(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- 10. VIEWS USED BY THE BACKEND
-- =====================================================

CREATE OR REPLACE VIEW todaysappointments AS
SELECT
    a.Id AS AppointmentId,
    a.AppointmentDate,
    a.AppointmentTime,
    a.Status,
    a.Symptoms,
    a.Notes,
    a.DoctorId,
    a.PatientId,
    p.FullName AS PatientName,
    p.Email AS PatientEmail,
    p.PhoneNumber AS PatientPhone,
    d.UserId AS DoctorUserId,
    du.FullName AS DoctorName,
    d.Specialization,
    d.RoomNumber
FROM appointments a
JOIN users p ON a.PatientId = p.Id
JOIN doctors d ON a.DoctorId = d.Id
JOIN users du ON d.UserId = du.Id
WHERE a.AppointmentDate = CURDATE();

CREATE OR REPLACE VIEW activemedicinereminders AS
SELECT
    mr.Id AS ReminderId,
    mr.StudentId,
    mr.MedicineName,
    mr.Dosage,
    mr.Frequency,
    mr.CustomFrequency,
    mr.Times,
    mr.StartDate,
    mr.EndDate,
    mr.Notes,
    mr.IsActive,
    u.FullName AS StudentName,
    u.Email AS StudentEmail
FROM medicinereminders mr
JOIN users u ON mr.StudentId = u.Id
WHERE mr.IsActive = TRUE
  AND (mr.EndDate IS NULL OR mr.EndDate >= CURDATE());

CREATE OR REPLACE VIEW doctorperformancesummary AS
SELECT
    d.Id AS DoctorId,
    du.FullName AS DoctorName,
    d.Specialization,
    d.AverageRating,
    d.TotalRatings,
    COUNT(DISTINCT a.Id) AS TotalAppointments,
    COUNT(DISTINCT CASE WHEN a.Status = 'Completed' THEN a.Id END) AS CompletedAppointments,
    COUNT(DISTINCT CASE WHEN a.Status = 'Cancelled' THEN a.Id END) AS CancelledAppointments
FROM doctors d
JOIN users du ON d.UserId = du.Id
LEFT JOIN appointments a ON d.Id = a.DoctorId
GROUP BY d.Id, du.FullName, d.Specialization, d.AverageRating, d.TotalRatings;

-- =====================================================
-- 11. TRIGGERS
-- =====================================================

DELIMITER //

DROP TRIGGER IF EXISTS after_appointment_insert_notify_doctor //
CREATE TRIGGER after_appointment_insert_notify_doctor
AFTER INSERT ON appointments
FOR EACH ROW
BEGIN
    INSERT INTO notifications (
        UserId,
        Title,
        Message,
        Type,
        RelatedEntityId,
        RelatedEntityType,
        IsRead,
        CreatedAt
    )
    SELECT
        d.UserId,
        'New Appointment Booking',
        CONCAT(
            'A new appointment is booked for ',
            DATE_FORMAT(NEW.AppointmentDate, '%Y-%m-%d'),
            ' at ',
            TIME_FORMAT(NEW.AppointmentTime, '%H:%i')
        ),
        'Appointment',
        NEW.Id,
        'Appointment',
        FALSE,
        UTC_TIMESTAMP()
    FROM doctors d
    WHERE d.Id = NEW.DoctorId;
END //

DELIMITER ;

-- =====================================================
-- 12. OPTIONAL SEED DATA FOR APP SETTINGS
-- =====================================================

INSERT INTO systemsettings (SettingKey, SettingValue, Description, DataType) VALUES
('AppointmentDuration', '30', 'Default appointment duration in minutes', 'Integer'),
('MaxAdvanceBookingDays', '30', 'Maximum days in advance for booking appointments', 'Integer'),
('EnableEmailNotifications', 'true', 'Enable or disable email notifications', 'Boolean'),
('EnablePushNotifications', 'true', 'Enable or disable push notifications', 'Boolean'),
('OTPExpiryMinutes', '10', 'OTP expiry time in minutes', 'Integer'),
('MaxLoginAttempts', '5', 'Maximum login attempts before account lockout', 'Integer'),
('MinPasswordLength', '6', 'Minimum password length', 'Integer'),
('HealthTipsPerPage', '10', 'Number of health tips per page', 'Integer'),
('AppVersion', '1.0.0', 'Current app version', 'String'),
('MaintenanceMode', 'false', 'Enable or disable maintenance mode', 'Boolean'),
('DoctorBookingSettings:1', '{"appointmentDuration":30,"maxPatientsPerDay":16,"autoConfirmAppointments":false,"enableBreakTime":false,"breakStartTime":"12:00","breakEndTime":"13:00","enableAppointmentReminders":true,"enableMedicineReminders":true,"reminderNotificationMinutes":15}', 'Doctor-specific booking settings in JSON format (key pattern: DoctorBookingSettings:{doctorId})', 'JSON');
