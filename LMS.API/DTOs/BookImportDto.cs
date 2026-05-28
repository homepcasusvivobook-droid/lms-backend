namespace LMS.API.DTOs
{
    public class BookImportDto
    {
        public string? BookName { get; set; }
        public string? ISBN { get; set; }

        public int CategoryId { get; set; }
        public int? AuthorId { get; set; }
        public int? PublisherId { get; set; }
        public int? LanguageId { get; set; }

        public string? CodePrefix { get; set; }
        public string? CodeNo { get; set; }

        public string? CreatedBy { get; set; }
    }
}