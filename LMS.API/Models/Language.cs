using System.ComponentModel.DataAnnotations;

namespace LMS.API.Models
{
    public class Language
    {
        public int Id { get; set; }

        [Required]
        public string LanguageName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        public DateTime? EditedDate { get; set; }
        public string? EditedBy { get; set; }

        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }
        public string? LanguagePrefix { get; set; }
    }
}