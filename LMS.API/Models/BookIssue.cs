using System.ComponentModel.DataAnnotations;

namespace LMS.API.Models
{
    public class BookIssue
    {
        public int Id { get; set; }

        public int MemberId { get; set; }
        public Member? Member { get; set; }

        public int BookCopyId { get; set; }
        public BookCopy? BookCopy { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.Now;

        public DateTime DueDate { get; set; }

        public string Status { get; set; } = "Issued";

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