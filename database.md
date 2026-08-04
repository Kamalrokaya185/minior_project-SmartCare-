-- =====================================================================
-- SmartCare — MySQL 8.0+ DDL Script
-- Generated from the canonical schema definition (schema.py)
-- Differences from the PostgreSQL script (read before running):
--   * UUID -> CHAR(36); generate UUIDs in the application (recommended) or via UUID()/triggers.
--   * TIMESTAMPTZ -> DATETIME; the application MUST always store/read UTC explicitly,
--     since MySQL's DATETIME has no timezone awareness (unlike TIMESTAMPTZ in Postgres).
--   * JSONB -> JSON (MySQL's JSON type; no binary-storage equivalent to JSONB, slightly slower).
--   * Partial/filtered UNIQUE indexes (e.g. 'one active receptionist membership') are NOT
--     natively supported in MySQL and are called out explicitly below — enforce these in the
--     Application layer's command validation/transaction, as a mandatory compensating control.
--   * Row-Level Security (used in the PostgreSQL script for tenant defense-in-depth) has no MySQL
--     equivalent; MySQL deployments must rely on the EF Core global query filter layer alone,
--     making that filter a strictly load-bearing control rather than defense-in-depth on MySQL.
-- =====================================================================

-- ============ TABLE DEFINITIONS ============

-- Table: Users  (Module: Identity & Access)
CREATE TABLE `Users` (
    `Id` CHAR(36) NOT NULL,
    `Email` VARCHAR(256) NOT NULL,
    `PhoneNumber` VARCHAR(20) NULL,
    `PasswordHash` VARCHAR(512) NOT NULL,
    `FullName` VARCHAR(200) NOT NULL,
    `IsEmailVerified` TINYINT(1) NOT NULL DEFAULT 0,
    `IsPhoneVerified` TINYINT(1) NOT NULL DEFAULT 0,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `FailedLoginAttempts` INT NOT NULL DEFAULT 0,
    `LockoutEndUtc` DATETIME NULL,
    `LastLoginAtUtc` DATETIME NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAtUtc` DATETIME NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAtUtc` DATETIME NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: Roles  (Module: Identity & Access) 
        why:-store how many roles in this system
CREATE TABLE `Roles` (
    `Id` CHAR(36) NOT NULL,   =>unique primary key
    `Name` VARCHAR(50) NOT NULL, =>name of role
    `Description` VARCHAR(250) NULL, =>about that role
    `IsSystemRole` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: UserRoles  (Module: Identity & Access)
CREATE TABLE `UserRoles` (
    `Id` CHAR(36) NOT NULL,
    `UserId` CHAR(36) NOT NULL,
    `RoleId` CHAR(36) NOT NULL,
    `HospitalId` CHAR(36) NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: Permissions  (Module: Identity & Access)
CREATE TABLE `Permissions` (
    `Id` CHAR(36) NOT NULL,
    `Code` VARCHAR(100) NOT NULL,
    `Description` VARCHAR(250) NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: RolePermissions  (Module: Identity & Access)
CREATE TABLE `RolePermissions` (
    `Id` CHAR(36) NOT NULL,
    `RoleId` CHAR(36) NOT NULL,
    `PermissionId` CHAR(36) NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: RefreshTokens  (Module: Identity & Access)
CREATE TABLE `RefreshTokens` (
    `Id` CHAR(36) NOT NULL,
    `UserId` CHAR(36) NOT NULL,
    `TokenHash` VARCHAR(512) NOT NULL,
    `FamilyId` CHAR(36) NOT NULL,
    `DeviceFingerprint` VARCHAR(256) NULL,
    `IsRevoked` TINYINT(1) NOT NULL DEFAULT 0,
    `ExpiresAtUtc` DATETIME NOT NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `RevokedAtUtc` DATETIME NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: UserSessions  (Module: Identity & Access)
CREATE TABLE `UserSessions` (
    `Id` CHAR(36) NOT NULL,
    `UserId` CHAR(36) NOT NULL,
    `DeviceFingerprint` VARCHAR(256) NULL,
    `IPAddress` VARCHAR(45) NULL,
    `UserAgent` VARCHAR(500) NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LastActivityAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: LoginHistory  (Module: Identity & Access)
CREATE TABLE `LoginHistory` (
    `Id` CHAR(36) NOT NULL,
    `UserId` CHAR(36) NULL,
    `EmailAttempted` VARCHAR(256) NOT NULL,
    `IsSuccessful` TINYINT(1) NOT NULL,
    `IPAddress` VARCHAR(45) NULL,
    `UserAgent` VARCHAR(500) NULL,
    `FailureReason` VARCHAR(200) NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: AuditLogs  (Module: Identity & Access)
CREATE TABLE `AuditLogs` (
    `Id` CHAR(36) NOT NULL,
    `TenantId` CHAR(36) NULL,
    `UserId` CHAR(36) NULL,
    `Action` VARCHAR(100) NOT NULL,
    `EntityType` VARCHAR(100) NOT NULL,
    `EntityId` CHAR(36) NULL,
    `OldValues` JSON NULL,
    `NewValues` JSON NULL,
    `IPAddress` VARCHAR(45) NULL,
    `CorrelationId` CHAR(36) NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: Hospitals  (Module: Tenancy)
CREATE TABLE `Hospitals` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(200) NOT NULL,
    `Slug` VARCHAR(100) NOT NULL,
    `Email` VARCHAR(256) NULL,
    `Phone` VARCHAR(20) NULL,
    `AddressLine1` VARCHAR(250) NULL,
    `City` VARCHAR(100) NULL,
    `State` VARCHAR(100) NULL,
    `Country` VARCHAR(100) NULL,
    `PostalCode` VARCHAR(20) NULL,
    `LogoUrl` VARCHAR(500) NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Pending',
    `ApprovedAtUtc` DATETIME NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: SubscriptionPlans  (Module: Subscription)
CREATE TABLE `SubscriptionPlans` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `Price` DECIMAL(18,2) NOT NULL,
    `BillingCycle` VARCHAR(20) NOT NULL,
    `MaxDoctors` INT NULL,
    `MaxReceptionists` INT NULL,
    `Features` JSON NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: Subscriptions  (Module: Subscription)
CREATE TABLE `Subscriptions` (
    `Id` CHAR(36) NOT NULL,
    `HospitalId` CHAR(36) NOT NULL,
    `PlanId` CHAR(36) NOT NULL,
    `StartDateUtc` DATETIME NOT NULL,
    `ExpiryDateUtc` DATETIME NOT NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Active',
    `AutoRenew` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: HospitalMemberships  (Module: Tenancy)
CREATE TABLE `HospitalMemberships` (
    `Id` CHAR(36) NOT NULL,
    `UserId` CHAR(36) NOT NULL,
    `HospitalId` CHAR(36) NOT NULL,
    `MembershipType` VARCHAR(20) NOT NULL,
    `DepartmentId` CHAR(36) NULL,
    `ConsultationFee` DECIMAL(18,2) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `JoinedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LeftAtUtc` DATETIME NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: Departments  (Module: Clinical Directory)
CREATE TABLE `Departments` (
    `Id` CHAR(36) NOT NULL,
    `HospitalId` CHAR(36) NOT NULL,
    `Name` VARCHAR(150) NOT NULL,
    `Description` VARCHAR(500) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: DoctorProfiles  (Module: Clinical Directory)
CREATE TABLE `DoctorProfiles` (
    `Id` CHAR(36) NOT NULL,
    `UserId` CHAR(36) NOT NULL,
    `LicenseNumber` VARCHAR(100) NOT NULL,
    `Qualification` VARCHAR(300) NULL,
    `Specialization` VARCHAR(150) NULL,
    `ExperienceYears` INT NULL,
    `Biography` TEXT NULL,
    `AverageRating` DECIMAL(3,2) NOT NULL DEFAULT 0,
    `TotalReviews` INT NOT NULL DEFAULT 0,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: PatientProfiles  (Module: Patient Management)
CREATE TABLE `PatientProfiles` (
    `Id` CHAR(36) NOT NULL,
    `UserId` CHAR(36) NOT NULL,
    `Gender` VARCHAR(10) NULL,
    `DateOfBirth` DATE NULL,
    `EmergencyContactName` VARCHAR(150) NULL,
    `EmergencyContactPhone` VARCHAR(20) NULL,
    `AttendanceScore` DECIMAL(5,2) NOT NULL DEFAULT 100.00,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: AttendanceScoreHistory  (Module: Patient Management)
CREATE TABLE `AttendanceScoreHistory` (
    `Id` CHAR(36) NOT NULL,
    `PatientProfileId` CHAR(36) NOT NULL,
    `AppointmentId` CHAR(36) NULL,
    `PreviousScore` DECIMAL(5,2) NOT NULL,
    `NewScore` DECIMAL(5,2) NOT NULL,
    `ChangeReason` VARCHAR(50) NOT NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: DoctorSchedules  (Module: Clinical Directory)
CREATE TABLE `DoctorSchedules` (
    `Id` CHAR(36) NOT NULL,
    `HospitalMembershipId` CHAR(36) NOT NULL,
    `DayOfWeek` SMALLINT NULL,
    `SpecificDate` DATE NULL,
    `StartTime` TIME NOT NULL,
    `EndTime` TIME NOT NULL,
    `SlotDurationMinutes` INT NOT NULL DEFAULT 15,
    `IsRecurring` TINYINT(1) NOT NULL DEFAULT 1,
    `EffectiveFrom` DATE NOT NULL,
    `EffectiveTo` DATE NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: ScheduleSlots  (Module: Clinical Directory)
CREATE TABLE `ScheduleSlots` (
    `Id` CHAR(36) NOT NULL,
    `DoctorScheduleId` CHAR(36) NULL,
    `HospitalMembershipId` CHAR(36) NOT NULL,
    `SlotDate` DATE NOT NULL,
    `StartTime` TIME NOT NULL,
    `EndTime` TIME NOT NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Available',
    `ReservedUntilUtc` DATETIME NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: DoctorLeaves  (Module: Clinical Directory)
CREATE TABLE `DoctorLeaves` (
    `Id` CHAR(36) NOT NULL,
    `HospitalMembershipId` CHAR(36) NOT NULL,
    `StartDate` DATE NOT NULL,
    `EndDate` DATE NOT NULL,
    `Reason` VARCHAR(300) NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Requested',
    `ApprovedByUserId` CHAR(36) NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: HospitalPolicies  (Module: Hospital Policy)
CREATE TABLE `HospitalPolicies` (
    `Id` CHAR(36) NOT NULL,
    `HospitalId` CHAR(36) NOT NULL,
    `AdvancePaymentRequired` TINYINT(1) NOT NULL DEFAULT 1,
    `DepositPercentage` DECIMAL(5,2) NOT NULL DEFAULT 100.00,
    `CancellationWindowHours` INT NOT NULL DEFAULT 24,
    `RefundPercentage` DECIMAL(5,2) NOT NULL DEFAULT 100.00,
    `NoShowPenaltyAmount` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `BookingWindowDays` INT NOT NULL DEFAULT 30,
    `MaxFutureBookingDays` INT NOT NULL DEFAULT 60,
    `MaxDailyBookingsPerPatient` INT NOT NULL DEFAULT 3,
    `WalkInAllowed` TINYINT(1) NOT NULL DEFAULT 1,
    `DoctorLoginEnabled` TINYINT(1) NOT NULL DEFAULT 1,
    `ConfirmationRequired` TINYINT(1) NOT NULL DEFAULT 1,
    `LateArrivalGraceMinutes` INT NOT NULL DEFAULT 15,
    `AttendanceThreshold` DECIMAL(5,2) NOT NULL DEFAULT 50.00,
    `EffectiveFromUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `EffectiveToUtc` DATETIME NULL,
    `IsCurrent` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: Appointments  (Module: Appointment)
CREATE TABLE `Appointments` (
    `Id` CHAR(36) NOT NULL,
    `HospitalId` CHAR(36) NOT NULL,
    `PatientProfileId` CHAR(36) NOT NULL,
    `HospitalMembershipId` CHAR(36) NOT NULL,
    `DepartmentId` CHAR(36) NULL,
    `ScheduleSlotId` CHAR(36) NULL,
    `HospitalPolicyId` CHAR(36) NOT NULL,
    `BookingDateUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `AppointmentDate` DATE NOT NULL,
    `AppointmentTime` TIME NOT NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Pending',
    `FeeAtBooking` DECIMAL(18,2) NOT NULL,
    `Notes` VARCHAR(1000) NULL,
    `CancelledAtUtc` DATETIME NULL,
    `CancellationReason` VARCHAR(300) NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: AppointmentStatusHistory  (Module: Appointment)
CREATE TABLE `AppointmentStatusHistory` (
    `Id` CHAR(36) NOT NULL,
    `AppointmentId` CHAR(36) NOT NULL,
    `FromStatus` VARCHAR(20) NULL,
    `ToStatus` VARCHAR(20) NOT NULL,
    `ChangedByUserId` CHAR(36) NULL,
    `Reason` VARCHAR(300) NULL,
    `ChangedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: Payments  (Module: Payment)
CREATE TABLE `Payments` (
    `Id` CHAR(36) NOT NULL,
    `AppointmentId` CHAR(36) NOT NULL,
    `HospitalId` CHAR(36) NOT NULL,
    `Amount` DECIMAL(18,2) NOT NULL,
    `Method` VARCHAR(20) NOT NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Pending',
    `TransactionReference` VARCHAR(150) NULL,
    `GatewayReference` VARCHAR(150) NULL,
    `IdempotencyKey` VARCHAR(150) NULL,
    `PaidAtUtc` DATETIME NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: Invoices  (Module: Payment)
CREATE TABLE `Invoices` (
    `Id` CHAR(36) NOT NULL,
    `PaymentId` CHAR(36) NOT NULL,
    `InvoiceNumber` VARCHAR(50) NOT NULL,
    `Amount` DECIMAL(18,2) NOT NULL,
    `IssuedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `PdfUrl` VARCHAR(500) NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: RefundRequests  (Module: Payment)
CREATE TABLE `RefundRequests` (
    `Id` CHAR(36) NOT NULL,
    `PaymentId` CHAR(36) NOT NULL,
    `RequestedAmount` DECIMAL(18,2) NOT NULL,
    `ApprovedAmount` DECIMAL(18,2) NULL,
    `Reason` VARCHAR(300) NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Requested',
    `RequestedByUserId` CHAR(36) NOT NULL,
    `ApprovedByUserId` CHAR(36) NULL,
    `RequestedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ProcessedAtUtc` DATETIME NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: Reviews  (Module: Review)
CREATE TABLE `Reviews` (
    `Id` CHAR(36) NOT NULL,
    `ReviewerPatientProfileId` CHAR(36) NOT NULL,
    `AppointmentId` CHAR(36) NULL,
    `RevieweeType` VARCHAR(20) NOT NULL,
    `RevieweeId` CHAR(36) NOT NULL,
    `Rating` SMALLINT NOT NULL,
    `Comment` VARCHAR(1000) NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: NotificationTemplates  (Module: Notification)
CREATE TABLE `NotificationTemplates` (
    `Id` CHAR(36) NOT NULL,
    `HospitalId` CHAR(36) NULL,
    `TemplateKey` VARCHAR(100) NOT NULL,
    `Channel` VARCHAR(20) NOT NULL,
    `Locale` VARCHAR(10) NOT NULL DEFAULT 'en',
    `Subject` VARCHAR(200) NULL,
    `Body` TEXT NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: Notifications  (Module: Notification)
CREATE TABLE `Notifications` (
    `Id` CHAR(36) NOT NULL,
    `UserId` CHAR(36) NOT NULL,
    `TemplateId` CHAR(36) NULL,
    `Channel` VARCHAR(20) NOT NULL,
    `Recipient` VARCHAR(256) NOT NULL,
    `Subject` VARCHAR(200) NULL,
    `Body` TEXT NOT NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Queued',
    `RetryCount` INT NOT NULL DEFAULT 0,
    `SentAtUtc` DATETIME NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============ CONSTRAINTS (FK / UNIQUE / CHECK) ============

-- Constraints for `Users`
ALTER TABLE `Users` ADD CONSTRAINT `UQ_Users_1` UNIQUE (`Email`);
-- NOTE: MySQL has no native partial/filtered unique index. Conditional uniqueness ("PhoneNumber (partial, WHERE PhoneNumber IS NOT NULL)") on `Users` must be enforced in the Application layer (a validation check before insert/update inside the same transaction) or via a generated/virtual column trick (MySQL 8+) if a hard DB-level guarantee is required.

-- Constraints for `Roles`
ALTER TABLE `Roles` ADD CONSTRAINT `UQ_Roles_1` UNIQUE (`Name`);

-- Constraints for `UserRoles`
ALTER TABLE `UserRoles` ADD CONSTRAINT `FK_UserRoles_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE;
ALTER TABLE `UserRoles` ADD CONSTRAINT `FK_UserRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles`(`Id`) ON DELETE RESTRICT;
ALTER TABLE `UserRoles` ADD CONSTRAINT `FK_UserRoles_HospitalId` FOREIGN KEY (`HospitalId`) REFERENCES `Hospitals`(`Id`) ON DELETE CASCADE;
ALTER TABLE `UserRoles` ADD CONSTRAINT `UQ_UserRoles_1` UNIQUE (`UserId`, `RoleId`, `HospitalId`);

-- Constraints for `Permissions`
ALTER TABLE `Permissions` ADD CONSTRAINT `UQ_Permissions_1` UNIQUE (`Code`);

-- Constraints for `RolePermissions`
ALTER TABLE `RolePermissions` ADD CONSTRAINT `FK_RolePermissions_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles`(`Id`) ON DELETE CASCADE;
ALTER TABLE `RolePermissions` ADD CONSTRAINT `FK_RolePermissions_PermissionId` FOREIGN KEY (`PermissionId`) REFERENCES `Permissions`(`Id`) ON DELETE CASCADE;
ALTER TABLE `RolePermissions` ADD CONSTRAINT `UQ_RolePermissions_1` UNIQUE (`RoleId`, `PermissionId`);

-- Constraints for `RefreshTokens`
ALTER TABLE `RefreshTokens` ADD CONSTRAINT `FK_RefreshTokens_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE;
ALTER TABLE `RefreshTokens` ADD CONSTRAINT `UQ_RefreshTokens_1` UNIQUE (`TokenHash`);

-- Constraints for `UserSessions`
ALTER TABLE `UserSessions` ADD CONSTRAINT `FK_UserSessions_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE;

-- Constraints for `LoginHistory`
ALTER TABLE `LoginHistory` ADD CONSTRAINT `FK_LoginHistory_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE SET NULL;

-- Constraints for `AuditLogs`
ALTER TABLE `AuditLogs` ADD CONSTRAINT `FK_AuditLogs_TenantId` FOREIGN KEY (`TenantId`) REFERENCES `Hospitals`(`Id`) ON DELETE SET NULL;
ALTER TABLE `AuditLogs` ADD CONSTRAINT `FK_AuditLogs_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE SET NULL;

-- Constraints for `Hospitals`
ALTER TABLE `Hospitals` ADD CONSTRAINT `UQ_Hospitals_1` UNIQUE (`Slug`);
ALTER TABLE `Hospitals` ADD CONSTRAINT `CK_Hospitals_1` CHECK (Status IN ('Pending','Active','Suspended'));

-- Constraints for `SubscriptionPlans`
ALTER TABLE `SubscriptionPlans` ADD CONSTRAINT `UQ_SubscriptionPlans_1` UNIQUE (`Name`);
ALTER TABLE `SubscriptionPlans` ADD CONSTRAINT `CK_SubscriptionPlans_1` CHECK (BillingCycle IN ('Monthly','Yearly'));
ALTER TABLE `SubscriptionPlans` ADD CONSTRAINT `CK_SubscriptionPlans_2` CHECK (Price >= 0);

-- Constraints for `Subscriptions`
ALTER TABLE `Subscriptions` ADD CONSTRAINT `FK_Subscriptions_HospitalId` FOREIGN KEY (`HospitalId`) REFERENCES `Hospitals`(`Id`) ON DELETE CASCADE;
ALTER TABLE `Subscriptions` ADD CONSTRAINT `FK_Subscriptions_PlanId` FOREIGN KEY (`PlanId`) REFERENCES `SubscriptionPlans`(`Id`) ON DELETE RESTRICT;
ALTER TABLE `Subscriptions` ADD CONSTRAINT `CK_Subscriptions_1` CHECK (Status IN ('Active','Expired','Cancelled'));
ALTER TABLE `Subscriptions` ADD CONSTRAINT `CK_Subscriptions_2` CHECK (ExpiryDateUtc > StartDateUtc);

-- Constraints for `HospitalMemberships`
ALTER TABLE `HospitalMemberships` ADD CONSTRAINT `FK_HospitalMemberships_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE;
ALTER TABLE `HospitalMemberships` ADD CONSTRAINT `FK_HospitalMemberships_HospitalId` FOREIGN KEY (`HospitalId`) REFERENCES `Hospitals`(`Id`) ON DELETE CASCADE;
ALTER TABLE `HospitalMemberships` ADD CONSTRAINT `FK_HospitalMemberships_DepartmentId` FOREIGN KEY (`DepartmentId`) REFERENCES `Departments`(`Id`) ON DELETE SET NULL;
ALTER TABLE `HospitalMemberships` ADD CONSTRAINT `UQ_HospitalMemberships_1` UNIQUE (`UserId`, `HospitalId`, `MembershipType`);
-- NOTE: MySQL has no native partial/filtered unique index. Conditional uniqueness ("UserId WHERE MembershipType='Receptionist' AND IsActive=true (partial — enforces one active hospital per receptionist)") on `HospitalMemberships` must be enforced in the Application layer (a validation check before insert/update inside the same transaction) or via a generated/virtual column trick (MySQL 8+) if a hard DB-level guarantee is required.
ALTER TABLE `HospitalMemberships` ADD CONSTRAINT `CK_HospitalMemberships_1` CHECK (MembershipType IN ('Doctor','Receptionist'));
ALTER TABLE `HospitalMemberships` ADD CONSTRAINT `CK_HospitalMemberships_2` CHECK (ConsultationFee IS NULL OR ConsultationFee >= 0);

-- Constraints for `Departments`
ALTER TABLE `Departments` ADD CONSTRAINT `FK_Departments_HospitalId` FOREIGN KEY (`HospitalId`) REFERENCES `Hospitals`(`Id`) ON DELETE CASCADE;
ALTER TABLE `Departments` ADD CONSTRAINT `UQ_Departments_1` UNIQUE (`HospitalId`, `Name`);

-- Constraints for `DoctorProfiles`
ALTER TABLE `DoctorProfiles` ADD CONSTRAINT `FK_DoctorProfiles_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE;
ALTER TABLE `DoctorProfiles` ADD CONSTRAINT `UQ_DoctorProfiles_1` UNIQUE (`UserId`);
ALTER TABLE `DoctorProfiles` ADD CONSTRAINT `UQ_DoctorProfiles_2` UNIQUE (`LicenseNumber`);
ALTER TABLE `DoctorProfiles` ADD CONSTRAINT `CK_DoctorProfiles_1` CHECK (ExperienceYears >= 0);
ALTER TABLE `DoctorProfiles` ADD CONSTRAINT `CK_DoctorProfiles_2` CHECK (AverageRating BETWEEN 0 AND 5);

-- Constraints for `PatientProfiles`
ALTER TABLE `PatientProfiles` ADD CONSTRAINT `FK_PatientProfiles_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE;
ALTER TABLE `PatientProfiles` ADD CONSTRAINT `UQ_PatientProfiles_1` UNIQUE (`UserId`);
ALTER TABLE `PatientProfiles` ADD CONSTRAINT `CK_PatientProfiles_1` CHECK (AttendanceScore BETWEEN 0 AND 100);
ALTER TABLE `PatientProfiles` ADD CONSTRAINT `CK_PatientProfiles_2` CHECK (Gender IN ('Male','Female','Other','PreferNotToSay') OR Gender IS NULL);

-- Constraints for `AttendanceScoreHistory`
ALTER TABLE `AttendanceScoreHistory` ADD CONSTRAINT `FK_AttendanceScoreHistory_PatientProfileId` FOREIGN KEY (`PatientProfileId`) REFERENCES `PatientProfiles`(`Id`) ON DELETE CASCADE;
ALTER TABLE `AttendanceScoreHistory` ADD CONSTRAINT `FK_AttendanceScoreHistory_AppointmentId` FOREIGN KEY (`AppointmentId`) REFERENCES `Appointments`(`Id`) ON DELETE SET NULL;

-- Constraints for `DoctorSchedules`
ALTER TABLE `DoctorSchedules` ADD CONSTRAINT `FK_DoctorSchedules_HospitalMembershipId` FOREIGN KEY (`HospitalMembershipId`) REFERENCES `HospitalMemberships`(`Id`) ON DELETE CASCADE;
ALTER TABLE `DoctorSchedules` ADD CONSTRAINT `CK_DoctorSchedules_1` CHECK (EndTime > StartTime);
ALTER TABLE `DoctorSchedules` ADD CONSTRAINT `CK_DoctorSchedules_2` CHECK (SlotDurationMinutes > 0);
ALTER TABLE `DoctorSchedules` ADD CONSTRAINT `CK_DoctorSchedules_3` CHECK ((DayOfWeek IS NOT NULL) OR (SpecificDate IS NOT NULL));

-- Constraints for `ScheduleSlots`
ALTER TABLE `ScheduleSlots` ADD CONSTRAINT `FK_ScheduleSlots_DoctorScheduleId` FOREIGN KEY (`DoctorScheduleId`) REFERENCES `DoctorSchedules`(`Id`) ON DELETE SET NULL;
ALTER TABLE `ScheduleSlots` ADD CONSTRAINT `FK_ScheduleSlots_HospitalMembershipId` FOREIGN KEY (`HospitalMembershipId`) REFERENCES `HospitalMemberships`(`Id`) ON DELETE CASCADE;
ALTER TABLE `ScheduleSlots` ADD CONSTRAINT `UQ_ScheduleSlots_1` UNIQUE (`HospitalMembershipId`, `SlotDate`, `StartTime`);
ALTER TABLE `ScheduleSlots` ADD CONSTRAINT `CK_ScheduleSlots_1` CHECK (Status IN ('Available','Reserved','Booked','Blocked'));
ALTER TABLE `ScheduleSlots` ADD CONSTRAINT `CK_ScheduleSlots_2` CHECK (EndTime > StartTime);

-- Constraints for `DoctorLeaves`
ALTER TABLE `DoctorLeaves` ADD CONSTRAINT `FK_DoctorLeaves_HospitalMembershipId` FOREIGN KEY (`HospitalMembershipId`) REFERENCES `HospitalMemberships`(`Id`) ON DELETE CASCADE;
ALTER TABLE `DoctorLeaves` ADD CONSTRAINT `FK_DoctorLeaves_ApprovedByUserId` FOREIGN KEY (`ApprovedByUserId`) REFERENCES `Users`(`Id`) ON DELETE SET NULL;
ALTER TABLE `DoctorLeaves` ADD CONSTRAINT `CK_DoctorLeaves_1` CHECK (EndDate >= StartDate);
ALTER TABLE `DoctorLeaves` ADD CONSTRAINT `CK_DoctorLeaves_2` CHECK (Status IN ('Requested','Approved','Rejected'));

-- Constraints for `HospitalPolicies`
ALTER TABLE `HospitalPolicies` ADD CONSTRAINT `FK_HospitalPolicies_HospitalId` FOREIGN KEY (`HospitalId`) REFERENCES `Hospitals`(`Id`) ON DELETE CASCADE;
-- NOTE: MySQL has no native partial/filtered unique index. Conditional uniqueness ("HospitalId WHERE IsCurrent=true (partial — exactly one current policy per hospital)") on `HospitalPolicies` must be enforced in the Application layer (a validation check before insert/update inside the same transaction) or via a generated/virtual column trick (MySQL 8+) if a hard DB-level guarantee is required.
ALTER TABLE `HospitalPolicies` ADD CONSTRAINT `CK_HospitalPolicies_1` CHECK (DepositPercentage BETWEEN 0 AND 100);
ALTER TABLE `HospitalPolicies` ADD CONSTRAINT `CK_HospitalPolicies_2` CHECK (RefundPercentage BETWEEN 0 AND 100);
ALTER TABLE `HospitalPolicies` ADD CONSTRAINT `CK_HospitalPolicies_3` CHECK (CancellationWindowHours >= 0);

-- Constraints for `Appointments`
ALTER TABLE `Appointments` ADD CONSTRAINT `FK_Appointments_HospitalId` FOREIGN KEY (`HospitalId`) REFERENCES `Hospitals`(`Id`) ON DELETE RESTRICT;
ALTER TABLE `Appointments` ADD CONSTRAINT `FK_Appointments_PatientProfileId` FOREIGN KEY (`PatientProfileId`) REFERENCES `PatientProfiles`(`Id`) ON DELETE RESTRICT;
ALTER TABLE `Appointments` ADD CONSTRAINT `FK_Appointments_HospitalMembershipId` FOREIGN KEY (`HospitalMembershipId`) REFERENCES `HospitalMemberships`(`Id`) ON DELETE RESTRICT;
ALTER TABLE `Appointments` ADD CONSTRAINT `FK_Appointments_DepartmentId` FOREIGN KEY (`DepartmentId`) REFERENCES `Departments`(`Id`) ON DELETE SET NULL;
ALTER TABLE `Appointments` ADD CONSTRAINT `FK_Appointments_ScheduleSlotId` FOREIGN KEY (`ScheduleSlotId`) REFERENCES `ScheduleSlots`(`Id`) ON DELETE SET NULL;
ALTER TABLE `Appointments` ADD CONSTRAINT `FK_Appointments_HospitalPolicyId` FOREIGN KEY (`HospitalPolicyId`) REFERENCES `HospitalPolicies`(`Id`) ON DELETE RESTRICT;
-- NOTE: MySQL has no native partial/filtered unique index. Conditional uniqueness ("ScheduleSlotId (where not null)") on `Appointments` must be enforced in the Application layer (a validation check before insert/update inside the same transaction) or via a generated/virtual column trick (MySQL 8+) if a hard DB-level guarantee is required.
-- NOTE: MySQL has no native partial/filtered unique index. Conditional uniqueness ("(HospitalMembershipId, AppointmentDate, AppointmentTime) WHERE Status NOT IN ('Cancelled','Rejected','Expired')") on `Appointments` must be enforced in the Application layer (a validation check before insert/update inside the same transaction) or via a generated/virtual column trick (MySQL 8+) if a hard DB-level guarantee is required.
ALTER TABLE `Appointments` ADD CONSTRAINT `CK_Appointments_1` CHECK (Status IN ('Pending','PaymentPending','Confirmed','Completed','Cancelled','Rejected','NoShow','Expired'));
ALTER TABLE `Appointments` ADD CONSTRAINT `CK_Appointments_2` CHECK (FeeAtBooking >= 0);

-- Constraints for `AppointmentStatusHistory`
ALTER TABLE `AppointmentStatusHistory` ADD CONSTRAINT `FK_AppointmentStatusHistory_AppointmentId` FOREIGN KEY (`AppointmentId`) REFERENCES `Appointments`(`Id`) ON DELETE CASCADE;
ALTER TABLE `AppointmentStatusHistory` ADD CONSTRAINT `FK_AppointmentStatusHistory_ChangedByUserId` FOREIGN KEY (`ChangedByUserId`) REFERENCES `Users`(`Id`) ON DELETE SET NULL;

-- Constraints for `Payments`
ALTER TABLE `Payments` ADD CONSTRAINT `FK_Payments_AppointmentId` FOREIGN KEY (`AppointmentId`) REFERENCES `Appointments`(`Id`) ON DELETE RESTRICT;
ALTER TABLE `Payments` ADD CONSTRAINT `FK_Payments_HospitalId` FOREIGN KEY (`HospitalId`) REFERENCES `Hospitals`(`Id`) ON DELETE RESTRICT;
-- NOTE: MySQL has no native partial/filtered unique index. Conditional uniqueness ("IdempotencyKey (where not null)") on `Payments` must be enforced in the Application layer (a validation check before insert/update inside the same transaction) or via a generated/virtual column trick (MySQL 8+) if a hard DB-level guarantee is required.
-- NOTE: MySQL has no native partial/filtered unique index. Conditional uniqueness ("AppointmentId WHERE Status='Completed' (partial — at most one successful payment per appointment)") on `Payments` must be enforced in the Application layer (a validation check before insert/update inside the same transaction) or via a generated/virtual column trick (MySQL 8+) if a hard DB-level guarantee is required.
ALTER TABLE `Payments` ADD CONSTRAINT `CK_Payments_1` CHECK (Method IN ('Cash','Card','eSewa','Khalti','BankTransfer','Gateway'));
ALTER TABLE `Payments` ADD CONSTRAINT `CK_Payments_2` CHECK (Status IN ('Pending','Completed','Failed','Refunded','PartiallyRefunded'));
ALTER TABLE `Payments` ADD CONSTRAINT `CK_Payments_3` CHECK (Amount >= 0);

-- Constraints for `Invoices`
ALTER TABLE `Invoices` ADD CONSTRAINT `FK_Invoices_PaymentId` FOREIGN KEY (`PaymentId`) REFERENCES `Payments`(`Id`) ON DELETE RESTRICT;
ALTER TABLE `Invoices` ADD CONSTRAINT `UQ_Invoices_1` UNIQUE (`InvoiceNumber`);
ALTER TABLE `Invoices` ADD CONSTRAINT `UQ_Invoices_2` UNIQUE (`PaymentId`);
ALTER TABLE `Invoices` ADD CONSTRAINT `CK_Invoices_1` CHECK (Amount >= 0);

-- Constraints for `RefundRequests`
ALTER TABLE `RefundRequests` ADD CONSTRAINT `FK_RefundRequests_PaymentId` FOREIGN KEY (`PaymentId`) REFERENCES `Payments`(`Id`) ON DELETE RESTRICT;
ALTER TABLE `RefundRequests` ADD CONSTRAINT `FK_RefundRequests_RequestedByUserId` FOREIGN KEY (`RequestedByUserId`) REFERENCES `Users`(`Id`) ON DELETE RESTRICT;
ALTER TABLE `RefundRequests` ADD CONSTRAINT `FK_RefundRequests_ApprovedByUserId` FOREIGN KEY (`ApprovedByUserId`) REFERENCES `Users`(`Id`) ON DELETE SET NULL;
ALTER TABLE `RefundRequests` ADD CONSTRAINT `CK_RefundRequests_1` CHECK (Status IN ('Requested','Approved','Rejected','Processed'));
ALTER TABLE `RefundRequests` ADD CONSTRAINT `CK_RefundRequests_2` CHECK (RequestedAmount >= 0);
ALTER TABLE `RefundRequests` ADD CONSTRAINT `CK_RefundRequests_3` CHECK (ApprovedAmount IS NULL OR ApprovedAmount >= 0);

-- Constraints for `Reviews`
ALTER TABLE `Reviews` ADD CONSTRAINT `FK_Reviews_ReviewerPatientProfileId` FOREIGN KEY (`ReviewerPatientProfileId`) REFERENCES `PatientProfiles`(`Id`) ON DELETE CASCADE;
ALTER TABLE `Reviews` ADD CONSTRAINT `FK_Reviews_AppointmentId` FOREIGN KEY (`AppointmentId`) REFERENCES `Appointments`(`Id`) ON DELETE SET NULL;
-- NOTE: MySQL has no native partial/filtered unique index. Conditional uniqueness ("(AppointmentId, RevieweeType) WHERE AppointmentId IS NOT NULL — one review per reviewee-type per appointment") on `Reviews` must be enforced in the Application layer (a validation check before insert/update inside the same transaction) or via a generated/virtual column trick (MySQL 8+) if a hard DB-level guarantee is required.
ALTER TABLE `Reviews` ADD CONSTRAINT `CK_Reviews_1` CHECK (Rating BETWEEN 1 AND 5);
ALTER TABLE `Reviews` ADD CONSTRAINT `CK_Reviews_2` CHECK (RevieweeType IN ('Doctor','Hospital'));

-- Constraints for `NotificationTemplates`
ALTER TABLE `NotificationTemplates` ADD CONSTRAINT `FK_NotificationTemplates_HospitalId` FOREIGN KEY (`HospitalId`) REFERENCES `Hospitals`(`Id`) ON DELETE CASCADE;
ALTER TABLE `NotificationTemplates` ADD CONSTRAINT `UQ_NotificationTemplates_1` UNIQUE (`HospitalId`, `TemplateKey`, `Channel`, `Locale`);
ALTER TABLE `NotificationTemplates` ADD CONSTRAINT `CK_NotificationTemplates_1` CHECK (Channel IN ('Email','SMS','WhatsApp','Push'));

-- Constraints for `Notifications`
ALTER TABLE `Notifications` ADD CONSTRAINT `FK_Notifications_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE;
ALTER TABLE `Notifications` ADD CONSTRAINT `FK_Notifications_TemplateId` FOREIGN KEY (`TemplateId`) REFERENCES `NotificationTemplates`(`Id`) ON DELETE SET NULL;
ALTER TABLE `Notifications` ADD CONSTRAINT `CK_Notifications_1` CHECK (Channel IN ('Email','SMS','WhatsApp','Push'));
ALTER TABLE `Notifications` ADD CONSTRAINT `CK_Notifications_2` CHECK (Status IN ('Queued','Sent','Failed','Delivered'));

-- ============ APPLICATION-ENFORCED CONDITIONAL UNIQUENESS (MySQL compensating controls) ============

-- The following rules have a real, DB-level partial-unique-index guarantee in the PostgreSQL
-- script but must be enforced by the Application layer's command handlers (inside the same DB
-- transaction as the write) when running on MySQL:
--   1. Users.PhoneNumber must be unique only among non-null values.
--   2. HospitalMemberships: a User may hold at most ONE active ('IsActive'=1) row where
--      MembershipType='Receptionist'.
--   3. HospitalPolicies: exactly one row per HospitalId may have IsCurrent=1.
--   4. Appointments: no two rows may share (HospitalMembershipId, AppointmentDate, AppointmentTime)
--      while Status NOT IN ('Cancelled','Rejected','Expired').
--   5. Payments: at most one row per AppointmentId may have Status='Completed'.
--   6. Reviews: at most one row per (AppointmentId, RevieweeType) where AppointmentId IS NOT NULL.
-- Recommendation: pair the transactional check with a short-lived Redis distributed lock on the
-- contested key (as already specified for double-booking prevention) so the check-then-insert
-- is not itself racy under concurrent load.

-- ============ PERFORMANCE INDEXES ============

-- `Users`
CREATE INDEX `IX_Users_IsActive_IsDeleted` ON `Users` (`IsActive`, `IsDeleted`);

-- `UserRoles`
CREATE INDEX `IX_UserRoles_UserId` ON `UserRoles` (`UserId`);
CREATE INDEX `IX_UserRoles_HospitalId_RoleId` ON `UserRoles` (`HospitalId`, `RoleId`);

-- `RefreshTokens`
CREATE INDEX `IX_RefreshTokens_UserId_IsRevoked` ON `RefreshTokens` (`UserId`, `IsRevoked`);
CREATE INDEX `IX_RefreshTokens_FamilyId` ON `RefreshTokens` (`FamilyId`);

-- `LoginHistory`
CREATE INDEX `IX_LoginHistory_UserId_CreatedAtUtc` ON `LoginHistory` (`UserId`, `CreatedAtUtc`);
CREATE INDEX `IX_LoginHistory_IPAddress_CreatedAtUtc` ON `LoginHistory` (`IPAddress`, `CreatedAtUtc`);

-- `AuditLogs`
CREATE INDEX `IX_AuditLogs_TenantId_CreatedAtUtc` ON `AuditLogs` (`TenantId`, `CreatedAtUtc`);
CREATE INDEX `IX_AuditLogs_EntityType_EntityId` ON `AuditLogs` (`EntityType`, `EntityId`);
CREATE INDEX `IX_AuditLogs_CorrelationId` ON `AuditLogs` (`CorrelationId`);

-- `Hospitals`
CREATE INDEX `IX_Hospitals_Status` ON `Hospitals` (`Status`);

-- `Subscriptions`
CREATE INDEX `IX_Subscriptions_HospitalId_Status` ON `Subscriptions` (`HospitalId`, `Status`);
CREATE INDEX `IX_Subscriptions_ExpiryDateUtc` ON `Subscriptions` (`ExpiryDateUtc`);

-- `HospitalMemberships`
CREATE INDEX `IX_HospitalMemberships_HospitalId_Type_Active` ON `HospitalMemberships` (`HospitalId`, `MembershipType`, `IsActive`);
CREATE INDEX `IX_HospitalMemberships_UserId` ON `HospitalMemberships` (`UserId`);

-- `Departments`
CREATE INDEX `IX_Departments_HospitalId_IsActive` ON `Departments` (`HospitalId`, `IsActive`);

-- `DoctorProfiles`
CREATE INDEX `IX_DoctorProfiles_Specialization` ON `DoctorProfiles` (`Specialization`);

-- `DoctorSchedules`
CREATE INDEX `IX_DoctorSchedules_HospitalMembershipId_IsActive` ON `DoctorSchedules` (`HospitalMembershipId`, `IsActive`);

-- `ScheduleSlots`
CREATE INDEX `IX_ScheduleSlots_Membership_Date_Status` ON `ScheduleSlots` (`HospitalMembershipId`, `SlotDate`, `Status`);

-- `DoctorLeaves`
CREATE INDEX `IX_DoctorLeaves_Membership_Status` ON `DoctorLeaves` (`HospitalMembershipId`, `Status`);

-- `AttendanceScoreHistory`
CREATE INDEX `IX_AttendanceScoreHistory_Patient_CreatedAtUtc` ON `AttendanceScoreHistory` (`PatientProfileId`, `CreatedAtUtc`);

-- `HospitalPolicies`
CREATE INDEX `IX_HospitalPolicies_HospitalId_IsCurrent` ON `HospitalPolicies` (`HospitalId`, `IsCurrent`);

-- `Appointments`
CREATE INDEX `IX_Appointments_Hospital_Date_Status` ON `Appointments` (`HospitalId`, `AppointmentDate`, `Status`);
CREATE INDEX `IX_Appointments_Patient_Date` ON `Appointments` (`PatientProfileId`, `AppointmentDate`);
CREATE INDEX `IX_Appointments_Membership_Date` ON `Appointments` (`HospitalMembershipId`, `AppointmentDate`);

-- `AppointmentStatusHistory`
CREATE INDEX `IX_AppointmentStatusHistory_Appointment_ChangedAt` ON `AppointmentStatusHistory` (`AppointmentId`, `ChangedAtUtc`);

-- `Payments`
CREATE INDEX `IX_Payments_AppointmentId` ON `Payments` (`AppointmentId`);
CREATE INDEX `IX_Payments_Hospital_Status_CreatedAt` ON `Payments` (`HospitalId`, `Status`, `CreatedAtUtc`);

-- `Invoices`
CREATE INDEX `IX_Invoices_PaymentId` ON `Invoices` (`PaymentId`);

-- `RefundRequests`
CREATE INDEX `IX_RefundRequests_Payment_Status` ON `RefundRequests` (`PaymentId`, `Status`);
CREATE INDEX `IX_RefundRequests_Status` ON `RefundRequests` (`Status`);

-- `Reviews`
CREATE INDEX `IX_Reviews_RevieweeType_RevieweeId` ON `Reviews` (`RevieweeType`, `RevieweeId`);
CREATE INDEX `IX_Reviews_ReviewerPatientProfileId` ON `Reviews` (`ReviewerPatientProfileId`);

-- `NotificationTemplates`
CREATE INDEX `IX_NotificationTemplates_Key_Channel_Locale` ON `NotificationTemplates` (`TemplateKey`, `Channel`, `Locale`);

-- `Notifications`
CREATE INDEX `IX_Notifications_UserId_CreatedAtUtc` ON `Notifications` (`UserId`, `CreatedAtUtc`);
CREATE INDEX `IX_Notifications_Status_RetryCount` ON `Notifications` (`Status`, `RetryCount`);