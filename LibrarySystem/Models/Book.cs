namespace LibrarySystem.API.Models
{
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public int PublishedYear { get; set; }
        public bool IsAvailable { get; set; }
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
