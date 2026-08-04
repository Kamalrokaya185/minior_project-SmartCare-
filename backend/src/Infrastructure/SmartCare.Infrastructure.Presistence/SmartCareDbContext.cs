using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Appointments;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.Domain.Identity;
using SmartCare.Domain.Patients;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Infrastructure.Presistence;

public class SmartCareDbContext : DbContext
{
    public SmartCareDbContext(DbContextOptions<SmartCareDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<ClinicMembership> ClinicMemberships => Set<ClinicMembership>();
    public DbSet<PatientProfile> PatientProfiles => Set<PatientProfile>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<ClinicPolicy> ClinicPolicies => Set<ClinicPolicy>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<ScheduleSlot> ScheduleSlots => Set<ScheduleSlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentStatusHistoryEntry> AppointmentStatusHistory => Set<AppointmentStatusHistoryEntry>();
    public DbSet<RefundRequest> RefundRequests => Set<RefundRequest>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(SmartCareDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}