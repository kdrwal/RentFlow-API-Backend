using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RentFlow.Models;

namespace RentFlow.Models.Contexts;

public partial class RentContext : DbContext
{
    public RentContext()
    {
    }

    public RentContext(DbContextOptions<RentContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bike> Bikes { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<ReservationBike> ReservationBikes { get; set; }

    public virtual DbSet<User> Users { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bike>(entity =>
        {
            entity.HasKey(e => e.BikeId).HasName("PK__Bikes__7DC817210FE493A9");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.ReservationId).HasName("PK__Reservat__B7EE5F249825C95F");

            entity.HasOne(d => d.User).WithMany(p => p.Reservations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Res_Users");
        });

        modelBuilder.Entity<ReservationBike>(entity =>
        {
            entity.HasKey(e => new { e.ReservationId, e.BikeId }).HasName("PK_ResBikes");

            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Bike).WithMany(p => p.ReservationBikes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ResBikes_Bike");

            entity.HasOne(d => d.Reservation).WithMany(p => p.ReservationBikes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ResBikes_Res");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C5A211BDE");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
