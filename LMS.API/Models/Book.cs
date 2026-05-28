using System.ComponentModel.DataAnnotations;

namespace LMS.API.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? ISBN { get; set; }

        // CATEGORY
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // AUTHOR
        public int? AuthorId { get; set; }
        public Author? Author { get; set; }

        // PUBLISHER
        public int? PublisherId { get; set; }
        public Publisher? Publisher { get; set; }

        // LANGUAGE
        public int? LanguageId { get; set; }
        public Language? Language { get; set; }

        // BOOK COPIES
        public ICollection<BookCopy>? BookCopies { get; set; }

        // AUDIT FIELDS
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public bool IsEdited { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        public DateTime? EditedDate { get; set; }
        public string? EditedBy { get; set; }

        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }
        public string? BookPrefix { get; set; }

        public string? CodeNo { get; set; }

        public string? CustomBarcode { get; set; }
    }
}