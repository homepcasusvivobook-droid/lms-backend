using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.API.Models
{
    public class MembershipRenewal
    {
        public int Id { get; set; }

        public int MemberDbId { get; set; }

        [ForeignKey("MemberDbId")]
        public Member? Member { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }

        public string? Remarks { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }

        public string? EditedBy { get; set; }
        public DateTime? EditedDate { get; set; }
        public bool IsEdited { get; set; }
    }
}