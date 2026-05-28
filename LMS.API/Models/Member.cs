using System.ComponentModel.DataAnnotations;

namespace LMS.API.Models
{
    public class Member
    {
        public int Id { get; set; }

        public string? MemberId { get; set; }

        [Required]
        public string CardexNo { get; set; } = string.Empty;

        [Required]
        public string MemberName { get; set; } = string.Empty;

        public string? PhoneNo { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? Parish { get; set; }

        public ICollection<MembershipRenewal>? MembershipRenewals { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public bool IsEdited { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        public DateTime? EditedDate { get; set; }
        public string? EditedBy { get; set; }

        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }

        public int? MemberTypeId { get; set; }

        public MemberType? MemberType { get; set; }
    }
}