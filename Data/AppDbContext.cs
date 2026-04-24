using Microsoft.EntityFrameworkCore;
using UpgradePortal.Web.Models;

namespace UpgradePortal.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<UpgradeSchedule> UpgradeSchedules => Set<UpgradeSchedule>();
    public DbSet<ShellRequest> ShellRequests => Set<ShellRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToTable("users").HasKey(x => x.UserId);
        modelBuilder.Entity<Role>().ToTable("roles").HasKey(x => x.RoleId);
        modelBuilder.Entity<Customer>().ToTable("customers").HasKey(x => x.CustomerId);
        modelBuilder.Entity<UpgradeSchedule>().ToTable("upgrade_schedules").HasKey(x => x.ScheduleId);
        modelBuilder.Entity<ShellRequest>().ToTable("shell_requests").HasKey(x => x.ShellRequestId);

        modelBuilder.Entity<User>().Property(x => x.UserId).HasColumnName("user_id");
        modelBuilder.Entity<User>().Property(x => x.RoleId).HasColumnName("role_id");
        modelBuilder.Entity<User>().Property(x => x.FullName).HasColumnName("full_name");
        modelBuilder.Entity<User>().Property(x => x.Email).HasColumnName("email");
        modelBuilder.Entity<User>().Property(x => x.PasswordHash).HasColumnName("password_hash");
        modelBuilder.Entity<User>().Property(x => x.TwoFactorEnabled).HasColumnName("two_factor_enabled");
        modelBuilder.Entity<User>().Property(x => x.TwoFactorEmail).HasColumnName("two_factor_email");
        modelBuilder.Entity<User>().Property(x => x.CreatedDate).HasColumnName("created_date");
        modelBuilder.Entity<User>().Property(x => x.IsActive).HasColumnName("is_active");

        modelBuilder.Entity<Role>().Property(x => x.RoleId).HasColumnName("role_id");
        modelBuilder.Entity<Role>().Property(x => x.RoleName).HasColumnName("role_name");
        modelBuilder.Entity<Role>().Property(x => x.Description).HasColumnName("description");

        modelBuilder.Entity<Customer>().Property(x => x.CustomerId).HasColumnName("customer_id");
        modelBuilder.Entity<Customer>().Property(x => x.CustomerCode).HasColumnName("customer_code");
        modelBuilder.Entity<Customer>().Property(x => x.CustomerName).HasColumnName("customer_name");
        modelBuilder.Entity<Customer>().Property(x => x.PrimaryEmail).HasColumnName("primary_email");
        modelBuilder.Entity<Customer>().Property(x => x.RegionCode).HasColumnName("region_code");

        modelBuilder.Entity<UpgradeSchedule>().Property(x => x.ScheduleId).HasColumnName("schedule_id");
        modelBuilder.Entity<UpgradeSchedule>().Property(x => x.CustomerId).HasColumnName("customer_id");
        modelBuilder.Entity<UpgradeSchedule>().Property(x => x.HostingType).HasColumnName("hosting_type");
        modelBuilder.Entity<UpgradeSchedule>().Property(x => x.CurrentVersion).HasColumnName("current_version");
        modelBuilder.Entity<UpgradeSchedule>().Property(x => x.TargetVersion).HasColumnName("target_version");
        modelBuilder.Entity<UpgradeSchedule>().Property(x => x.ScheduleDate).HasColumnName("schedule_date");
        modelBuilder.Entity<UpgradeSchedule>().Property(x => x.ScheduleTime).HasColumnName("schedule_time");
        modelBuilder.Entity<UpgradeSchedule>().Property(x => x.TicketNumber).HasColumnName("ticket_number");
        modelBuilder.Entity<UpgradeSchedule>().Property(x => x.Notes).HasColumnName("notes");
        modelBuilder.Entity<UpgradeSchedule>().Property(x => x.Status).HasColumnName("status");

        modelBuilder.Entity<ShellRequest>().Property(x => x.ShellRequestId).HasColumnName("shell_request_id");
        modelBuilder.Entity<ShellRequest>().Property(x => x.CustomerId).HasColumnName("customer_id");
        modelBuilder.Entity<ShellRequest>().Property(x => x.ClinicName).HasColumnName("clinic_name");
        modelBuilder.Entity<ShellRequest>().Property(x => x.Email).HasColumnName("email");
        modelBuilder.Entity<ShellRequest>().Property(x => x.Address).HasColumnName("address");
        modelBuilder.Entity<ShellRequest>().Property(x => x.Phone).HasColumnName("phone");
        modelBuilder.Entity<ShellRequest>().Property(x => x.EmrId).HasColumnName("emr_id");
        modelBuilder.Entity<ShellRequest>().Property(x => x.TokenId).HasColumnName("token_id");
        modelBuilder.Entity<ShellRequest>().Property(x => x.BaseContainer).HasColumnName("base_container");
        modelBuilder.Entity<ShellRequest>().Property(x => x.ProfileVersion).HasColumnName("profile_version");
        modelBuilder.Entity<ShellRequest>().Property(x => x.NumProviders).HasColumnName("num_providers");
        modelBuilder.Entity<ShellRequest>().Property(x => x.NumIHServers).HasColumnName("num_ih_servers");
        modelBuilder.Entity<ShellRequest>().Property(x => x.Region).HasColumnName("region");
        modelBuilder.Entity<ShellRequest>().Property(x => x.ExpectedDate).HasColumnName("expected_date");
        modelBuilder.Entity<ShellRequest>().Property(x => x.ClientRegistry).HasColumnName("client_registry");
        modelBuilder.Entity<ShellRequest>().Property(x => x.Integrations).HasColumnName("integrations");
        modelBuilder.Entity<ShellRequest>().Property(x => x.Attachments).HasColumnName("attachments");
        modelBuilder.Entity<ShellRequest>().Property(x => x.Status).HasColumnName("status");
        modelBuilder.Entity<ShellRequest>().Property(x => x.Notes).HasColumnName("notes");

        modelBuilder.Entity<UpgradeSchedule>()
            .HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId);
    }
}