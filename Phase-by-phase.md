# Process
---
## step-0 (DataBase designe)

**Module :- Identity & Access**

### 1. **Users** 
*CREATE TABLE Users* 
1. Id` CHAR(36) NOT NULL,
2. Email VARCHAR(256) UNIQUE NOT NULL,
3. PhoneNumber` VARCHAR(20) NULL,
4. PasswordHash` VARCHAR(512) NOT NULL,
5. FullName` VARCHAR(200) NOT NULL,
6. IsEmailVerified` TINYINT(1) NOT NULL DEFAULT 0,
7. IsPhoneVerified` TINYINT(1) NOT NULL DEFAULT 0,
8. IsActive` TINYINT(1) NOT NULL DEFAULT 1,
9. FailedLoginAttempts` INT NOT NULL DEFAULT 0,
10. LockoutEndUtc` DATETIME NULL,
11. LastLoginAtUtc` DATETIME NULL,
12. CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
13. UpdatedAtUtc` DATETIME NULL,
14. IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
15. DeletedAtUtc` DATETIME NULL,

### 2. **Tables:Roles** (why:-Stores all system roles that users can have. A role defines what type of user they are in the system.)

*CREATE TABLE Roles*
1. id (CHAR(36)) ==> Unique identifier (Primary Key) for each role.
2. name TINYINT	==>Name of the role (e.g., 0->Super Admin, 1->Clinic, 2->Patient).
3. description (VARCHAR250)	==>Short explanation of what this role can do.
4. isSystemRole (BOOLEAN)	==>Indicates whether the role is built into the system. 1 = System Role, 0 = Custom Role.
5. createdAtUtc (DATETIME)	==>Stores when this role was created.

### 3. **Tables:UserRoles** (why:- Connects users with their roles. A user can have different roles in different hospitals.)

*CREATE TABLE UserRoles*
1. id	==>Unique record identifier.
2. userId ==> References the user receiving the role.
3. roleId ==> References the assigned role.
4. profileId	==>Specifies profileId according to roles (eg:- roles is patient then return id which is link to then patientprofile table ,if roles clinic then is to link clinic table).
5. createdAtUtc	==>Date and time the role was assigned.

### 4. **Table: RefreshTokens** (why:-Provides secure login without requiring users to enter their password repeatedly.)

*CREATE TABLE RefreshTokens*
1. id	==>Unique token record.
2. userId	==>Owner of the refresh token.
3. tokenHash	==>Encrypted refresh token (never store the original token).
4. deviceFingerprint	==>Identifies the user's device/browser.(for future)
5. IPAddress VARCHAR(45)
5. isRevoked	==>Indicates whether the token has been invalidated.
6. expiresAtUtc	==>Expiration date of the refresh token.
7. createdAtUtc	==>Token creation time.
8. revokedAtUtc	==>Date and time the token was revoked.


**Module: Clinic Management**

### 5.**Table: Clinic** (why:-Stores all hospitals/clinics registered in the SmartCare system. This table contains the clinic's basic information and is referenced by many other tables (Departments, Memberships, Policies, Appointments, etc.).)

*CREATE TABLE Clinic*
1. id	CHAR(36)	==>Unique Primary Key (UUID) for each clinic.
2. name	VARCHAR(150)	==>Official clinic or hospital name.
3. slug	VARCHAR(150)	==>URL-friendly unique name (e.g., city-hospital). Used in website URLs.
4. email	VARCHAR(255)	==>Official clinic email address.
5. phone	VARCHAR(20)	==>Contact phone number.
6. address	VARCHAR(255)	==>Street address of the clinic.
7. city	VARCHAR(100)	==>City where the clinic is located.
8. state	VARCHAR(100)	==>State/Province.
9. logoUrl	VARCHAR(500)	==>Path or URL of the clinic logo.
10. Status TINYINT	==>Current status (0 Pending 1 Approved 2 Suspended 3 Active).
11. approvedAtUtc	DATETIME	==>Date and time when Super Admin approved the clinic.
12. createdAtUtc	DATETIME	==>Clinic registration date.
13. isDeleted	TINYINT(1)	==>Soft delete flag (1 = Deleted, 0 = Active).


### 6. **Table: PatientProfiles** (why:-Stores patient-specific information that is not common to all users. Login information remains in the Users table, while patient information is stored here.)

*CREATE TABLE PatientProfiles*
1. id	CHAR(36)	==>Unique Primary Key.
2. userId	CHAR(36)	==>Links this profile to the Users table.
3. gender	VARCHAR(20)	==>Patient gender.
4. dob	DATE	==>Date of birth.
5. NID	VARCHAR(50)	==>National ID (Optional). Helps prevent duplicate registrations.
6. BloodGroup VARCHAR(5) NULL
7. emergencyContactName	VARCHAR(150)	==>Name of emergency contact person.
8. emergencyContactRelationship	VARCHAR(100)	==>Relationship (Father, Mother, Wife, Brother, etc.).
9. emergencyContactPhone	VARCHAR(20)	==>Emergency contact phone number.
10. createdAtUtc	DATETIME	==>Profile creation date.
11. updatedAtUtc	DATETIME	==>Last profile update date.


### 7. **Table: ClinicPolicies** (why:-Stores each clinic's appointment and booking rules. This allows every clinic to have different policies without changing the application code.)

*CREATE TABLE ClinicPolicies* 
1. id	CHAR(36)	==>Unique Primary Key.
2. clinicId	CHAR(36)	==>Clinic that owns these policies.
3. advancePaymentRequired	TINYINT(1)	==>Whether advance payment is required.
4. depositPercentage	DECIMAL(5,2)	==>Percentage of payment required before booking.
5. cancellationWindowHours	INT	 ==>Hours before appointment when cancellation is allowed.
6. refundPercentage	DECIMAL(5,2)	==>Percentage refunded after cancellation.
7. noShowPenaltyAmount	DECIMAL(18,2)	==>Penalty charged for no-shows.
8. bookingWindowDays	INT	 ==>Maximum number of days in advance that appointments can be booked.
9. maxDailyBookingDays	INT	 ==>Maximum appointments a patient can book in one day. (I'd rename this to MaxDailyBookingsPerPatient for clarity.)
10. receptionistAllowed	TINYINT(1)	==>Whether receptionists can create walk-in appointments. (I'd rename this to WalkInBookingAllowed.)
11. confirmationRequired	TINYINT(1)	==>Whether receptionist approval is required after booking.
12. lateArrivalGraceMinutes	INT	==>Grace period before marking a patient late/no-show.
13. minAttendancePercentage	DECIMAL(5,2)	==>Minimum attendance percentage before penalties apply.
14. allowedReschedule	TINYINT(1)	==>Whether rescheduling is allowed.
15. maxReschedule	INT	==>Maximum number of reschedules allowed.
16. effectiveFromUtc	DATETIME	==>Policy start date.
16. effectiveToUtc	DATETIME	==>Policy end date (optional).
17. isCurrent	TINYINT(1)	==>Indicates whether this is the active policy.



### 8. **Table: ClinicMembership** (why:-Connects users to clinics and defines what role they perform in that clinic. A user can belong to multiple clinics with different responsibilities.)

*CREATE TALBE ClinicMembership*
1. Id	CHAR(36)	==>Unique Primary Key.
2. ClinicId	CHAR(36)	==>Clinic where the user works.
3. DoctorId             ==>To refrence which doctor work in this clinc with this department
4. DepartmentId	CHAR(36)	==>Department assignment (optional).
5. ConsultationFee DECIMAL(18,2)   ==>doctor fee for checking.
5. IsActive	TINYINT(1)	==>Whether the membership is currently active.
6. JoinedAtUtc	DATETIME	==>Date joined the clinic.
7. LeftAtUtc	DATETIME	==>Date left the clinic (optional).


### 9. **Table: Department**  (why:-Stores medical departments within each clinic.)

*CREATE TABLE Department*
1. Id	CHAR(36)	Unique Primary Key.
2. ClinicId	CHAR(36)	Clinic that owns the department.
3. Name	VARCHAR(100)	Department name.
4. Description	VARCHAR(300)	Brief description of the department.
5. IsActive	TINYINT(1)	Whether the department is active.
6. CreatedAtUtc	DATETIME	Department creation date.


### 10. **Table: DoctorProfile** (why:-Stores doctor-specific professional information. Login details remain in the Users table, while professional details are stored here.)

*CREATE TABLE DoctorProfile*
1. Id	CHAR(36)	Unique Primary Key.
2. Fullname 
3. LicenseNumber	VARCHAR(100)	Government-issued medical licence number.
4. Specialization	VARCHAR(150)	Doctor's specialty.
5. Gender VARCHAR(20)
6. CreatedAtUtc	DATETIME	Doctor profile creation date.



**Module: Scheduling & Appointment**

### 11. **Table: AppointmentStatusHistory** (why:-Stores the complete history of every appointment status change. It helps track who changed the appointment, when it was changed, and why it was changed. This table acts as an audit trail for appointments.)

*CREATE TABLE AppointmentStatusHistory*
1. Id	CHAR(36)	Unique Primary Key.
2. AppointmentId	CHAR(36)	References the appointment whose status changed.
3. FromStatus	VARCHAR(30)	Previous appointment status.
4. ToStatus	VARCHAR(30)	New appointment status.
5. ChangedByUserId	CHAR(36)	User who changed the status (Patient, Receptionist, System).
6. Reason	VARCHAR(300)	Optional reason for the status change.
7. ChangedAtUtc	DATETIME	Date and time when the status changed.


### 12. **Table: DoctorSchedules** (why:-Defines a doctor's regular working schedule. It specifies when a doctor is available so the system can automatically generate appointment slots.)

*CREATE TABLE DoctorSchedules*
1. Id	CHAR(36)	Unique Primary Key.
2. ClinicMembershipId	CHAR(36)	Doctor's membership in a clinic.
3. DayOfWeek	TINYINT	Day of the week (1=Monday ... 7=Sunday).
4. SpecificDate	DATE	Used for one-time schedules instead of recurring days.
5. StartTime	TIME	Doctor's working start time.
6. EndTime	TIME	Doctor's working end time.
7. SlotDurationMinutes	INT	Length of each appointment slot.
8. IsRecurring	TINYINT(1)	Whether the schedule repeats weekly.
9. EffectiveFrom	DATE	Date from which this schedule becomes active.
10. EffectiveTo	DATE	Date until which this schedule remains valid.
11. IsActive	TINYINT(1)	Indicates whether this schedule is currently active.


### 13. **Table: ScheduleSlots** (why:-Stores the individual appointment slots generated from a doctor's schedule. Patients book these slots instead of booking directly from the doctor's schedule.)

*CREATE TABLE ScheduleSlots*
1. Id	CHAR(36)	Unique Primary Key.
2. DoctorScheduleId     References the parent schedule.
3. SlotDate	DATE	Date of the appointment slot.
4. StartTime	TIME	Slot starting time.
5. EndTime	TIME	Slot ending time.
6. Status	TINYINT	Current slot status (0->Available(free slot), 1->Reserved(temprory bookrd), 2->Booked(perment book), 3->Completed(while after patient attemded slot), 4->Blocked(doctor leave / clinic manually blocked this time)).
7. ReservedUntilUtc	DATETIME	Temporary reservation expiry time during booking/payment.
8. CreatedAtUtc	DATETIME	Slot creation date.

### 14. **Table: Appointments**  (why:-Stores every appointment booked through SmartCare. This is the core table of the system.)

*CREATE TABLE Appointments*
1. Id	CHAR(36)	Unique Primary Key.
2. ClinicId	CHAR(36)	Clinic where the appointment is booked.
3. PatientProfileId	CHAR(36)	Patient attending the appointment.
4. DoctorId	CHAR(36)	Doctor assigned to the appointment.
5. DepartmentId	CHAR(36)	Medical department.
6. ScheduleSlotId	CHAR(36)	Reserved appointment slot.
7. BookingDateUtc	DATETIME	Date and time when the booking was made.
8. AppointmentDate	DATE	Appointment date.
9. AppointmentTime	TIME	Appointment time.
10. Status	VARCHAR(30)	Current appointment status.
11. FeeAtBooking	DECIMAL(18,2)	Consultation fee at booking time.
12. Notes	TEXT	Additional booking notes.
13. CancelledAtUtc	DATETIME	Cancellation date (if cancelled).
14. CancellationReason	VARCHAR(300)	Reason for cancellation.
15. CreatedAtUtc	DATETIME	Appointment creation date.
16. IsDeleted	TINYINT(1)	Soft delete flag.

| Column                  | Data Type     | Purpose                                           |
| ----------------------- | ------------- | ------------------------------------------------- |
| ******1.****** PaymentStatus           | TINYINT       | NotPaid, AwaitingVerification, Verified, Rejected |
| PaymentProofUrl         | VARCHAR(500)  | Uploaded QR screenshot                            |
| PaymentMethod           | VARCHAR(20)   | eSewa, Khalti, Bank Transfer, Cash                |
| PaymentVerifiedByUserId | CHAR(36) NULL | Receptionist/Admin who verified                   |
| PaymentVerifiedAtUtc    | DATETIME NULL | Verification time                                 |



**Module: Payment** 

### 15. **Table: RefundRequests** (why:-Stores refund requests made by patients and tracks the approval process according to the clinic's cancellation policy.)

*CREATE TABLE RefundRequests*
1. Id	CHAR(36)	Unique Primary Key.
2. AppointmentsId	CHAR(36)	 which appointments of this refund requested.
3. RequestedAmount	DECIMAL(18,2)	Amount requested by the patient.
4. ApprovedAmount	DECIMAL(18,2)	Amount approved after applying clinic policy.
5. Reason	VARCHAR(300)	Reason for requesting the refund.
6. Status	TINYINT	(0->Pending, 1->Approved, 2->Rejected, 3->Processed.)
7. RequestedByUserId	CHAR(36)	User who submitted the request.
8. ApprovedByUserId	CHAR(36)	Receptionist/Admin who approved or rejected it.
9. RequestedAtUtc	DATETIME	Date and time the refund request was created.
10. ProcessedAtUtc	DATETIME	Date and time the refund was completed.







### . **Table: Payments**  (why:-Stores every payment related to appointments, penalties, or future services. It records payment details regardless of the payment gateway.)

*CREATE TABLE Payments*
1. Id	CHAR(36)	Unique Primary Key.
2. AppointmentId	CHAR(36)	Appointment associated with this payment.
3. ClinicId	CHAR(36)	Clinic receiving the payment.
4. PaymentPurpose	VARCHAR(50)	Appointment, Penalty, Subscription, etc.
5. Amount	DECIMAL(18,2)	Amount paid.
6. Currency	CHAR(3)	Currency code (NPR, INR, GBP). (Optional if SmartCare only supports Nepal initially.)
7. Method	VARCHAR(30)	Cash, eSewa, Khalti, FonePay, Card, etc.
8. Gateway	VARCHAR(50)	Payment gateway used.
9. Status	VARCHAR(20)	Pending, Success, Failed, Refunded.
10. TransactionReference	VARCHAR(150)	Transaction ID returned by the gateway.
11. GatewayReference	VARCHAR(150)	Gateway-specific reference ID.
12. IdempotencyKey	VARCHAR(150)	Prevents duplicate payment processing.
13. FailureReason	VARCHAR(300)	Reason if payment failed.
14. PaidAtUtc	DATETIME	Payment completion time.
15. CreatedAtUtc	DATETIME	Payment record creation time.


### this is for future while doctor page is make (now for in doctorschedules table has option is active so manually do for that by receptionasist). **Table: DoctorLeaves** (why:-Stores doctor's leave requests so unavailable dates are automatically excluded from booking.)

*CREATE TABLE DoctorLeaves*
1. Id	CHAR(36)	Unique Primary Key.
2. ClinicMembershipId	CHAR(36)	Doctor taking leave.
3. StartDate	DATE	Leave start date.
4. EndDate	DATE	Leave end date.
5. Reason	VARCHAR(250)	Reason for leave.
6. Status	VARCHAR(20)	Pending, Approved, Rejected.
7. ApprovedByUserId	CHAR(36)	Receptionist/Admin who approved the leave.
8. CreatedAtUtc	DATETIME	Date leave request was created.












## Step-1 (NuGet packages to install first)

 In Package Manager Console, or dotnet add package from each project's folder

 **Application**

`dotnet add src/Core/SmartCare.Application package MediatR/`

`dotnet add src/Core/SmartCare.Application package FluentValidation.DependencyInjectionExtensions/`

 **Infrastructure.Presistence**

`dotnet add src/Infrastructure/SmartCare.Infrastructure.Presistence package Microsoft.EntityFrameworkCore.Sqlite/`

`dotnet add src/Infrastructure/SmartCare.Infrastructure.Presistence package Microsoft.EntityFrameworkCore.Design/`

**Infrastructure.Identity**

`dotnet add src/Infrastructure/SmartCare.Infrastructure.Identity package Microsoft.AspNetCore.Authentication.JwtBearer/`

`dotnet add src/Infrastructure/SmartCare.Infrastructure.Identity package BCrypt.Net-Next/`

**SmartCare.API**

`dotnet add src/SmartCare.API package Swashbuckle.AspNetCore   # usually already included/`

---

## step-2 (Build order — do it in this sequence)

1. SharedKernel          → base classes everything else needs
2. Domain                → User, Role (just Identity module for now)
3. Application           → RegisterUser, Login commands
4. Infrastructure.Presistence → DbContext (SQLite), migration
5. Infrastructure.Identity    → password hashing, JWT
6. SmartCare.API          → wire it up, test in Swagger
