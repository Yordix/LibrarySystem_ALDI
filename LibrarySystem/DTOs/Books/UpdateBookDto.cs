using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.DTOs.Books
{
    public class UpdateBookDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Author { get; set; } = string.Empty;
        [Required]
        public string ISBN { get; set; } = string.Empty;
        [Range(0, 2100)]
        public int PublishedYear { get; set; }
    }
}
