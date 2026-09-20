namespace LibrarySystem.API.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime RegisteredDate { get; set; }
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
