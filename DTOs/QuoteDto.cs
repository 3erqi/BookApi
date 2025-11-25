namespace BookApi.DTOs
{
    public class CreateQuoteDto
    {
        public required string Text { get; set; }
        public required string Author { get; set; }
    }

    public class UpdateQuoteDto
    {
        public required string Text { get; set; }
        public required string Author { get; set; }
    }

    public class QuoteResponseDto
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public required string Author { get; set; }
        public int UserId { get; set; }
    }
}
