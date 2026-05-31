using LMS.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate?.Date ?? new DateTime(DateTime.Now.Year, 1, 1);
            var to = toDate?.Date.AddDays(1).AddTicks(-1) ?? new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59);

            var totalMembers = await _context.Members
                .CountAsync(x => !x.IsDeleted);

            var activeMembers = await _context.Members
                .CountAsync(x => !x.IsDeleted && x.IsActive);

            var totalBooks = await _context.Books
                .CountAsync(x => !x.IsDeleted);

            var totalCopies = await _context.BookCopies
                .CountAsync(x => !x.IsDeleted);

            var availableCopies = await _context.BookCopies
                .CountAsync(x => !x.IsDeleted && x.Status == "Available");

            var issuedCopies = await _context.BookCopies
                .CountAsync(x => !x.IsDeleted && x.Status == "Issued");

            var overdueBooks = await _context.BookIssues
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.Status == "Issued" &&
                    x.DueDate.Date < DateTime.Now.Date);

            var totalPurchases = await _context.BookPurchases
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.PurchaseDate >= from &&
                    x.PurchaseDate <= to);

            var totalPurchaseCost = await _context.BookPurchases
                .Where(x =>
                    !x.IsDeleted &&
                    x.PurchaseDate >= from &&
                    x.PurchaseDate <= to)
                .SumAsync(x => (decimal?)x.TotalCost) ?? 0;

            return Ok(new
            {
                TotalMembers = totalMembers,
                ActiveMembers = activeMembers,
                TotalBooks = totalBooks,
                TotalCopies = totalCopies,
                AvailableCopies = availableCopies,
                IssuedCopies = issuedCopies,
                OverdueBooks = overdueBooks,
                TotalPurchases = totalPurchases,
                TotalPurchaseCost = totalPurchaseCost
            });
        }

        [HttpGet("available-books")]
        public async Task<IActionResult> GetAvailableBooks()
        {
            var result = await _context.BookCopies
                .Include(x => x.Book)
                .Include(x => x.Shelf)
                .Include(x => x.Rack)
                .Where(x => !x.IsDeleted && x.Status == "Available")
                .Select(x => new
                {
                    x.Id,
                    x.Barcode,
                    BookTitle = x.Book != null ? x.Book.Title : "",
                    Shelf = x.Shelf != null ? x.Shelf.ShelfName : "",
                    Rack = x.Rack != null ? x.Rack.RackName : "",
                    x.Status
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("issued-books")]
        public async Task<IActionResult> GetIssuedBooks()
        {
            var result = await _context.BookIssues
                .Include(x => x.Member)
                .Include(x => x.BookCopy)
                    .ThenInclude(x => x.Book)
                .Where(x => !x.IsDeleted && x.Status == "Issued")
                .Select(x => new
                {
                    IssueId = x.Id,
                    MemberName = x.Member != null ? x.Member.MemberName : "",
                    CardexNo = x.Member != null ? x.Member.CardexNo : "",
                    Barcode = x.BookCopy != null ? x.BookCopy.Barcode : "",
                    BookTitle = x.BookCopy != null && x.BookCopy.Book != null ? x.BookCopy.Book.Title : "",
                    x.IssueDate,
                    x.DueDate,
                    x.Status
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdueBooks()
        {
            var today = DateTime.Now.Date;

            var result = await _context.BookIssues
                .Include(x => x.Member)
                .Include(x => x.BookCopy)
                    .ThenInclude(x => x.Book)
                .Where(x =>
                    !x.IsDeleted &&
                    x.Status == "Issued" &&
                    x.DueDate.Date < today)
                .Select(x => new
                {
                    IssueId = x.Id,
                    MemberName = x.Member != null ? x.Member.MemberName : "",
                    PhoneNo = x.Member != null ? x.Member.PhoneNo : "",
                    CardexNo = x.Member != null ? x.Member.CardexNo : "",
                    Barcode = x.BookCopy != null ? x.BookCopy.Barcode : "",
                    BookTitle = x.BookCopy != null && x.BookCopy.Book != null ? x.BookCopy.Book.Title : "",
                    x.IssueDate,
                    x.DueDate,
                    OverdueDays = EF.Functions.DateDiffDay(x.DueDate, DateTime.Now)
                })
                .ToListAsync();

            return Ok(result);
        }
    }
}