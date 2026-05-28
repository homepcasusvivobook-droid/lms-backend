namespace LMS.API.DTOs
{
    public class MembershipRenewalDto
    {
        public int MemberDbId { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }

        public string? Remarks { get; set; }

        public string? CreatedBy { get; set; }
    }
}