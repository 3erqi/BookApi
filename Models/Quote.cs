namespace BookApi.Models
{
    public class Quote
    {
        public int Id {get; set;}
        public required String  Text {get; set;}
        public required String Author {get; set;}
        public int UserId {get; set;}
        public User User {get; set;} = null!;
    }
}