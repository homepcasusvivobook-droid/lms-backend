using System.ComponentModel.DataAnnotations;

namespace LMS.API.Models
{
    public class Shelf
    {
        public int Id { get; set; }

        [Required]
        public string ShelfCode { get; set; } = string.Empty;

        [Required]
        public string ShelfName { get; set; } = string.Empty;
        
        public ICollection<Rack>? Racks { get; set; }

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