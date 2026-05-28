namespace LMS.API.Models
{
    public class BookReturn
    {
        public int Id { get; set; }

        public int BookIssueId { get; set; }
        public BookIssue? BookIssue { get; set; }

        public DateTime ReturnDate { get; set; } = DateTime.Now;

        public int OverdueDays { get; set; }

        public decimal FineAmount { get; set; }

        public string? Remarks { get; set; }

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