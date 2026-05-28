namespace LMS.API.Models
{
    public class BookPurchaseDetail
    {
        public int Id { get; set; }

        public int BookPurchaseId { get; set; }
        public BookPurchase? BookPurchase { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int ShelfId { get; set; }
        public Shelf? Shelf { get; set; }

        public int RackId { get; set; }
        public Rack? Rack { get; set; }

        public int NoOfCopies { get; set; }

        public decimal Cost { get; set; }

        public string? Remarks { get; set; }

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