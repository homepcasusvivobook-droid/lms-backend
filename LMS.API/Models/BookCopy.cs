using System.ComponentModel.DataAnnotations;

namespace LMS.API.Models
{
    public class BookCopy
    {
        public int Id { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        public int ShelfId { get; set; }
        public Shelf? Shelf { get; set; }

        public int RackId { get; set; }
        public Rack? Rack { get; set; }

        public int SerialNo { get; set; }

        [Required]
        public string Barcode { get; set; } = string.Empty;

        public string Status { get; set; } = "Available";

        public int? BookPurchaseDetailId { get; set; }
        public BookPurchaseDetail? BookPurchaseDetail { get; set; }

        // audit fields
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public bool IsEdited { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        public DateTime? EditedDate { get; set; }
        public string? EditedBy { get; set; }

        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }
    }
}