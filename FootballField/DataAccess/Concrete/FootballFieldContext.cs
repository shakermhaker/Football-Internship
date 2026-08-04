using Core.Entities;
using Core.Utilities.IoC;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;



namespace FootballField.DataAccess.Concrete.EntityFramework;

public class FootballFieldContext : DbContext
{

    private readonly IHttpContextAccessor _httpContextAccessor;

    public FootballFieldContext(DbContextOptions<FootballFieldContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public FootballFieldContext()
    {
        _httpContextAccessor = ServiceTool.ServiceProvider.GetService<IHttpContextAccessor>();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=FootballFieldDB;Username=postgres;Password=omer123");
    }
    // --- DbSet Tanımlamaları ---
    public DbSet<Business> Businesses { get; set; }
    public DbSet<BusinessImage> BusinessImages { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Day> Days { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<FieldPriceSchedule> FieldPriceSchedules { get; set; }
    public DbSet<Entities.Concrete.FootballField> FootballFields { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<OperationClaim> OperationClaims { get; set; }
    public DbSet<Status> Statuses { get; set; }
    public DbSet<TimeSlot> TimeSlots { get; set; } // Sınıf adın TimeSlots çoğul olduğu için DbSet adı da TimeSlots oldu, istersen TimeSlot olarak düzeltebilirsin.
    public DbSet<Entities.Concrete.User> Users { get; set; }

    // Ara tablo (Join Table)
    public DbSet<UserOperationClaim> UserOperationClaims{ get; set; }
    public DbSet<TeamAvatar> TeamAvatars { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ayrı dosya oluşturmadan doğrudan Context içine kuralı yazmak (Tek kuralımız olduğu için en pratik yol. artık iki xd ama uğraşmicam ayırmakla)
        modelBuilder.Entity<FieldPriceSchedule>(entity =>
        {
            // İlgili 3 kolonu (Saha, Saat, Gün) birleştirip Unique (Benzersiz) yapıyoruz!
            entity.HasIndex(f => new { f.FootballFieldId, f.TimeSlotId, f.DayId })
                  .IsUnique()
                  .HasDatabaseName("IX_Unique_Field_Time_Day");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasIndex(r => new { r.ReservationDate, r.FieldPriceScheduleId })
                  .IsUnique()
                  .HasDatabaseName("IX_Unique_ReservationDate_ScheduleId");
        });
    }

    public override int SaveChanges()
    {
        AddAuditInfo();
        return base.SaveChanges();
    }

    // 🚀 2. Asenkron (Async) Kaydetme işlemini eziyoruz
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AddAuditInfo()
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();

        // Artık _httpContextAccessor null gelmeyecek!
        var userIdString = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        int? currentUserId = null;
        if (int.TryParse(userIdString, out int parsedId))
        {
            currentUserId = parsedId;
        }

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow; // PostgreSQL UTC ister
                entry.Entity.CreatedBy = currentUserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(p => p.CreatedAt).IsModified = false;
                entry.Property(p => p.CreatedBy).IsModified = false;

                entry.Entity.UpdatedAt = DateTime.UtcNow; // PostgreSQL UTC ister
                entry.Entity.UpdatedBy = currentUserId;
            }
        }
    }


}

