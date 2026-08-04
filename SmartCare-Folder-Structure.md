# SmartCare — Solution Folder & File Structure

**Maps directly to:** SmartCare-Architecture.md (Clean Architecture layers, DDD bounded contexts), SmartCare-Security-Architecture.md, SmartCare-Database-Design.md (29 tables → entities/configurations)
**Convention:** one folder per bounded context inside each layer (vertical-slice style), one file per class, one `IEntityTypeConfiguration<T>` per entity, one Command/Query per folder with its Handler + Validator alongside it.

---

## Full Solution Tree

```
SmartCare.sln
│
├── src/
│   │
│   ├── Core/
│   │   │
│   │   ├── SmartCare.Domain/                              # No external dependencies. Pure C#.
│   │   │   ├── Common/
│   │   │   │   ├── AggregateRoot.cs
│   │   │   │   ├── Entity.cs
│   │   │   │   ├── ValueObject.cs
│   │   │   │   ├── DomainEvent.cs
│   │   │   │   ├── IDomainEventDispatcher.cs
│   │   │   │   ├── ISoftDeletable.cs
│   │   │   │   └── Specification.cs                       # Base class for the Specification pattern
│   │   │   │
│   │   │   ├── Exceptions/
│   │   │   │   ├── DomainException.cs
│   │   │   │   ├── InvalidAppointmentTransitionException.cs
│   │   │   │   ├── SlotAlreadyBookedException.cs
│   │   │   │   └── InvariantViolationException.cs
│   │   │   │
│   │   │   ├── Identity/                                   # Bounded Context: Identity & Access
│   │   │   │   ├── User.cs                                 # Aggregate Root
│   │   │   │   ├── Role.cs
│   │   │   │   ├── Permission.cs
│   │   │   │   ├── UserRole.cs
│   │   │   │   ├── RefreshToken.cs                         # Entity (child of User)
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   ├── Email.cs
│   │   │   │   │   ├── PasswordHash.cs
│   │   │   │   │   └── PermissionCode.cs
│   │   │   │   ├── Events/
│   │   │   │   │   ├── UserRegisteredEvent.cs
│   │   │   │   │   ├── UserEmailVerifiedEvent.cs
│   │   │   │   │   └── RefreshTokenFamilyRevokedEvent.cs
│   │   │   │   └── Repositories/
│   │   │   │       └── IUserRepository.cs
│   │   │   │
│   │   │   ├── Tenancy/                                    # Bounded Context: Tenancy & Subscription
│   │   │   │   ├── Hospital.cs                             # Aggregate Root
│   │   │   │   ├── HospitalMembership.cs                   # Aggregate Root (own lifecycle: join/leave)
│   │   │   │   ├── Subscription.cs
│   │   │   │   ├── SubscriptionPlan.cs
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   ├── Address.cs
│   │   │   │   │   ├── ContactInfo.cs
│   │   │   │   │   ├── HospitalStatus.cs                   # Smart enum: Pending/Active/Suspended
│   │   │   │   │   └── MembershipType.cs                   # Smart enum: Doctor/Receptionist
│   │   │   │   ├── Events/
│   │   │   │   │   ├── HospitalApprovedEvent.cs
│   │   │   │   │   ├── HospitalSuspendedEvent.cs
│   │   │   │   │   └── SubscriptionExpiredEvent.cs
│   │   │   │   └── Repositories/
│   │   │   │       ├── IHospitalRepository.cs
│   │   │   │       └── IHospitalMembershipRepository.cs
│   │   │   │
│   │   │   ├── ClinicalDirectory/                          # Bounded Context: Clinical Directory
│   │   │   │   ├── DoctorProfile.cs                        # Aggregate Root
│   │   │   │   ├── Department.cs                           # Aggregate Root
│   │   │   │   ├── DoctorSchedule.cs                        # Aggregate Root
│   │   │   │   ├── ScheduleSlot.cs                         # Entity (child of DoctorSchedule)
│   │   │   │   ├── DoctorLeave.cs                          # Aggregate Root
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   ├── TimeSlot.cs
│   │   │   │   │   ├── RecurrenceRule.cs
│   │   │   │   │   ├── LicenseNumber.cs
│   │   │   │   │   └── SlotStatus.cs                       # Available/Reserved/Booked/Blocked
│   │   │   │   ├── DomainServices/
│   │   │   │   │   └── SlotAvailabilityService.cs          # Spans DoctorSchedule + Appointment
│   │   │   │   ├── Specifications/
│   │   │   │   │   └── DoctorAvailableOnDateSpec.cs
│   │   │   │   ├── Events/
│   │   │   │   │   ├── DoctorScheduleUpdatedEvent.cs
│   │   │   │   │   └── DoctorLeaveApprovedEvent.cs
│   │   │   │   └── Repositories/
│   │   │   │       ├── IDoctorProfileRepository.cs
│   │   │   │       ├── IDepartmentRepository.cs
│   │   │   │       └── IDoctorScheduleRepository.cs
│   │   │   │
│   │   │   ├── Patients/                                   # Bounded Context: Patient Management
│   │   │   │   ├── PatientProfile.cs                       # Aggregate Root
│   │   │   │   ├── AttendanceScoreHistoryEntry.cs          # Entity (child)
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   ├── AttendanceScore.cs                  # Bounded 0-100, own transition rules
│   │   │   │   │   ├── EmergencyContact.cs
│   │   │   │   │   └── Gender.cs
│   │   │   │   ├── DomainServices/
│   │   │   │   │   └── AttendanceScoreCalculator.cs
│   │   │   │   ├── Events/
│   │   │   │   │   └── AttendanceScoreChangedEvent.cs
│   │   │   │   └── Repositories/
│   │   │   │       └── IPatientProfileRepository.cs
│   │   │   │
│   │   │   ├── HospitalPolicy/                             # Bounded Context: Hospital Policy
│   │   │   │   ├── HospitalPolicy.cs                       # Aggregate Root, versioned
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   └── CancellationPolicy.cs               # Window, RefundPercentage, PenaltyAmount
│   │   │   │   ├── Events/
│   │   │   │   │   └── HospitalPolicyChangedEvent.cs
│   │   │   │   └── Repositories/
│   │   │   │       └── IHospitalPolicyRepository.cs
│   │   │   │
│   │   │   ├── Appointments/                               # Bounded Context: Scheduling & Appointments
│   │   │   │   ├── Appointment.cs                          # Aggregate Root — the core state machine
│   │   │   │   ├── AppointmentStatusHistoryEntry.cs        # Entity (child)
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   ├── AppointmentStatus.cs                # Smart enum with legal-transition graph
│   │   │   │   │   └── Money.cs
│   │   │   │   ├── DomainServices/
│   │   │   │   │   └── RefundCalculationService.cs         # Spans Hospital + Appointment + Payment
│   │   │   │   ├── Specifications/
│   │   │   │   │   └── UpcomingAppointmentsForPatientSpec.cs
│   │   │   │   ├── Events/
│   │   │   │   │   ├── AppointmentBookedEvent.cs
│   │   │   │   │   ├── AppointmentConfirmedEvent.cs
│   │   │   │   │   ├── AppointmentCancelledEvent.cs
│   │   │   │   │   ├── AppointmentCompletedEvent.cs
│   │   │   │   │   ├── AppointmentNoShowEvent.cs
│   │   │   │   │   └── AppointmentExpiredEvent.cs
│   │   │   │   └── Repositories/
│   │   │   │       └── IAppointmentRepository.cs
│   │   │   │
│   │   │   ├── Payments/                                   # Bounded Context: Payments & Refunds
│   │   │   │   ├── Payment.cs                              # Aggregate Root
│   │   │   │   ├── Invoice.cs                              # Aggregate Root
│   │   │   │   ├── RefundRequest.cs                        # Aggregate Root
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   ├── PaymentStatus.cs
│   │   │   │   │   ├── PaymentMethod.cs
│   │   │   │   │   └── GatewayReference.cs
│   │   │   │   ├── Events/
│   │   │   │   │   ├── PaymentVerifiedEvent.cs
│   │   │   │   │   ├── PaymentFailedEvent.cs
│   │   │   │   │   └── RefundIssuedEvent.cs
│   │   │   │   └── Repositories/
│   │   │   │       ├── IPaymentRepository.cs
│   │   │   │       └── IRefundRequestRepository.cs
│   │   │   │
│   │   │   ├── Reviews/                                    # Bounded Context: Reputation
│   │   │   │   ├── Review.cs                               # Aggregate Root
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   ├── Rating.cs
│   │   │   │   │   └── RevieweeType.cs                     # Doctor/Hospital (extensible)
│   │   │   │   ├── Events/
│   │   │   │   │   └── ReviewSubmittedEvent.cs
│   │   │   │   └── Repositories/
│   │   │   │       └── IReviewRepository.cs
│   │   │   │
│   │   │   ├── Notifications/                              # Bounded Context: Notifications
│   │   │   │   ├── NotificationTemplate.cs                 # Aggregate Root
│   │   │   │   ├── Notification.cs                         # Aggregate Root
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   ├── NotificationChannel.cs              # Email/SMS/WhatsApp/Push
│   │   │   │   │   └── NotificationStatus.cs
│   │   │   │   └── Repositories/
│   │   │   │       └── INotificationRepository.cs
│   │   │   │
│   │   │   └── PlatformAdministration/                     # Bounded Context: Platform Administration
│   │   │       ├── AuditLogEntry.cs
│   │   │       ├── LoginHistoryEntry.cs
│   │   │       └── Repositories/
│   │   │           └── IAuditLogRepository.cs
│   │   │
│   │   └── SmartCare.Application/                          # Depends only on Domain + SharedKernel
│   │       ├── Common/
│   │       │   ├── Behaviors/
│   │       │   │   ├── ValidationBehavior.cs
│   │       │   │   ├── LoggingBehavior.cs
│   │       │   │   ├── TransactionBehavior.cs              # Wraps commands; collect-then-dispatch domain events
│   │       │   │   ├── AuthorizationBehavior.cs             # RBAC + Permission + Resource + Tenant checks
│   │       │   │   ├── AuditLoggingBehavior.cs
│   │       │   │   ├── CachingBehavior.cs
│   │       │   │   └── PerformanceBehavior.cs               # Logs slow handlers (>500ms)
│   │       │   ├── Interfaces/
│   │       │   │   ├── IApplicationDbContext.cs
│   │       │   │   ├── ITenantContext.cs
│   │       │   │   ├── ICurrentUserService.cs
│   │       │   │   ├── IDateTimeProvider.cs
│   │       │   │   ├── IEmailSender.cs
│   │       │   │   ├── ISmsSender.cs
│   │       │   │   ├── IPaymentGateway.cs
│   │       │   │   ├── IDistributedLockService.cs           # Redis Redlock abstraction
│   │       │   │   ├── ITokenGenerator.cs
│   │       │   │   ├── IPasswordHasher.cs
│   │       │   │   └── IFileStorageService.cs
│   │       │   ├── Models/
│   │       │   │   ├── Result.cs
│   │       │   │   ├── Result{T}.cs
│   │       │   │   ├── PagedList{T}.cs
│   │       │   │   ├── ApiResponse{T}.cs
│   │       │   │   └── QueryParameters.cs                  # filter/sort/search contract
│   │       │   ├── Mappings/
│   │       │   │   └── MappingProfile.cs                   # AutoMapper/Mapster profiles, one per module below
│   │       │   └── Extensions/
│   │       │       └── QueryableExtensions.cs               # ApplyFilter/ApplySort/ApplyPaging
│   │       │
│   │       ├── Identity/
│   │       │   ├── Commands/
│   │       │   │   ├── RegisterUser/
│   │       │   │   │   ├── RegisterUserCommand.cs
│   │       │   │   │   ├── RegisterUserCommandHandler.cs
│   │       │   │   │   └── RegisterUserCommandValidator.cs
│   │       │   │   ├── Login/
│   │       │   │   │   ├── LoginCommand.cs
│   │       │   │   │   ├── LoginCommandHandler.cs
│   │       │   │   │   └── LoginCommandValidator.cs
│   │       │   │   ├── RefreshToken/
│   │       │   │   ├── VerifyEmail/
│   │       │   │   ├── VerifyOtp/
│   │       │   │   ├── LogoutAllDevices/
│   │       │   │   ├── AssignRole/
│   │       │   │   └── ChangePassword/
│   │       │   ├── Queries/
│   │       │   │   ├── GetCurrentUser/
│   │       │   │   └── GetUserSessions/
│   │       │   └── EventHandlers/
│   │       │       └── UserRegisteredEventHandler.cs        # Triggers welcome/verification notification
│   │       │
│   │       ├── Tenancy/
│   │       │   ├── Commands/
│   │       │   │   ├── RegisterHospital/
│   │       │   │   ├── ApproveHospital/
│   │       │   │   ├── SuspendHospital/
│   │       │   │   ├── AddHospitalMembership/
│   │       │   │   ├── RemoveHospitalMembership/
│   │       │   │   └── ChangeSubscriptionPlan/
│   │       │   ├── Queries/
│   │       │   │   ├── GetHospitalById/
│   │       │   │   ├── SearchHospitals/
│   │       │   │   └── GetPlatformRevenueReport/            # Super Admin dashboard
│   │       │   └── EventHandlers/
│   │       │       └── HospitalApprovedEventHandler.cs
│   │       │
│   │       ├── ClinicalDirectory/
│   │       │   ├── Commands/
│   │       │   │   ├── CreateDepartment/
│   │       │   │   ├── AddDoctor/
│   │       │   │   ├── SetDoctorSchedule/
│   │       │   │   ├── RequestDoctorLeave/
│   │       │   │   ├── ApproveDoctorLeave/
│   │       │   │   └── GenerateScheduleSlots/               # Invoked by background job
│   │       │   ├── Queries/
│   │       │   │   ├── SearchDoctors/
│   │       │   │   ├── GetDoctorAvailability/
│   │       │   │   └── GetDoctorDaySchedule/
│   │       │   └── EventHandlers/
│   │       │       └── DoctorLeaveApprovedEventHandler.cs   # Cancels/blocks affected slots
│   │       │
│   │       ├── Patients/
│   │       │   ├── Commands/
│   │       │   │   ├── CreatePatientProfile/
│   │       │   │   └── UpdateEmergencyContact/
│   │       │   ├── Queries/
│   │       │   │   ├── GetPatientProfile/
│   │       │   │   └── GetAttendanceScoreHistory/
│   │       │   └── EventHandlers/
│   │       │       ├── AppointmentCompletedEventHandler.cs  # → raises score up
│   │       │       ├── AppointmentNoShowEventHandler.cs     # → lowers score
│   │       │       └── AppointmentCancelledEventHandler.cs  # → conditional score effect
│   │       │
│   │       ├── HospitalPolicy/
│   │       │   ├── Commands/
│   │       │   │   └── UpdateHospitalPolicy/                # Creates new version, closes prior
│   │       │   └── Queries/
│   │       │       └── GetCurrentHospitalPolicy/
│   │       │
│   │       ├── Appointments/
│   │       │   ├── Commands/
│   │       │   │   ├── BookAppointment/
│   │       │   │   │   ├── BookAppointmentCommand.cs
│   │       │   │   │   ├── BookAppointmentCommandHandler.cs # Uses IDistributedLockService + UoW
│   │       │   │   │   └── BookAppointmentCommandValidator.cs
│   │       │   │   ├── ConfirmAppointment/
│   │       │   │   ├── CheckInAppointment/
│   │       │   │   ├── StartConsultation/
│   │       │   │   ├── CompleteAppointment/
│   │       │   │   ├── CancelAppointment/
│   │       │   │   ├── RejectAppointment/
│   │       │   │   ├── MarkNoShow/
│   │       │   │   └── ExpirePendingAppointments/           # Invoked by background sweep job
│   │       │   ├── Queries/
│   │       │   │   ├── GetAppointmentById/
│   │       │   │   ├── GetAppointmentsByPatient/
│   │       │   │   ├── GetAppointmentsByDoctor/
│   │       │   │   ├── GetHospitalAppointmentDashboard/
│   │       │   │   └── GetAppointmentStatusHistory/
│   │       │   └── EventHandlers/
│   │       │       └── (cross-module handlers live in the consuming module, e.g. Notifications/Patients)
│   │       │
│   │       ├── Payments/
│   │       │   ├── Commands/
│   │       │   │   ├── InitiatePayment/
│   │       │   │   ├── VerifyPaymentWebhook/                # Signature-validated
│   │       │   │   ├── RequestRefund/
│   │       │   │   ├── ApproveRefund/
│   │       │   │   └── GenerateInvoice/
│   │       │   ├── Queries/
│   │       │   │   ├── GetPaymentByAppointment/
│   │       │   │   └── GetHospitalRevenueReport/
│   │       │   └── EventHandlers/
│   │       │       └── AppointmentCancelledEventHandler.cs  # → triggers RefundCalculationService
│   │       │
│   │       ├── Reviews/
│   │       │   ├── Commands/
│   │       │   │   └── SubmitReview/
│   │       │   ├── Queries/
│   │       │   │   ├── GetDoctorReviews/
│   │       │   │   └── GetHospitalReviews/
│   │       │   └── EventHandlers/
│   │       │       └── ReviewSubmittedEventHandler.cs       # Updates DoctorProfile.AverageRating cache
│   │       │
│   │       ├── Notifications/
│   │       │   ├── Commands/
│   │       │   │   ├── SendNotification/
│   │       │   │   └── RetryFailedNotifications/            # Invoked by background job
│   │       │   ├── Queries/
│   │       │   │   └── GetNotificationLog/
│   │       │   └── EventHandlers/                           # The decoupling point — subscribes across contexts
│   │       │       ├── AppointmentBookedEventHandler.cs
│   │       │       ├── AppointmentConfirmedEventHandler.cs
│   │       │       ├── AppointmentCancelledEventHandler.cs
│   │       │       ├── PaymentVerifiedEventHandler.cs
│   │       │       └── RefundIssuedEventHandler.cs
│   │       │
│   │       ├── Fraud/                                       # Bounded Context: Fraud (scores, never bans)
│   │       │   ├── Commands/
│   │       │   │   └── RecalculateFraudRiskScore/
│   │       │   ├── Queries/
│   │       │   │   └── GetFraudReviewQueue/                 # Super Admin
│   │       │   └── EventHandlers/
│   │       │       └── AppointmentCancelledEventHandler.cs  # Feeds fraud signal alongside Patients module
│   │       │
│   │       └── PlatformAdministration/
│   │           ├── Queries/
│   │           │   ├── GetAuditLogs/
│   │           │   └── GetLoginHistory/
│   │           └── DependencyInjection.cs                   # AddApplication() extension, registers MediatR/FluentValidation
│   │
│   ├── Infrastructure/
│   │   │
│   │   ├── SmartCare.Infrastructure.Persistence/
│   │   │   ├── SmartCareDbContext.cs                        # Implements IApplicationDbContext
│   │   │   ├── Configurations/                              # One IEntityTypeConfiguration<T> per table
│   │   │   │   ├── UserConfiguration.cs
│   │   │   │   ├── RoleConfiguration.cs
│   │   │   │   ├── UserRoleConfiguration.cs
│   │   │   │   ├── PermissionConfiguration.cs
│   │   │   │   ├── RolePermissionConfiguration.cs
│   │   │   │   ├── RefreshTokenConfiguration.cs
│   │   │   │   ├── UserSessionConfiguration.cs
│   │   │   │   ├── LoginHistoryConfiguration.cs
│   │   │   │   ├── AuditLogConfiguration.cs
│   │   │   │   ├── HospitalConfiguration.cs
│   │   │   │   ├── HospitalMembershipConfiguration.cs
│   │   │   │   ├── SubscriptionPlanConfiguration.cs
│   │   │   │   ├── SubscriptionConfiguration.cs
│   │   │   │   ├── DepartmentConfiguration.cs
│   │   │   │   ├── DoctorProfileConfiguration.cs
│   │   │   │   ├── DoctorScheduleConfiguration.cs
│   │   │   │   ├── ScheduleSlotConfiguration.cs
│   │   │   │   ├── DoctorLeaveConfiguration.cs
│   │   │   │   ├── PatientProfileConfiguration.cs
│   │   │   │   ├── AttendanceScoreHistoryConfiguration.cs
│   │   │   │   ├── HospitalPolicyConfiguration.cs
│   │   │   │   ├── AppointmentConfiguration.cs
│   │   │   │   ├── AppointmentStatusHistoryConfiguration.cs
│   │   │   │   ├── PaymentConfiguration.cs
│   │   │   │   ├── InvoiceConfiguration.cs
│   │   │   │   ├── RefundRequestConfiguration.cs
│   │   │   │   ├── ReviewConfiguration.cs
│   │   │   │   ├── NotificationTemplateConfiguration.cs
│   │   │   │   └── NotificationConfiguration.cs
│   │   │   ├── Migrations/                                  # EF Core generated
│   │   │   ├── Interceptors/
│   │   │   │   ├── AuditSaveChangesInterceptor.cs           # Writes AuditLogs on every SaveChanges
│   │   │   │   └── DomainEventDispatchInterceptor.cs        # Collect-then-dispatch post-commit
│   │   │   ├── Repositories/                                # One per Aggregate Root, matching Domain interfaces
│   │   │   │   ├── UserRepository.cs
│   │   │   │   ├── HospitalRepository.cs
│   │   │   │   ├── HospitalMembershipRepository.cs
│   │   │   │   ├── DoctorProfileRepository.cs
│   │   │   │   ├── DoctorScheduleRepository.cs
│   │   │   │   ├── PatientProfileRepository.cs
│   │   │   │   ├── AppointmentRepository.cs
│   │   │   │   ├── PaymentRepository.cs
│   │   │   │   ├── RefundRequestRepository.cs
│   │   │   │   ├── ReviewRepository.cs
│   │   │   │   └── NotificationRepository.cs
│   │   │   ├── UnitOfWork.cs
│   │   │   ├── TenantSessionInitializer.cs                  # Sets Postgres app.current_tenant per unit of work
│   │   │   └── DependencyInjection.cs                       # AddPersistence() extension
│   │   │
│   │   ├── SmartCare.Infrastructure.Identity/
│   │   │   ├── JwtTokenGenerator.cs                         # RS256, implements ITokenGenerator
│   │   │   ├── Argon2PasswordHasher.cs                      # Implements IPasswordHasher
│   │   │   ├── RefreshTokenService.cs                       # Rotation + reuse detection
│   │   │   ├── CurrentUserService.cs                        # Implements ICurrentUserService from JWT claims
│   │   │   ├── TenantContext.cs                              # Implements ITenantContext from JWT claims
│   │   │   ├── OtpService.cs
│   │   │   ├── BreachedPasswordChecker.cs                   # HIBP k-anonymity check
│   │   │   └── DependencyInjection.cs
│   │   │
│   │   ├── SmartCare.Infrastructure.Notifications/
│   │   │   ├── Channels/
│   │   │   │   ├── INotificationChannel.cs
│   │   │   │   ├── EmailChannel.cs                          # SendGrid/SES adapter
│   │   │   │   ├── SmsChannel.cs                             # Twilio/local gateway adapter
│   │   │   │   ├── PushChannel.cs                           # Future: FCM/APNs
│   │   │   │   └── WhatsAppChannel.cs                       # Future
│   │   │   ├── TemplateRenderer.cs                          # Resolves hospital override → platform default
│   │   │   └── DependencyInjection.cs
│   │   │
│   │   ├── SmartCare.Infrastructure.Payments/
│   │   │   ├── Gateways/
│   │   │   │   ├── EsewaGateway.cs                          # Implements IPaymentGateway
│   │   │   │   ├── KhaltiGateway.cs
│   │   │   │   └── WebhookSignatureValidator.cs
│   │   │   ├── RefundCalculationServiceImpl.cs              # Infra glue around the Domain service
│   │   │   └── DependencyInjection.cs
│   │   │
│   │   ├── SmartCare.Infrastructure.Caching/
│   │   │   ├── RedisCacheService.cs
│   │   │   ├── RedisDistributedLockService.cs               # Redlock, used by BookAppointment
│   │   │   └── DependencyInjection.cs
│   │   │
│   │   ├── SmartCare.Infrastructure.FileStorage/
│   │   │   ├── BlobFileStorageService.cs                    # Random GUID names, private bucket, signed URLs
│   │   │   ├── FileTypeValidator.cs                         # Magic-byte MIME validation
│   │   │   └── DependencyInjection.cs
│   │   │
│   │   └── SmartCare.Infrastructure.BackgroundJobs/
│   │       ├── Jobs/
│   │       │   ├── ScheduleSlotGenerationJob.cs             # Materializes ScheduleSlots from DoctorSchedules
│   │       │   ├── AppointmentExpirySweepJob.cs             # Pending → Expired
│   │       │   ├── SlotReservationReleaseJob.cs             # Releases short-hold Reserved slots
│   │       │   ├── NotificationDispatchJob.cs
│   │       │   ├── NotificationRetryJob.cs
│   │       │   ├── SubscriptionRenewalCheckJob.cs
│   │       │   ├── AttendanceScoreRecalculationJob.cs
│   │       │   ├── FraudRiskScoreRecalculationJob.cs
│   │       │   └── AuditLogArchivalJob.cs                   # Retention-policy driven
│   │       └── DependencyInjection.cs                       # Hangfire/Quartz registration
│   │
│   ├── Presentation/
│   │   └── SmartCare.WebApi/
│   │       ├── Controllers/
│   │       │   └── v1/
│   │       │       ├── AuthController.cs
│   │       │       ├── HospitalsController.cs
│   │       │       ├── DepartmentsController.cs
│   │       │       ├── DoctorsController.cs
│   │       │       ├── PatientsController.cs
│   │       │       ├── AppointmentsController.cs
│   │       │       ├── PaymentsController.cs
│   │       │       ├── RefundsController.cs
│   │       │       ├── ReviewsController.cs
│   │       │       ├── NotificationsController.cs
│   │       │       ├── SubscriptionsController.cs
│   │       │       └── SuperAdminController.cs
│   │       ├── Middleware/
│   │       │   ├── TenantResolutionMiddleware.cs
│   │       │   ├── ExceptionHandlingMiddleware.cs
│   │       │   ├── CorrelationIdMiddleware.cs
│   │       │   └── RequestLoggingMiddleware.cs
│   │       ├── Filters/
│   │       │   └── PermissionAuthorizeAttribute.cs           # [Authorize(Permission = "appointments.confirm")]
│   │       ├── Extensions/
│   │       │   ├── ServiceCollectionExtensions.cs
│   │       │   ├── ApiVersioningExtensions.cs
│   │       │   ├── SwaggerExtensions.cs                      # Disabled/gated outside dev+staging
│   │       │   ├── RateLimitingExtensions.cs
│   │       │   └── SecurityHeadersExtensions.cs
│   │       ├── HealthChecks/
│   │       │   ├── DatabaseHealthCheck.cs
│   │       │   └── RedisHealthCheck.cs
│   │       ├── appsettings.json
│   │       ├── appsettings.Development.json
│   │       ├── Program.cs
│   │       └── Dockerfile
│   │
│   └── SharedKernel/
│       └── SmartCare.SharedKernel/
│           ├── Guard.cs                                     # Guard clauses used across Domain
│           ├── ValueObjectBase.cs
│           └── Constants/
│               └── PermissionCodes.cs                       # e.g. "appointments.confirm", "refunds.approve"
│
├── tests/
│   ├── SmartCare.Domain.UnitTests/
│   │   ├── Appointments/
│   │   │   ├── AppointmentStateTransitionTests.cs           # e.g. Pending → Completed directly must throw
│   │   │   └── RefundCalculationServiceTests.cs
│   │   ├── ClinicalDirectory/
│   │   │   └── SlotAvailabilityServiceTests.cs
│   │   └── Patients/
│   │       └── AttendanceScoreCalculatorTests.cs
│   │
│   ├── SmartCare.Application.UnitTests/
│   │   ├── Appointments/
│   │   │   └── BookAppointmentCommandHandlerTests.cs
│   │   ├── Payments/
│   │   │   └── RequestRefundCommandHandlerTests.cs
│   │   └── Common/
│   │       └── ValidationBehaviorTests.cs
│   │
│   ├── SmartCare.Infrastructure.IntegrationTests/            # Real PostgreSQL via Testcontainers
│   │   ├── Persistence/
│   │   │   ├── AppointmentRepositoryTests.cs
│   │   │   ├── GlobalQueryFilterTests.cs                    # Confirms tenant filter actually applies
│   │   │   └── RowLevelSecurityTests.cs                     # Confirms RLS blocks cross-tenant rows
│   │   └── Payments/
│   │       └── WebhookSignatureValidatorTests.cs
│   │
│   └── SmartCare.WebApi.FunctionalTests/
│       ├── Security/
│       │   ├── IdorRegressionTests.cs                       # Patient A cannot fetch Patient B's appointment
│       │   ├── CrossTenantAccessTests.cs
│       │   ├── RateLimitingTests.cs
│       │   └── WebhookForgeryRejectionTests.cs
│       ├── Appointments/
│       │   ├── BookingConcurrencyTests.cs                   # Double-booking race-condition test
│       │   └── AppointmentLifecycleTests.cs                 # Full Pending→...→Completed happy path
│       └── Auth/
│           └── RefreshTokenReuseDetectionTests.cs
│
├── docs/
│   ├── SmartCare-Architecture.md
│   ├── SmartCare-Security-Architecture.md
│   ├── SmartCare-Database-Design.md
│   └── adr/                                                 # Architecture Decision Records, one per major choice
│       ├── 0001-shared-schema-multi-tenancy.md
│       ├── 0002-cqrs-lite-not-event-sourcing.md
│       └── 0003-uuid-v7-surrogate-keys.md
│
├── scripts/
│   ├── smartcare_postgres.sql
│   ├── smartcare_mysql.sql
│   └── seed/
│       ├── seed-roles-permissions.sql
│       └── seed-subscription-plans.sql
│
├── docker-compose.yml                                       # API + PostgreSQL + Redis + Seq + mail-catcher
├── .github/workflows/ci.yml                                 # Build → test → SAST → scan → image → deploy
├── .editorconfig
├── Directory.Build.props                                    # Shared project settings (nullable, analyzers)
└── README.md
```

---

## Why this shape, tied back to the architecture

- **One folder per bounded context, repeated identically across `Domain/`, `Application/`, and `Infrastructure.Persistence/Configurations`** — so "where does Appointment logic live" has one answer at every layer (`Domain/Appointments`, `Application/Appointments`, `AppointmentConfiguration.cs`), not a different organizing principle per layer.
- **Every Command/Query gets its own folder** with the Command, Handler, and Validator as siblings (`BookAppointment/BookAppointmentCommand.cs`, `...Handler.cs`, `...Validator.cs`) rather than three parallel `Commands/`, `Handlers/`, `Validators/` folders — this is what keeps a single use case's full logic reviewable as one unit, consistent with the CQRS-lite decision.
- **`EventHandlers/` folders live inside the *consuming* module, not the producing one** — e.g., `Application/Notifications/EventHandlers/AppointmentBookedEventHandler.cs`, not inside `Application/Appointments/`. This is the folder-level enforcement of the event-driven decoupling described in the base architecture: the Appointments module never contains a reference to Notifications.
- **Five separate `Infrastructure.*` projects**, not one — mirrors the base document's explicit call for small, swappable Infrastructure projects (a future SMS provider swap touches only `SmartCare.Infrastructure.Notifications`, never rebuilds `Persistence`).
- **`Configurations/` has exactly 29 files**, one per table in the database design — a new table in a future migration is a missing-file smell, easy to catch in review.
- **Test project split (`Domain.UnitTests` / `Application.UnitTests` / `Infrastructure.IntegrationTests` / `WebApi.FunctionalTests`)** matches the four test types from the security document that need genuinely different setups (no DB at all → real Testcontainers PostgreSQL → full HTTP pipeline), and `Security/` gets its own folder inside functional tests specifically so the IDOR/cross-tenant/webhook-forgery regression tests (Section 21 of the security doc) are never quietly skipped or lost among ordinary feature tests.
- **`docs/adr/`** — a place for the "why" behind decisions like shared-schema tenancy or CQRS-lite to live as short, dated records next to the code, rather than only in the long-form architecture documents, so future contributors can see *when* and *why* a decision was made without re-reading the whole document.
