namespace library_management.Models
{
    public class BookListViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string GenreName { get; set; }
        public int? PublicationYear { get; set; }
        public string BookImagePath { get; set; }
        public string Libraries { get; set; }
    }

}
