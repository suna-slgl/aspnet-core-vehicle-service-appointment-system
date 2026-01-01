using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VehicleServiceApp.Models;

namespace VehicleServiceApp.Data
{
    /// <summary>
    /// Application Database Context - Entity Framework Core DbContext with Identity
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for entities
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUser Configuration
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ProfileImagePath).HasMaxLength(500);
            });

            // Vehicle Configuration
            builder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LicensePlate).IsRequired().HasMaxLength(15);
                entity.Property(e => e.Brand).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Model).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Color).HasMaxLength(20);
                entity.Property(e => e.ImagePath).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(500);

                // Index for license plate uniqueness
                entity.HasIndex(e => e.LicensePlate).IsUnique();

                // Relationship with ApplicationUser
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Vehicles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ServiceType Configuration
            builder.Entity<ServiceType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.IconClass).HasMaxLength(50);
                entity.Property(e => e.ColorCode).HasMaxLength(20);
            });

            // Technician Configuration
            builder.Entity<Technician>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Specialization).HasMaxLength(100);
                entity.Property(e => e.ImagePath).HasMaxLength(500);
            });

            // Appointment Configuration
            builder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerNotes).HasMaxLength(500);
                entity.Property(e => e.TechnicianNotes).HasMaxLength(500);
                entity.Property(e => e.CancellationReason).HasMaxLength(500);

                // Relationship with ApplicationUser
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Appointments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationship with Vehicle
                entity.HasOne(e => e.Vehicle)
                    .WithMany(v => v.Appointments)
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationship with ServiceType
                entity.HasOne(e => e.ServiceType)
                    .WithMany(s => s.Appointments)
                    .HasForeignKey(e => e.ServiceTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationship with Technician (optional)
                entity.HasOne(e => e.Technician)
                    .WithMany(t => t.Appointments)
                    .HasForeignKey(e => e.TechnicianId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Index for date-time queries
                entity.HasIndex(e => e.AppointmentDate);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.AppointmentDate, e.AppointmentTime, e.TechnicianId });
            });

            // Seed Data
            SeedData(builder);
        }

        private void SeedData(ModelBuilder builder)
        {
            // Seed Service Types
            builder.Entity<ServiceType>().HasData(
                new ServiceType
                {
                    Id = 1,
                    Name = "Periyodik Bakım",
                    Description = "Motor yağı, filtre değişimi ve genel kontroller",
                    EstimatedDurationMinutes = 60,
                    Price = 1500.00m,
                    IconClass = "bi-gear-fill",
                    ColorCode = "#0d6efd",
                    SortOrder = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new ServiceType
                {
                    Id = 2,
                    Name = "Fren Bakımı",
                    Description = "Fren balatası, disk kontrolü ve değişimi",
                    EstimatedDurationMinutes = 90,
                    Price = 2500.00m,
                    IconClass = "bi-disc-fill",
                    ColorCode = "#dc3545",
                    SortOrder = 2,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new ServiceType
                {
                    Id = 3,
                    Name = "Lastik Değişimi",
                    Description = "4 lastik değişimi, balans ve rot ayarı",
                    EstimatedDurationMinutes = 45,
                    Price = 800.00m,
                    IconClass = "bi-vinyl-fill",
                    ColorCode = "#198754",
                    SortOrder = 3,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new ServiceType
                {
                    Id = 4,
                    Name = "Araç Muayenesi",
                    Description = "Resmi muayene öncesi hazırlık ve kontroller",
                    EstimatedDurationMinutes = 120,
                    Price = 500.00m,
                    IconClass = "bi-clipboard-check-fill",
                    ColorCode = "#ffc107",
                    SortOrder = 4,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new ServiceType
                {
                    Id = 5,
                    Name = "Klima Bakımı",
                    Description = "Klima gazı dolumu ve sistem kontrolü",
                    EstimatedDurationMinutes = 60,
                    Price = 1200.00m,
                    IconClass = "bi-snow",
                    ColorCode = "#0dcaf0",
                    SortOrder = 5,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new ServiceType
                {
                    Id = 6,
                    Name = "Motor Arıza Tespiti",
                    Description = "Arıza kodu okuma ve motor diagnostik",
                    EstimatedDurationMinutes = 45,
                    Price = 750.00m,
                    IconClass = "bi-exclamation-triangle-fill",
                    ColorCode = "#fd7e14",
                    SortOrder = 6,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new ServiceType
                {
                    Id = 7,
                    Name = "Detaylı Yıkama",
                    Description = "İç dış detaylı temizlik ve cilalama",
                    EstimatedDurationMinutes = 180,
                    Price = 1000.00m,
                    IconClass = "bi-droplet-fill",
                    ColorCode = "#6f42c1",
                    SortOrder = 7,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new ServiceType
                {
                    Id = 8,
                    Name = "Akü Değişimi",
                    Description = "Akü kontrolü ve değişimi",
                    EstimatedDurationMinutes = 30,
                    Price = 2000.00m,
                    IconClass = "bi-battery-charging",
                    ColorCode = "#20c997",
                    SortOrder = 8,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );

            // Seed Technicians
            builder.Entity<Technician>().HasData(
                new Technician
                {
                    Id = 1,
                    FirstName = "Ahmet",
                    LastName = "Yılmaz",
                    PhoneNumber = "0532 111 22 33",
                    Email = "ahmet.yilmaz@servis.com",
                    Specialization = "Motor ve Şanzıman",
                    ExperienceYears = 15,
                    WorkStartTime = new TimeSpan(8, 0, 0),
                    WorkEndTime = new TimeSpan(18, 0, 0),
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Technician
                {
                    Id = 2,
                    FirstName = "Mehmet",
                    LastName = "Kaya",
                    PhoneNumber = "0533 222 33 44",
                    Email = "mehmet.kaya@servis.com",
                    Specialization = "Elektrik ve Elektronik",
                    ExperienceYears = 10,
                    WorkStartTime = new TimeSpan(8, 0, 0),
                    WorkEndTime = new TimeSpan(18, 0, 0),
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Technician
                {
                    Id = 3,
                    FirstName = "Ali",
                    LastName = "Demir",
                    PhoneNumber = "0534 333 44 55",
                    Email = "ali.demir@servis.com",
                    Specialization = "Fren ve Süspansiyon",
                    ExperienceYears = 8,
                    WorkStartTime = new TimeSpan(9, 0, 0),
                    WorkEndTime = new TimeSpan(19, 0, 0),
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Technician
                {
                    Id = 4,
                    FirstName = "Mustafa",
                    LastName = "Şahin",
                    PhoneNumber = "0535 444 55 66",
                    Email = "mustafa.sahin@servis.com",
                    Specialization = "Klima ve Kalorifer",
                    ExperienceYears = 5,
                    WorkStartTime = new TimeSpan(8, 0, 0),
                    WorkEndTime = new TimeSpan(17, 0, 0),
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Technician
                {
                    Id = 5,
                    FirstName = "Emre",
                    LastName = "Çelik",
                    PhoneNumber = "0536 555 66 77",
                    Email = "emre.celik@servis.com",
                    Specialization = "Lastik ve Rot Balans",
                    ExperienceYears = 6,
                    WorkStartTime = new TimeSpan(8, 30, 0),
                    WorkEndTime = new TimeSpan(18, 30, 0),
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );
        }
    }
}
