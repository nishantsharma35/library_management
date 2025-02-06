using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace library_management.Models;

public partial class dbConnect : DbContext
{
    public dbConnect()
    {
    }

    public dbConnect(DbContextOptions<dbConnect> options)
        : base(options)
    {
    }

    public virtual DbSet<Library> Libraries { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Role> tblRoles { get; set; }

    public virtual DbSet<TblPermission> TblPermissions { get; set; }

    public virtual DbSet<TblTab> TblTabs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=NISHANTSHARMA\\SQLEXPRESS;Database=library_management_main;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Library>(entity =>
        {
            entity.HasKey(e => e.LibraryId).HasName("PK__library__95E69ECE930D6FA6");

            entity.ToTable("library");

            entity.Property(e => e.LibraryId).HasColumnName("libraryID");
            entity.Property(e => e.Address).IsUnicode(false);
            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.City)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LibraryImagePath).IsUnicode(false);
            entity.Property(e => e.Libraryname)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("libraryname");
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__members__3213E83F58B3705C");

            entity.ToTable("members");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("address");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("city");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("gender");
            entity.Property(e => e.IsEmailVerified).HasDefaultValue(false);
            entity.Property(e => e.Joiningdate).HasColumnName("joiningdate");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Otp)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("OTP");
            entity.Property(e => e.Otpexpiry).HasColumnType("datetime");
            entity.Property(e => e.Password)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phoneno).HasColumnName("phoneno");
            entity.Property(e => e.Picture)
                .IsUnicode(false)
                .HasColumnName("picture");
            entity.Property(e => e.State)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("state");
            entity.Property(e => e.VerificationStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("VerificationStatus");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__tblroles__8AFACE1A3FD59A23");

            entity.ToTable("tblroles");

            entity.Property(e => e.RoleName)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblPermission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__tblPermi__EFA6FB2FAFFCD3CC");

            entity.ToTable("tblPermissions");

            entity.HasOne(d => d.Tab).WithMany(p => p.TblPermissions)
                .HasForeignKey(d => d.TabId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblPermis__TabId__2EDAF651");
        });

        modelBuilder.Entity<TblTab>(entity =>
        {
            entity.HasKey(e => e.TabId).HasName("PK__tblTabs__80E37C18E881AD13");

            entity.ToTable("tblTabs");

            entity.Property(e => e.IconPath).IsUnicode(false);
            entity.Property(e => e.TabName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TabUrl)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
