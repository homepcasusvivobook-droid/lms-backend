using System.ComponentModel.DataAnnotations;

namespace LMS.API.Models
{
    public class BookPurchase
    {
        public int Id { get; set; }

        [Required]
        public string InvoiceNo { get; set; } = string.Empty;

        public string? StoreName { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        public int TotalCopies { get; set; }

        public decimal TotalCost { get; set; }

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

        public ICollection<BookPurchaseDetail>? Details { get; set; }

        public string PurchaseType { get; set; } = "Purchase"; // Purchase / Sponsorship
        public string Currency { get; set; } = "AED";          // AED / INR / USD
        public decimal ConversionRate { get; set; } = 1;
        public decimal TotalCostAed { get; set; }
        public string? SponsorName { get; set; }
    }
}