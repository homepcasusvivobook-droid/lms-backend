namespace LMS.API.Models
{
    public class Currency
    {
        public int Id { get; set; }

        public string CurrencyCode { get; set; } = string.Empty; // AED, INR, USD

        public string CurrencyName { get; set; } = string.Empty; // UAE Dirham

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        public DateTime? EditedDate { get; set; }
        public string? EditedBy { get; set; }

        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }
    }
}