using System;
using System.Collections.Generic;
using System.Text;
using Entities.Concrete;
using Microsoft.EntityFrameworkCore;

   

namespace FootballField.DataAccess.Concrete.EntityFramework;

public class FootballFieldContext : DbContext
{
    public FootballFieldContext(DbContextOptions<FootballFieldContext> options) : base(options)
    {
    }

    // 🎯 2. KRİTİK EKLEME: Boş Constructor (EF Core Migration araçları bazen arar, hata vermesin diye koyarız)
    public FootballFieldContext()
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=FootballFieldDB;Username=postgres;Password=emre0660");
    }
    // --- DbSet Tanımlamaları ---
    public DbSet<Business> Businesses { get; set; }
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




}

