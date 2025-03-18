using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace library_management.Models;


public partial class Book
{
    public int BookId { get; set; }

    public string? Title { get; set; }

    public string? Author { get; set; }

    public string? Isbn { get; set; }

    public string? Publisher { get; set; }

    public int? PublicationYear { get; set; }

    public int? GenreId { get; set; }
    public string? bookimagepath { get; set; }

    public string Edition { get; set; } = "1st Edition"; // Default Value
    public string Language { get; set; } = "English"; // Default Value
    [NotMapped]

    public IFormFile? bookImage { get; set; }
    [NotMapped]
    public int LibraryId { get; set; }  // Hidden field ke liye

    [NotMapped]
    public int Stock { get; set; }  // Stock input field ke liye


    public virtual ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();

    public virtual Genre? Genre { get; set; }


    public virtual ICollection<LibraryBook> LibraryBooks { get; set; } = new List<LibraryBook>();



    public sealed class BookCsvMap : ClassMap<Book>
    {
        public BookCsvMap()
        {
            Map(m => m.Title);
            Map(m => m.Author);
            Map(m => m.Isbn).Name("ISBN"); // Case-sensitive hai
            Map(m => m.Publisher);
            Map(m => m.PublicationYear);
            Map(m => m.GenreId);
            Map(m => m.Edition);
            Map(m => m.Language);
            Map(m => m.bookimagepath);
        }
    }

}
