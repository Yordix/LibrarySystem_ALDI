using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.DTOs.Loans
{
    public class BorrowBookDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid BookId { get; set; }
    }
}
