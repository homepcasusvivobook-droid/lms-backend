using LMS.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("barcode/{barcode}")]
        public async Task<IActionResult> SearchByBarcode(string barcode)
        {
            var result = await _context.BookCopies
                .Include(x => x.Book)
                    .ThenInclude(b => b!.Category)
                .Include(x => x.Book)
                    .ThenInclude(b => b!.Author)
                .Include(x => x.Book)
                    .ThenInclude(b => b!.Publisher)
                .Include(x => x.Book)
                    .ThenInclude(b => b!.Language)
                .Include(x => x.Shelf)
                .Include(x => x.Rack)
                .Where(x => x.Barcode == barcode && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    x.Barcode,
                    x.Status,
                    BookTitle = x.Book != null ? x.Book.Title : "",
                    Author = x.Book != null && x.Book.Author != null ? x.Book.Author.AuthorName : "",
                    Publisher = x.Book != null && x.Book.Publisher != null ? x.Book.Publisher.PublisherName : "",
                    ISBN = x.Book != null ? x.Book.ISBN : "",
                    Language = x.Book != null && x.Book.Language != null ? x.Book.Language.LanguageName : "",
                    Category = x.Book != null && x.Book.Category != null ? x.Book.Category.CategoryName : "",
                    CategoryCode = x.Book != null && x.Book.Category != null ? x.Book.Category.CategoryCode : "",
                    Shelf = x.Shelf != null ? x.Shelf.ShelfName : "",
                    ShelfCode = x.Shelf != null ? x.Shelf.ShelfCode : "",
                    Rack = x.Rack != null ? x.Rack.RackName : "",
                    RackCode = x.Rack != null ? x.Rack.RackCode : ""
                })
                .FirstOrDefaultAsync();

            if (result == null)
                return NotFound("Book copy not found for this barcode.");

            return Ok(result);
        }

        [HttpGet("books")]
        public async Task<IActionResult> SearchBooks(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Search keyword is required.");

            var result = await _context.BookCopies
                .Include(x => x.Book)
                    .ThenInclude(b => b!.Category)
                .Include(x => x.Book)
                    .ThenInclude(b => b!.Author)
                .Include(x => x.Book)
                    .ThenInclude(b => b!.Publisher)
                .Include(x => x.Book)
                    .ThenInclude(b => b!.Language)
                .Include(x => x.Shelf)
                .Include(x => x.Rack)
                .Where(x =>
                    !x.IsDeleted &&
                    x.Book != null &&
                    (
                        x.Book.Title.Contains(keyword) ||
                        (x.Book.ISBN != null && x.Book.ISBN.Contains(keyword)) ||
                        (x.Book.Author != null && x.Book.Author.AuthorName.Contains(keyword)) ||
                        (x.Book.Publisher != null && x.Book.Publisher.PublisherName.Contains(keyword)) ||
                        (x.Book.Language != null && x.Book.Language.LanguageName.Contains(keyword)) ||
                        (x.Book.Category != null && x.Book.Category.CategoryName.Contains(keyword)) ||
                        (x.Book.Category != null && x.Book.Category.CategoryCode.Contains(keyword))
                    )
                )
                .Select(x => new
                {
                    x.Id,
                    x.Barcode,
                    x.Status,
                    BookTitle = x.Book != null ? x.Book.Title : "",
                    Author = x.Book != null && x.Book.Author != null ? x.Book.Author.AuthorName : "",
                    Publisher = x.Book != null && x.Book.Publisher != null ? x.Book.Publisher.PublisherName : "",
                    ISBN = x.Book != null ? x.Book.ISBN : "",
                    Language = x.Book != null && x.Book.Language != null ? x.Book.Language.LanguageName : "",
                    Category = x.Book != null && x.Book.Category != null ? x.Book.Category.CategoryName : "",
                    CategoryCode = x.Book != null && x.Book.Category != null ? x.Book.Category.CategoryCode : "",
                    Shelf = x.Shelf != null ? x.Shelf.ShelfName : "",
                    ShelfCode = x.Shelf != null ? x.Shelf.ShelfCode : "",
                    Rack = x.Rack != null ? x.Rack.RackName : "",
                    RackCode = x.Rack != null ? x.Rack.RackCode : ""
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("members")]
        public async Task<IActionResult> SearchMembers(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Search keyword is required.");

            var result = await _context.Members
                .Where(x =>
                    !x.IsDeleted &&
                    (
                        x.CardexNo.Contains(keyword) ||
                        x.MemberId.Contains(keyword) ||
                        x.MemberName.Contains(keyword) ||
                        x.PhoneNo.Contains(keyword) ||
                        x.Email.Contains(keyword) ||
                        x.Parish.Contains(keyword)
                    )
                )
                .Select(x => new
                {
                    x.Id,
                    x.MemberId,
                    x.CardexNo,
                    x.MemberName,
                    x.PhoneNo,
                    x.Email,
                    x.Address,
                    x.Parish,
                    x.IsActive
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("stock")]
        public async Task<IActionResult> GetBookStock()
        {
            var result = await _context.Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.Category)
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Language)
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    Author = b.Author != null ? b.Author.AuthorName : "",
                    Publisher = b.Publisher != null ? b.Publisher.PublisherName : "",
                    b.ISBN,
                    Language = b.Language != null ? b.Language.LanguageName : "",

                    Category = b.Category != null ? b.Category.CategoryName : "",
                    CategoryCode = b.Category != null ? b.Category.CategoryCode : "",

                    TotalCopies = _context.BookCopies
                        .Count(c => c.BookId == b.Id && !c.IsDeleted),

                    AvailableCopies = _context.BookCopies
                        .Count(c => c.BookId == b.Id && !c.IsDeleted && c.Status == "Available"),

                    IssuedCopies = _context.BookCopies
                        .Count(c => c.BookId == b.Id && !c.IsDeleted && c.Status == "Issued"),

                    Locations = _context.BookCopies
                        .Where(c => c.BookId == b.Id && !c.IsDeleted)
                        .Include(c => c.Shelf)
                        .Include(c => c.Rack)
                        .GroupBy(c => new
                        {
                            ShelfCode = c.Shelf != null ? c.Shelf.ShelfCode : "",
                            ShelfName = c.Shelf != null ? c.Shelf.ShelfName : "",
                            RackCode = c.Rack != null ? c.Rack.RackCode : "",
                            RackName = c.Rack != null ? c.Rack.RackName : ""
                        })
                        .Select(g => new
                        {
                            g.Key.ShelfCode,
                            g.Key.ShelfName,
                            g.Key.RackCode,
                            g.Key.RackName,
                            TotalCopies = g.Count(),
                            AvailableCopies = g.Count(x => x.Status == "Available"),
                            IssuedCopies = g.Count(x => x.Status == "Issued")
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(result);
        }
    }
}