using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace library_management.Models;

public partial class Member
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Picture { get; set; }
    [NotMapped]
    public IFormFile? profilefile { get; set; }
    public string? Email { get; set; }

    public string? Gender { get; set; }

    public long? Phoneno { get; set; }

    public string? Address { get; set; }

    public string? State { get; set; }

    public string? City { get; set; }

    public DateOnly? Joiningdate { get; set; }

    public string? Password { get; set; }
    [NotMapped]
    public string? Confirmpassword { get; set; }
    public string? Otp { get; set; }

    public DateTime? Otpexpiry { get; set; }

    public bool IsEmailVerified { get; set; }

    public int RoleId { get; set; }

    public string? VerificationStatus { get; set; }
    // Foreign Key
    public int? LibraryId { get; set; }

    public bool? IsGoogleAccount { get; set; }

    // Navigation Property
    public virtual Library? Library { get; set; }
    

    public virtual ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();

    public virtual ICollection<Membership> Memberships { get; set; } = new List<Membership>();
}