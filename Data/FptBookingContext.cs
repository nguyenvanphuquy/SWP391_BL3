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

    public virtual DbSet<Checkin> Checkins { get; set; }

    public virtual DbSet<Checkout> Checkouts { get; set; }

    public virtual DbSet<Facility> Facilities { get; set; }

    public virtual DbSet<FacilityType> FacilityTypes { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Slot> Slots { get; set; }

    public virtual DbSet<User> Users { get; set; }

    /*
     * Q21: Tại sao dùng DateOnly cho BookingDate?
     * A21: 
     * - DateOnly: Chỉ lưu ngày, không có time (phù hợp với booking date)
     * - DateTime: Có cả ngày và giờ (không cần thiết cho booking date)
     * - Ưu điểm: Rõ ràng hơn, không lo lắng về timezone, tiết kiệm storage
     * - Lưu ý: DateOnly chỉ có từ .NET 6+, cần SQL Server 2008+
     */
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        /*
         * Q22: Connection string có an toàn không?
         * A22: 
         * - Hiện tại: Hardcoded trong code (KHÔNG AN TOÀN cho production)
         * - Nên: Connection string được config trong appsettings.json (đã làm)
         * - Production: Dùng Azure Key Vault, AWS Secrets Manager, hoặc Environment Variables
         * - Lưu ý: Không commit connection string vào git
         */
        // Connection string được config trong Program.cs qua DI
        // Không cần config ở đây nữa
    }

    /*
     * Q23: Có index cho các cột thường query không?
     * A23: CHƯA có index tùy chỉnh.
     * - EF Core tự tạo index cho Primary Key và Foreign Key
     * - Nên thêm index cho:
     *   + Users.Email (đã có unique index)
     *   + Bookings.BookingDate, Status, FacilityId (query thường xuyên)
     *   + Bookings.UserId (query booking của user)
     * - Cách: modelBuilder.Entity<Booking>().HasIndex(b => b.BookingDate);
     */
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Booking__73951AED2924C189");

            entity.ToTable("Booking");
            
            // TODO: Thêm index để tối ưu query
            // entity.HasIndex(b => b.BookingDate);
            // entity.HasIndex(b => new { b.FacilityId, b.BookingDate, b.SlotId });
            // entity.HasIndex(b => b.Status);

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
            entity.HasKey(e => e.CampusId).HasName("PK__Campus__FD598DD631F89571");

            entity.ToTable("Campus");

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.CampusName).HasMaxLength(200);
            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Checkin>(entity =>
        {
            entity.HasKey(e => e.CheckinId).HasName("PK__Checkin__F3C85D7156A07C46");

            entity.ToTable("Checkin");

            entity.Property(e => e.Comment).HasMaxLength(255);
            entity.Property(e => e.CreateAt).HasColumnType("datetime");

            entity.HasOne(d => d.Booking).WithMany(p => p.Checkins)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__Checkin__Booking__619B8048");
        });

        modelBuilder.Entity<Checkout>(entity =>
        {
            entity.HasKey(e => e.CheckoutId).HasName("PK__Checkout__E07EF5FC622CC3E0");

            entity.ToTable("Checkout");

            entity.Property(e => e.Comment).HasMaxLength(255);
            entity.Property(e => e.CreateAt).HasColumnType("datetime");

            entity.HasOne(d => d.Booking).WithMany(p => p.Checkouts)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__Checkout__Bookin__6477ECF3");
        });

        modelBuilder.Entity<Facility>(entity =>
        {
            entity.HasKey(e => e.FacilityId).HasName("PK__Facility__5FB08A74AA39BE93");

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
                        j.HasKey("FacilityId", "SlotId").HasName("PK__Facility__BF11AEDE30B75E21");
                        j.ToTable("Facility_Slot");
                    });
        });

        modelBuilder.Entity<FacilityType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__Facility__516F03B5A1AE28FD");

            entity.ToTable("Facility_Type");

            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.TypeName).HasMaxLength(200);
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDD6B10C77C5");

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
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E12F7929105");

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
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1ABFEB239A");

            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<Slot>(entity =>
        {
            entity.HasKey(e => e.SlotId).HasName("PK__Slot__0A124AAFEFA59FE3");

            entity.ToTable("Slot");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C6B027781");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534B9794374").IsUnique();

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
