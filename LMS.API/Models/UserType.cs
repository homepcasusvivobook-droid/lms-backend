using System.ComponentModel.DataAnnotations;

namespace LMS.API.Models
{
    public class UserType
    {
        public int Id { get; set; }

        [Required]
        public string UserTypeName { get; set; } = string.Empty;

        public bool CanCreate { get; set; }

        public bool CanView { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }

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