public class MemberType
{
    public int Id { get; set; }

    public string MemberTypeName { get; set; } = string.Empty;

    public string Prefix { get; set; } = string.Empty;

    public int MaxBooksAllowed { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public string? CreatedBy { get; set; }
}