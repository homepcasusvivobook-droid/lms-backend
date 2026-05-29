namespace LMS.API.DTOs
{
    public class AddBookPurchaseDto
    {
        public string InvoiceNo { get; set; } = string.Empty;
        public string? StoreName { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public decimal TotalCost { get; set; }
        public string? Remarks { get; set; }

        public string? CreatedBy { get; set; }
        public string? EditedBy { get; set; }
        public string PurchaseType { get; set; } = "Purchase";
        public string Currency { get; set; } = "AED";
        public decimal ConversionRate { get; set; } = 1;
        public decimal TotalCostAed { get; set; }
        public string? SponsorName { get; set; }

        public List<AddBookPurchaseDetailDto> Details { get; set; } = new();
    }

    public class AddBookPurchaseDetailDto
    {
        public string ISBN { get; set; } = string.Empty;
        public int ShelfId { get; set; }
        public int RackId { get; set; }
        public int NoOfCopies { get; set; }
        public decimal Cost { get; set; }
        public string? Remarks { get; set; }
    }
}