namespace BookApi.Models
{
    public class Book
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public string PublishedDate { get; set; } = string.Empty;
        public string? Genre { get; set; }
        public string? Isbn { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}