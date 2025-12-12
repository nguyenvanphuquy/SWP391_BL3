using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SWP391_BL3.Models.Entities;

namespace SWP391_BL3.Data;

public partial class FptBookingContext : DbContext
{
    public FptBookingContext()
    {
    }

    public FptBookingContext(DbContextOptions<FptBookingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Campus> Campuses { get; set; }

    public virtual DbSet<Facility> Facilities { get; set; }

    public virtual DbSet<FacilityType> FacilityTypes { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Slot> Slots { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server=localhost; database=FPT_BOOKING; uid=sa; pwd=1234567890; TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Booking__73951AEDD722307D");

            entity.ToTable("Booking");

            entity.Property(e => e.ApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.BookingCode).HasMaxLength(100);
            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            entity.HasOne(d => d.ApprovedByUser).WithMany(p => p.BookingApprovedByUsers)
                .HasForeignKey(d => d.ApprovedByUserId)
                .HasConstraintName("FK__Booking__Approve__4CA06362");

            entity.HasOne(d => d.Facility).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.FacilityId)
                .HasConstraintName("FK__Booking__Facilit__4AB81AF0");

            entity.HasOne(d => d.Slot).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.SlotId)
                .HasConstraintName("FK__Booking__SlotId__4BAC3F29");

            entity.HasOne(d => d.User).WithMany(p => p.BookingUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Booking__UserId__49C3F6B7");
        });

        modelBuilder.Entity<Campus>(entity =>
        {
            entity.HasKey(e => e.CampusId).HasName("PK__Campus__FD598DD67E954D1A");

            entity.ToTable("Campus");

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.CampusName).HasMaxLength(200);
            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Facility>(entity =>
        {
            entity.HasKey(e => e.FacilityId).HasName("PK__Facility__5FB08A74086108EA");

            entity.ToTable("Facility");

            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.FacilityCode).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            entity.HasOne(d => d.Campus).WithMany(p => p.Facilities)
                .HasForeignKey(d => d.CampusId)
                .HasConstraintName("FK__Facility__Campus__403A8C7D");

            entity.HasOne(d => d.Type).WithMany(p => p.Facilities)
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("FK__Facility__TypeId__412EB0B6");

            entity.HasMany(d => d.Slots).WithMany(p => p.Facilities)
                .UsingEntity<Dictionary<string, object>>(
                    "FacilitySlot",
                    r => r.HasOne<Slot>().WithMany()
                        .HasForeignKey("SlotId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Facility___SlotI__5070F446"),
                    l => l.HasOne<Facility>().WithMany()
                        .HasForeignKey("FacilityId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Facility___Facil__4F7CD00D"),
                    j =>
                    {
                        j.HasKey("FacilityId", "SlotId").HasName("PK__Facility__BF11AEDE769C41D0");
                        j.ToTable("Facility_Slot");
                    });
        });

        modelBuilder.Entity<FacilityType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__Facility__516F03B51A9AAE01");

            entity.ToTable("Facility_Type");

            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.TypeName).HasMaxLength(200);
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDD6968F6C93");

            entity.ToTable("Feedback");

            entity.Property(e => e.CreateAt).HasColumnType("datetime");

            entity.HasOne(d => d.Facility).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.FacilityId)
                .HasConstraintName("FK__Feedback__Facili__44FF419A");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Feedback__UserId__440B1D61");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E122DD9292E");

            entity.ToTable("Notification");

            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Booking).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__Notificat__Booki__5441852A");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__534D60F1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A8FDC21F9");

            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<Slot>(entity =>
        {
            entity.HasKey(e => e.SlotId).HasName("PK__Slot__0A124AAF53FA522C");

            entity.ToTable("Slot");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C991B8A68");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534204EF9A5").IsUnique();

            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(200)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Users__RoleId__398D8EEE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
