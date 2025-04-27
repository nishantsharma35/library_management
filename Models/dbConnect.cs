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

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Borrow> Borrows { get; set; }

    public virtual DbSet<Fine> Fines { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Library> Libraries { get; set; }

    public virtual DbSet<LibraryBook> LibraryBooks { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Membership> Memberships { get; set; }

    public virtual DbSet<TblPermission> TblPermissions { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblTab> TblTabs { get; set; }

    public virtual DbSet<TblTransaction> TblTransactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=NISHANTSHARMA;Database=library_management_main;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Books__3DE0C227A9C2D6D7");

            entity.HasIndex(e => e.Isbn, "UQ__Books__447D36EA7297204E").IsUnique();

            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.Author).HasMaxLength(255);
            entity.Property(e => e.bookimagepath)
                .IsUnicode(false)
                .HasColumnName("bookimagepath");
            entity.Property(e => e.Edition)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("1st Edition");
            entity.Property(e => e.GenreId).HasColumnName("GenreID");
            entity.Property(e => e.Isbn)
                .HasMaxLength(50)
                .HasColumnName("ISBN");
            entity.Property(e => e.Language)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("English");
            entity.Property(e => e.Publisher).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Genre).WithMany(p => p.Books)
                .HasForeignKey(d => d.GenreId)
                .HasConstraintName("FK__Books__GenreID__09746778");
        });

        modelBuilder.Entity<Borrow>(entity =>
        {
            entity.HasKey(e => e.BorrowId).HasName("PK__Borrow__4295F83FB200794E");

            entity.ToTable("Borrow");

            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.IssueDate).HasColumnType("datetime");
            entity.Property(e => e.LibraryId).HasColumnName("libraryID");
            entity.Property(e => e.otp)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("otp");
            entity.Property(e => e.otpexpires)
                .HasColumnType("datetime")
                .HasColumnName("otpexpires");
            entity.Property(e => e.ReturnDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Book).WithMany(p => p.Borrows)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Borrow_Book");

            entity.HasOne(d => d.Library).WithMany(p => p.Borrows)
                .HasForeignKey(d => d.LibraryId)
                .HasConstraintName("FK_Borrow_Library");

            entity.HasOne(d => d.Member).WithMany(p => p.Borrows)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Borrow_Member");
        });

        modelBuilder.Entity<Fine>(entity =>
        {
            entity.HasKey(e => e.FineId).HasName("PK__Fine__9D4A9B2C4A6664FE");

            entity.ToTable("Fine");

            entity.HasIndex(e => e.BorrowId, "UQ__Fine__4295F83E69EFAAFF").IsUnique();

            entity.Property(e => e.FineAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.LibraryId).HasColumnName("libraryId");
            entity.Property(e => e.PaidAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Borrow).WithOne(p => p.Fine)
                .HasForeignKey<Fine>(d => d.BorrowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Fine_Borrow");

            entity.HasOne(d => d.Library).WithMany(p => p.Fines)
                .HasForeignKey(d => d.LibraryId)
                .HasConstraintName("FK_Fine_Library");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.GenreId).HasName("PK__Genres__0385055E9C44FD92");

            entity.HasIndex(e => e.GenreName, "UQ__Genres__BBE1C339541D4257").IsUnique();

            entity.Property(e => e.GenreId).HasColumnName("GenreID");
            entity.Property(e => e.GenreName).HasMaxLength(100);
        });

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
            entity.Property(e => e.ClosingTime).HasPrecision(0);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LibraryFineAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.LibraryImagePath).IsUnicode(false);
            entity.Property(e => e.Libraryname)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("libraryname");
            entity.Property(e => e.StartTime).HasPrecision(0);
            entity.Property(e => e.State)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<LibraryBook>(entity =>
        {
            entity.HasKey(e => e.LibraryBookId).HasName("PK__LibraryB__86922193F10061A4");

            entity.Property(e => e.LibraryId).HasColumnName("libraryID");

            entity.HasOne(d => d.Book).WithMany(p => p.LibraryBooks)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK__LibraryBo__BookI__6225902D");

            entity.HasOne(d => d.Library).WithMany(p => p.LibraryBooks)
                .HasForeignKey(d => d.LibraryId)
                .HasConstraintName("FK__LibraryBo__libra__6319B466");
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
            entity.Property(e => e.IsGoogleAccount).HasDefaultValue(false);
            entity.Property(e => e.Joiningdate).HasColumnName("joiningdate");
            entity.Property(e => e.LibraryId).HasColumnName("libraryID");
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
                .IsUnicode(false);

            entity.HasOne(d => d.Library).WithMany(p => p.Member)
                .HasForeignKey(d => d.LibraryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Member_Library");
        });

        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasKey(e => e.MembershipId).HasName("PK__Membersh__92A78679C1671559");

            entity.ToTable("Membership");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Library).WithMany(p => p.Memberships)
                .HasForeignKey(d => d.LibraryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Membershi__Libra__5224328E");

            entity.HasOne(d => d.Member).WithMany(p => p.Memberships)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Membershi__Membe__51300E55");
        });

        modelBuilder.Entity<TblPermission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__tblPermi__EFA6FB2FD1838DB9");

            entity.ToTable("tblPermissions");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PermissionType)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Tab).WithMany(p => p.TblPermissions)
                .HasForeignKey(d => d.TabId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblPermis__TabId__498EEC8D");
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__tblRoles__8AFACE1AD1F4FB79");

            entity.ToTable("tblRoles");

            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleName)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblTab>(entity =>
        {
            entity.HasKey(e => e.TabId).HasName("PK__tblTabs__80E37C1845059C53");

            entity.ToTable("tblTabs");

            entity.Property(e => e.IconPath).IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(1);
            entity.Property(e => e.TabName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TabUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__TblTrans__55433A6BEF8AD363");

            entity.ToTable("TblTransaction");

            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Reference)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Fine).WithMany(p => p.TblTransactions)
                .HasForeignKey(d => d.FineId)
                .HasConstraintName("FK__TblTransa__FineI__73DA2C14");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
