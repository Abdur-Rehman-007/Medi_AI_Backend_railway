-- =====================================================
-- Medi-AI BUITEMS - Complete MySQL Database Schema
-- Healthcare Management System for Students & Faculty
-- =====================================================

-- Drop database if exists and create fresh
DROP DATABASE IF EXISTS mediaidb;
CREATE DATABASE mediaidb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE mediaidb;

-- =====================================================
-- 1. USERS & AUTHENTICATION
-- =====================================================

-- Users table (supports all 4 roles: Student, Faculty, Doctor, Admin)
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName VARCHAR(100) NOT NULL,
    Role ENUM('Student', 'Faculty', 'Doctor', 'Admin') NOT NULL,
    Department VARCHAR(100),
    RegistrationNumber VARCHAR(50),
    PhoneNumber VARCHAR(20),
    DateOfBirth DATE,
    Gender ENUM('Male', 'Female', 'Other'),
    Address TEXT,
    ProfileImageUrl VARCHAR(500),
    IsEmailVerified BOOLEAN DEFAULT FALSE,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    LastLoginAt TIMESTAMP NULL,
    FailedLoginAttempts INT DEFAULT 0,
    LockoutEnd TIMESTAMP NULL,
    INDEX idx_email (Email),
    INDEX idx_role (Role),
    INDEX idx_active (IsActive)
) ENGINE=InnoDB;

-- Email verification OTPs
CREATE TABLE EmailVerificationOTPs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    OTP VARCHAR(6) NOT NULL,
    ExpiresAt TIMESTAMP NOT NULL,
    IsUsed BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_user_otp (UserId, OTP, ExpiresAt)
) ENGINE=InnoDB;

-- Password reset tokens
CREATE TABLE PasswordResetTokens (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Token VARCHAR(255) UNIQUE NOT NULL,
    ExpiresAt TIMESTAMP NOT NULL,
    IsUsed BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_token (Token)
) ENGINE=InnoDB;

-- =====================================================
-- 2. DOCTOR PROFILES & AVAILABILITY
-- =====================================================

-- Doctor profiles (extends Users table)
CREATE TABLE Doctors (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT UNIQUE NOT NULL,
    Specialization VARCHAR(100) NOT NULL,
    LicenseNumber VARCHAR(50) UNIQUE NOT NULL,
    Qualification VARCHAR(200) NOT NULL,
    Experience INT DEFAULT 0, -- years of experience
    ConsultationFee DECIMAL(10,2) DEFAULT 0.00,
    RoomNumber VARCHAR(20),
    Bio TEXT,
    AverageRating DECIMAL(3,2) DEFAULT 0.00,
    TotalRatings INT DEFAULT 0,
    IsAvailable BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_specialization (Specialization),
    INDEX idx_available (IsAvailable)
) ENGINE=InnoDB;

-- Doctor availability schedule
CREATE TABLE DoctorSchedule (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DoctorId INT NOT NULL,
    DayOfWeek ENUM('Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday') NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (DoctorId) REFERENCES Doctors(Id) ON DELETE CASCADE,
    INDEX idx_doctor_day (DoctorId, DayOfWeek)
) ENGINE=InnoDB;

-- Doctor leave/unavailability dates
CREATE TABLE DoctorLeaves (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DoctorId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Reason VARCHAR(200),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (DoctorId) REFERENCES Doctors(Id) ON DELETE CASCADE,
    INDEX idx_doctor_dates (DoctorId, StartDate, EndDate)
) ENGINE=InnoDB;

-- =====================================================
-- 3. APPOINTMENTS MANAGEMENT
-- =====================================================

-- Appointments
CREATE TABLE Appointments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    PatientId INT NOT NULL,
    DoctorId INT NOT NULL,
    AppointmentDate DATE NOT NULL,
    AppointmentTime TIME NOT NULL,
    Duration INT DEFAULT 30, -- minutes
    Status ENUM('Pending','Confirmed','InProgress','Completed','Cancelled','NoShow') DEFAULT 'Pending',
    Symptoms TEXT,
    Notes TEXT,
    CancellationReason TEXT,
    CancelledBy INT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (PatientId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (DoctorId) REFERENCES Doctors(Id) ON DELETE CASCADE,
    FOREIGN KEY (CancelledBy) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX idx_patient (PatientId),
    INDEX idx_doctor (DoctorId),
    INDEX idx_date (AppointmentDate),
    INDEX idx_status (Status)
) ENGINE=InnoDB;

-- Appointment prescriptions
CREATE TABLE Prescriptions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    AppointmentId INT NOT NULL,
    Diagnosis TEXT NOT NULL,
    Notes TEXT,
    FollowUpDate DATE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- Prescription medicines
CREATE TABLE PrescriptionMedicines (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    PrescriptionId INT NOT NULL,
    MedicineName VARCHAR(200) NOT NULL,
    Dosage VARCHAR(100) NOT NULL,
    Frequency VARCHAR(100) NOT NULL,
    Duration VARCHAR(50) NOT NULL,
    Instructions TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PrescriptionId) REFERENCES Prescriptions(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- =====================================================
-- 4. MEDICINE REMINDERS (Student Feature)
-- =====================================================

-- Medicine reminders
CREATE TABLE MedicineReminders (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    StudentId INT NOT NULL,
    MedicineName VARCHAR(200) NOT NULL,
    Dosage VARCHAR(100) NOT NULL,
    Frequency ENUM('Once','Twice','Thrice','Four times','Custom') NOT NULL,
    CustomFrequency VARCHAR(100), -- for custom frequency
    Times JSON NOT NULL, -- ["08:00", "14:00", "20:00"]
    StartDate DATE NOT NULL,
    EndDate DATE,
    Notes TEXT,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (StudentId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_student_active (StudentId, IsActive)
) ENGINE=InnoDB;

-- Medicine reminder logs (track when reminder was taken)
CREATE TABLE MedicineReminderLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReminderId INT NOT NULL,
    ScheduledTime TIMESTAMP NOT NULL,
    TakenTime TIMESTAMP NULL,
    Status ENUM('Pending','Taken','Missed','Skipped') DEFAULT 'Pending',
    Notes TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ReminderId) REFERENCES MedicineReminders(Id) ON DELETE CASCADE,
    INDEX idx_reminder_status (ReminderId, Status)
) ENGINE=InnoDB;

-- =====================================================
-- 5. AI SYMPTOM CHECKER
-- =====================================================

-- Symptom checker history
CREATE TABLE SymptomChecks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Symptoms JSON NOT NULL, -- ["headache", "fever", "cough"]
    Duration VARCHAR(50),
    Severity ENUM('Mild','Moderate','Severe'),
    AIResponse JSON, -- AI diagnosis and recommendations
    Confidence DECIMAL(5,2), -- AI confidence score
    RecommendedAction VARCHAR(200),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_user_date (UserId, CreatedAt)
) ENGINE=InnoDB;

-- =====================================================
-- 6. HEALTH TIPS & ARTICLES
-- =====================================================

-- Health tips
CREATE TABLE HealthTips (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Category ENUM('Nutrition','Exercise','Mental Health','Sleep','General','Prevention','Seasonal') NOT NULL,
    Content TEXT NOT NULL,
    ImageUrl VARCHAR(500),
    Source VARCHAR(200),
    AuthorId INT,
    Views INT DEFAULT 0,
    Likes INT DEFAULT 0,
    IsPublished BOOLEAN DEFAULT TRUE,
    PublishedAt TIMESTAMP NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (AuthorId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX idx_category (Category),
    INDEX idx_published (IsPublished, PublishedAt)
) ENGINE=InnoDB;

-- Health tip user interactions
CREATE TABLE HealthTipInteractions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    HealthTipId INT NOT NULL,
    UserId INT NOT NULL,
    IsLiked BOOLEAN DEFAULT FALSE,
    IsBookmarked BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (HealthTipId) REFERENCES HealthTips(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_tip (UserId, HealthTipId)
) ENGINE=InnoDB;

-- =====================================================
-- 7. NOTIFICATIONS
-- =====================================================

-- User notifications
CREATE TABLE Notifications (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Message TEXT NOT NULL,
    Type ENUM('Appointment','Reminder','System','Health','General') NOT NULL,
    RelatedEntityId INT, -- e.g., AppointmentId, ReminderId
    RelatedEntityType VARCHAR(50), -- e.g., 'Appointment', 'Reminder'
    IsRead BOOLEAN DEFAULT FALSE,
    ReadAt TIMESTAMP NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_user_unread (UserId, IsRead),
    INDEX idx_created (CreatedAt)
) ENGINE=InnoDB;

-- =====================================================
-- 8. SYSTEM SETTINGS & CONFIGURATION
-- =====================================================

-- System settings (key-value pairs)
CREATE TABLE SystemSettings (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SettingKey VARCHAR(100) UNIQUE NOT NULL,
    SettingValue TEXT NOT NULL,
    Description TEXT,
    DataType ENUM('String','Integer','Boolean','JSON') DEFAULT 'String',
    UpdatedBy INT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX idx_key (SettingKey)
) ENGINE=InnoDB;

-- =====================================================
-- 9. AUDIT LOGS & REPORTS
-- =====================================================

-- Activity audit logs
CREATE TABLE AuditLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT,
    Action VARCHAR(100) NOT NULL,
    EntityType VARCHAR(50),
    EntityId INT,
    OldValues JSON,
    NewValues JSON,
    IpAddress VARCHAR(45),
    UserAgent TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX idx_user_action (UserId, Action),
    INDEX idx_entity (EntityType, EntityId),
    INDEX idx_created (CreatedAt)
) ENGINE=InnoDB;

-- System reports metadata
CREATE TABLE Reports (
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
    FOREIGN KEY (GeneratedBy) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_type_date (ReportType, CreatedAt)
) ENGINE=InnoDB;

-- =====================================================
-- 10. EMERGENCY CONTACTS
-- =====================================================

-- Emergency contacts for students
CREATE TABLE EmergencyContacts (
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
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_user_primary (UserId, IsPrimary)
) ENGINE=InnoDB;

-- =====================================================
-- 11. MEDICAL RECORDS
-- =====================================================

-- Patient medical history
CREATE TABLE MedicalHistory (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    PatientId INT NOT NULL,
    RecordType ENUM('Allergy','ChronicCondition','Surgery','Vaccination','FamilyHistory','Other') NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    DiagnosisDate DATE,
    Notes TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (PatientId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_patient_type (PatientId, RecordType)
) ENGINE=InnoDB;

-- =====================================================
-- 12. DOCTOR RATINGS & REVIEWS
-- =====================================================

-- Doctor ratings and reviews
CREATE TABLE DoctorReviews (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DoctorId INT NOT NULL,
    PatientId INT NOT NULL,
    AppointmentId INT NOT NULL,
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Review TEXT,
    IsAnonymous BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (DoctorId) REFERENCES Doctors(Id) ON DELETE CASCADE,
    FOREIGN KEY (PatientId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id) ON DELETE CASCADE,
    UNIQUE KEY unique_appointment_review (AppointmentId),
    INDEX idx_doctor_rating (DoctorId, Rating)
) ENGINE=InnoDB;

-- =====================================================
-- INSERT DEFAULT DATA
-- =====================================================

-- Insert default system settings
INSERT INTO SystemSettings (SettingKey, SettingValue, Description, DataType) VALUES
('AppointmentDuration', '30', 'Default appointment duration in minutes', 'Integer'),
('MaxAdvanceBookingDays', '30', 'Maximum days in advance for booking appointments', 'Integer'),
('EnableEmailNotifications', 'true', 'Enable/disable email notifications', 'Boolean'),
('EnablePushNotifications', 'true', 'Enable/disable push notifications', 'Boolean'),
('OTPExpiryMinutes', '10', 'OTP expiry time in minutes', 'Integer'),
('MaxLoginAttempts', '5', 'Maximum login attempts before account lockout', 'Integer'),
('MinPasswordLength', '6', 'Minimum password length', 'Integer'),
('HealthTipsPerPage', '10', 'Number of health tips per page', 'Integer'),
('AppVersion', '1.0.0', 'Current app version', 'String'),
('MaintenanceMode', 'false', 'Enable/disable maintenance mode', 'Boolean');

-- Insert test users (passwords are hashed - use bcrypt in your backend)
INSERT INTO Users (Email, PasswordHash, FullName, Role, Department, RegistrationNumber, PhoneNumber, IsEmailVerified, IsActive) VALUES
('student@buitms.edu.pk', '$2a$11$example_hash_for_123456', 'Ahmed Ali', 'Student', 'Computer Science', 'BUITMS-CS-2021-001', '+92-300-1234567', TRUE, TRUE),
('faculty@buitms.edu.pk', '$2a$11$example_hash_for_123456', 'Dr. Sara Khan', 'Faculty', 'Computer Science', 'FAC-CS-001', '+92-300-2345678', TRUE, TRUE),
('doctor@buitms.edu.pk', '$2a$11$example_hash_for_123456', 'Dr. Muhammad Hassan', 'Doctor', 'Medical', 'DOC-001', '+92-300-3456789', TRUE, TRUE),
('admin@buitms.edu.pk', '$2a$11$example_hash_for_123456', 'Admin User', 'Admin', 'Administration', 'ADM-001', '+92-300-4567890', TRUE, TRUE);

-- Insert doctor profile for the test doctor
INSERT INTO Doctors (UserId, Specialization, LicenseNumber, Qualification, Experience, ConsultationFee, RoomNumber, Bio, IsAvailable) VALUES
(3, 'General Physician', 'PMC-12345', 'MBBS, FCPS', 8, 500.00, 'Room 201', 'Experienced general physician specializing in student health and wellness.', TRUE);

-- Insert doctor schedule (Monday to Friday, 9 AM to 5 PM)
INSERT INTO DoctorSchedule (DoctorId, DayOfWeek, StartTime, EndTime, IsActive) VALUES
(1, 'Monday', '09:00:00', '17:00:00', TRUE),
(1, 'Tuesday', '09:00:00', '17:00:00', TRUE),
(1, 'Wednesday', '09:00:00', '17:00:00', TRUE),
(1, 'Thursday', '09:00:00', '17:00:00', TRUE),
(1, 'Friday', '09:00:00', '17:00:00', TRUE);

-- Insert sample health tips
INSERT INTO HealthTips (Title, Category, Content, AuthorId, IsPublished, PublishedAt) VALUES
('Stay Hydrated for Better Health', 'General', 'Drinking adequate water is essential for maintaining good health. Aim for 8-10 glasses daily to keep your body functioning optimally.', 2, TRUE, NOW()),
('Importance of Regular Exercise', 'Exercise', 'Regular physical activity helps maintain a healthy weight, reduces stress, and improves overall well-being. Try to exercise at least 30 minutes daily.', 2, TRUE, NOW()),
('Mental Health Matters', 'Mental Health', 'Take care of your mental health by practicing mindfulness, getting enough sleep, and reaching out for support when needed.', 2, TRUE, NOW()),
('Healthy Eating Habits', 'Nutrition', 'A balanced diet rich in fruits, vegetables, whole grains, and lean proteins supports your immune system and overall health.', 2, TRUE, NOW()),
('Quality Sleep for Better Performance', 'Sleep', 'Getting 7-9 hours of quality sleep is crucial for academic performance, concentration, and overall health.', 2, TRUE, NOW());

-- =====================================================
-- CREATE VIEWS FOR COMMON QUERIES
-- =====================================================

-- View: Today's appointments for doctors
CREATE VIEW TodaysAppointments AS
SELECT 
    a.Id AS AppointmentId,
    a.AppointmentDate,
    a.AppointmentTime,
    a.Status,
    a.Symptoms,
    u.FullName AS PatientName,
    u.Email AS PatientEmail,
    u.PhoneNumber AS PatientPhone,
    d.Id AS DoctorId,
    du.FullName AS DoctorName,
    doc.Specialization
FROM Appointments a
JOIN Users u ON a.PatientId = u.Id
JOIN Doctors d ON a.DoctorId = d.Id
JOIN Users du ON d.UserId = du.Id
JOIN Doctors doc ON d.Id = doc.Id
WHERE a.AppointmentDate = CURDATE()
ORDER BY a.AppointmentTime;

-- View: Active medicine reminders
CREATE VIEW ActiveMedicineReminders AS
SELECT 
    mr.Id AS ReminderId,
    mr.MedicineName,
    mr.Dosage,
    mr.Frequency,
    mr.Times,
    mr.StartDate,
    mr.EndDate,
    u.FullName AS StudentName,
    u.Email AS StudentEmail
FROM MedicineReminders mr
JOIN Users u ON mr.StudentId = u.Id
WHERE mr.IsActive = TRUE 
AND (mr.EndDate IS NULL OR mr.EndDate >= CURDATE());

-- View: Doctor performance summary
CREATE VIEW DoctorPerformanceSummary AS
SELECT 
    d.Id AS DoctorId,
    u.FullName AS DoctorName,
    d.Specialization,
    d.AverageRating,
    d.TotalRatings,
    COUNT(DISTINCT a.Id) AS TotalAppointments,
    COUNT(DISTINCT CASE WHEN a.Status = 'Completed' THEN a.Id END) AS CompletedAppointments,
    COUNT(DISTINCT CASE WHEN a.Status = 'Cancelled' THEN a.Id END) AS CancelledAppointments
FROM Doctors d
JOIN Users u ON d.UserId = u.Id
LEFT JOIN Appointments a ON d.Id = a.DoctorId
GROUP BY d.Id, u.FullName, d.Specialization, d.AverageRating, d.TotalRatings;

-- =====================================================
-- CREATE STORED PROCEDURES
-- =====================================================

DELIMITER //

-- Procedure: Get available doctors for a specific date and time
CREATE PROCEDURE GetAvailableDoctors(
    IN appointmentDate DATE,
    IN appointmentTime TIME
)
BEGIN
    SELECT 
        d.Id AS DoctorId,
        u.FullName AS DoctorName,
        d.Specialization,
        d.Qualification,
        d.Experience,
        d.ConsultationFee,
        d.RoomNumber,
        d.AverageRating,
        d.TotalRatings
    FROM Doctors d
    JOIN Users u ON d.UserId = u.Id
    WHERE d.IsAvailable = TRUE
    AND u.IsActive = TRUE
    AND d.Id IN (
        SELECT DoctorId 
        FROM DoctorSchedule 
        WHERE DayOfWeek = DAYNAME(appointmentDate)
        AND StartTime <= appointmentTime 
        AND EndTime >= appointmentTime
        AND IsActive = TRUE
    )
    AND d.Id NOT IN (
        SELECT DoctorId 
        FROM DoctorLeaves 
        WHERE appointmentDate BETWEEN StartDate AND EndDate
    )
    AND d.Id NOT IN (
        SELECT DoctorId 
        FROM Appointments 
        WHERE AppointmentDate = appointmentDate 
        AND AppointmentTime = appointmentTime
        AND Status IN ('Pending', 'Confirmed', 'InProgress')
    )
    ORDER BY d.AverageRating DESC, d.TotalRatings DESC;
END //

-- Procedure: Update doctor average rating
CREATE PROCEDURE UpdateDoctorRating(
    IN doctorId INT
)
BEGIN
    UPDATE Doctors 
    SET 
        AverageRating = (
            SELECT COALESCE(AVG(Rating), 0) 
            FROM DoctorReviews 
            WHERE DoctorId = doctorId
        ),
        TotalRatings = (
            SELECT COUNT(*) 
            FROM DoctorReviews 
            WHERE DoctorId = doctorId
        )
    WHERE Id = doctorId;
END //

DELIMITER ;

-- =====================================================
-- CREATE TRIGGERS
-- =====================================================

DELIMITER //

-- Trigger: Update doctor rating after new review
CREATE TRIGGER after_doctor_review_insert
AFTER INSERT ON DoctorReviews
FOR EACH ROW
BEGIN
    CALL UpdateDoctorRating(NEW.DoctorId);
END //

-- Trigger: Update doctor rating after review update
CREATE TRIGGER after_doctor_review_update
AFTER UPDATE ON DoctorReviews
FOR EACH ROW
BEGIN
    CALL UpdateDoctorRating(NEW.DoctorId);
END //

-- Trigger: Update doctor rating after review deletion
CREATE TRIGGER after_doctor_review_delete
AFTER DELETE ON DoctorReviews
FOR EACH ROW
BEGIN
    CALL UpdateDoctorRating(OLD.DoctorId);
END //

-- Trigger: Increment health tip views
CREATE TRIGGER before_health_tip_interaction_insert
BEFORE INSERT ON HealthTipInteractions
FOR EACH ROW
BEGIN
    UPDATE HealthTips 
    SET Views = Views + 1 
    WHERE Id = NEW.HealthTipId;
END //

DELIMITER ;

-- =====================================================
-- INDEXES FOR OPTIMIZATION
-- =====================================================

-- Already created inline with tables above
-- Additional composite indexes for complex queries:
CREATE INDEX idx_appointment_doctor_date_status ON Appointments(DoctorId, AppointmentDate, Status);
CREATE INDEX idx_appointment_patient_date_status ON Appointments(PatientId, AppointmentDate, Status);
CREATE INDEX idx_notification_user_read_created ON Notifications(UserId, IsRead, CreatedAt);

-- =====================================================
-- GRANT PERMISSIONS (Update with your actual username)
-- =====================================================

-- Create application user (change password in production)
-- CREATE USER IF NOT EXISTS 'mediaiapp'@'localhost' IDENTIFIED BY 'SecurePassword123!';
-- GRANT SELECT, INSERT, UPDATE, DELETE ON mediaidb.* TO 'mediaiapp'@'localhost';
-- FLUSH PRIVILEGES;

-- =====================================================
-- DATABASE BACKUP RECOMMENDATION
-- =====================================================

-- Schedule regular backups using:
-- mysqldump -u root -p mediaidb > mediaidb_backup_$(date +%Y%m%d).sql

-- =====================================================
-- END OF DATABASE SCHEMA
-- =====================================================
