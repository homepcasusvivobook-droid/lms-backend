using LMS.API.Data;
using LMS.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibraryTransactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LibraryTransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions()
        {
            var today = DateTime.Today;

            var data = await _context.BookIssues
                .Where(x => !x.IsDeleted)
                .Include(x => x.Member)
                .Include(x => x.BookCopy)
                    .ThenInclude(x => x.Book)
                .Include(x => x.BookCopy)
                    .ThenInclude(x => x.Shelf)
                .Include(x => x.BookCopy)
                    .ThenInclude(x => x.Rack)
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    BookIssueId = x.Id,

                    x.MemberId,
                    MemberName = x.Member != null ? x.Member.MemberName : "",
                    CardexNo = x.Member != null ? x.Member.CardexNo : "",
                    PhoneNo = x.Member != null ? x.Member.PhoneNo : "",

                    MembershipExpiredOn = _context.MembershipRenewals
                        .Where(r => r.MemberDbId == x.MemberId && !r.IsDeleted)
                        .OrderByDescending(r => r.ValidTo)
                        .Select(r => (DateTime?)r.ValidTo)
                        .FirstOrDefault(),

                    MemberStatus = _context.MembershipRenewals
                        .Where(r => r.MemberDbId == x.MemberId && !r.IsDeleted)
                        .OrderByDescending(r => r.ValidTo)
                        .Select(r => (DateTime?)r.ValidTo)
                        .FirstOrDefault() == null
                            ? "Not Renewed"
                            : _context.MembershipRenewals
                                .Where(r => r.MemberDbId == x.MemberId && !r.IsDeleted)
                                .OrderByDescending(r => r.ValidTo)
                                .Select(r => r.ValidTo)
                                .FirstOrDefault() >= today
                                    ? "Active"
                                    : "Expired",

                    x.BookCopyId,
                    BookId = x.BookCopy != null ? x.BookCopy.BookId : 0,
                    BookName = x.BookCopy != null && x.BookCopy.Book != null ? x.BookCopy.Book.Title : "",
                    ISBN = x.BookCopy != null && x.BookCopy.Book != null ? x.BookCopy.Book.ISBN : "",
                    Barcode = x.BookCopy != null ? x.BookCopy.Barcode : "",
                    BookPurchaseDetailId = x.BookCopy != null ? x.BookCopy.BookPurchaseDetailId : (int?)null,

                    ShelfId = x.BookCopy != null ? x.BookCopy.ShelfId : 0,
                    ShelfName = x.BookCopy != null && x.BookCopy.Shelf != null ? x.BookCopy.Shelf.ShelfName : "",
                    RackId = x.BookCopy != null ? x.BookCopy.RackId : 0,
                    RackName = x.BookCopy != null && x.BookCopy.Rack != null ? x.BookCopy.Rack.RackName : "",

                    x.IssueDate,
                    x.DueDate,

                    ReturnDate = _context.BookReturns
                        .Where(r => r.BookIssueId == x.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.Id)
                        .Select(r => (DateTime?)r.ReturnDate)
                        .FirstOrDefault(),

                    OverdueDays = _context.BookReturns
                        .Where(r => r.BookIssueId == x.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.Id)
                        .Select(r => (int?)r.OverdueDays)
                        .FirstOrDefault(),

                    FineAmount = _context.BookReturns
                        .Where(r => r.BookIssueId == x.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.Id)
                        .Select(r => (decimal?)r.FineAmount)
                        .FirstOrDefault(),

                    Status = _context.BookReturns.Any(r => r.BookIssueId == x.Id && !r.IsDeleted)
                        ? "Returned"
                        : x.Status,

                    x.Remarks,
                    x.CreatedDate,
                    x.CreatedBy
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(int id)
        {
            var today = DateTime.Today;

            var data = await _context.BookIssues
                .Where(x => x.Id == id && !x.IsDeleted)
                .Include(x => x.Member)
                .Include(x => x.BookCopy)
                    .ThenInclude(x => x.Book)
                .Include(x => x.BookCopy)
                    .ThenInclude(x => x.Shelf)
                .Include(x => x.BookCopy)
                    .ThenInclude(x => x.Rack)
                .Select(x => new
                {
                    x.Id,
                    BookIssueId = x.Id,

                    x.MemberId,
                    MemberName = x.Member != null ? x.Member.MemberName : "",
                    CardexNo = x.Member != null ? x.Member.CardexNo : "",
                    PhoneNo = x.Member != null ? x.Member.PhoneNo : "",

                    MembershipExpiredOn = _context.MembershipRenewals
                        .Where(r => r.MemberDbId == x.MemberId && !r.IsDeleted)
                        .OrderByDescending(r => r.ValidTo)
                        .Select(r => (DateTime?)r.ValidTo)
                        .FirstOrDefault(),

                    MemberStatus = _context.MembershipRenewals
                        .Where(r => r.MemberDbId == x.MemberId && !r.IsDeleted)
                        .OrderByDescending(r => r.ValidTo)
                        .Select(r => (DateTime?)r.ValidTo)
                        .FirstOrDefault() == null
                            ? "Not Renewed"
                            : _context.MembershipRenewals
                                .Where(r => r.MemberDbId == x.MemberId && !r.IsDeleted)
                                .OrderByDescending(r => r.ValidTo)
                                .Select(r => r.ValidTo)
                                .FirstOrDefault() >= today
                                    ? "Active"
                                    : "Expired",

                    x.BookCopyId,
                    BookId = x.BookCopy != null ? x.BookCopy.BookId : 0,
                    BookName = x.BookCopy != null && x.BookCopy.Book != null ? x.BookCopy.Book.Title : "",
                    ISBN = x.BookCopy != null && x.BookCopy.Book != null ? x.BookCopy.Book.ISBN : "",
                    Barcode = x.BookCopy != null ? x.BookCopy.Barcode : "",
                    BookPurchaseDetailId = x.BookCopy != null ? x.BookCopy.BookPurchaseDetailId : (int?)null,

                    ShelfId = x.BookCopy != null ? x.BookCopy.ShelfId : 0,
                    ShelfName = x.BookCopy != null && x.BookCopy.Shelf != null ? x.BookCopy.Shelf.ShelfName : "",
                    RackId = x.BookCopy != null ? x.BookCopy.RackId : 0,
                    RackName = x.BookCopy != null && x.BookCopy.Rack != null ? x.BookCopy.Rack.RackName : "",

                    x.IssueDate,
                    x.DueDate,

                    ReturnDate = _context.BookReturns
                        .Where(r => r.BookIssueId == x.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.Id)
                        .Select(r => (DateTime?)r.ReturnDate)
                        .FirstOrDefault(),

                    OverdueDays = _context.BookReturns
                        .Where(r => r.BookIssueId == x.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.Id)
                        .Select(r => (int?)r.OverdueDays)
                        .FirstOrDefault(),

                    FineAmount = _context.BookReturns
                        .Where(r => r.BookIssueId == x.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.Id)
                        .Select(r => (decimal?)r.FineAmount)
                        .FirstOrDefault(),

                    Status = _context.BookReturns.Any(r => r.BookIssueId == x.Id && !r.IsDeleted)
                        ? "Returned"
                        : x.Status,

                    x.Remarks,
                    x.CreatedDate,
                    x.CreatedBy
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpPost("issue")]
        public async Task<IActionResult> IssueBook(BookIssue bookIssue)
        {
            var member = await _context.Members
                .FirstOrDefaultAsync(x => x.Id == bookIssue.MemberId && !x.IsDeleted);

            if (member == null)
                return BadRequest("Member not found.");

            var validTo = await _context.MembershipRenewals
                .Where(x => x.MemberDbId == bookIssue.MemberId && !x.IsDeleted)
                .OrderByDescending(x => x.ValidTo)
                .Select(x => (DateTime?)x.ValidTo)
                .FirstOrDefaultAsync();

            if (validTo == null)
                return BadRequest("Membership not renewed.");

            if (validTo.Value.Date < DateTime.Today)
                return BadRequest("Membership expired. Please renew membership.");

            var maxBooksAllowed = await _context.MemberTypes
    .Where(mt => mt.Id == member.MemberTypeId && !mt.IsDeleted && mt.IsActive)
    .Select(mt => mt.MaxBooksAllowed)
    .FirstOrDefaultAsync();

            if (maxBooksAllowed <= 0)
                return BadRequest("Member type or max book limit not configured.");

            var pendingCount = await _context.BookIssues
                .Where(x => x.MemberId == bookIssue.MemberId && !x.IsDeleted)
                .CountAsync(x => !_context.BookReturns.Any(r => r.BookIssueId == x.Id && !r.IsDeleted));

            if (pendingCount >= maxBooksAllowed)
                return BadRequest($"Book issue limit exceeded. Max allowed: {maxBooksAllowed}, Pending: {pendingCount}, Remaining: 0.");

            var bookCopy = await _context.BookCopies
                .FirstOrDefaultAsync(x => x.Id == bookIssue.BookCopyId && !x.IsDeleted);

            if (bookCopy == null)
                return BadRequest("Book copy not found.");

            if (bookCopy.Status == "Issued")
                return BadRequest("Book already issued.");

            var alreadyIssued = await _context.BookIssues
                .Where(x => x.BookCopyId == bookIssue.BookCopyId && !x.IsDeleted)
                .AnyAsync(x => !_context.BookReturns.Any(r => r.BookIssueId == x.Id && !r.IsDeleted));

            if (alreadyIssued)
                return BadRequest("Book already issued.");

            bookIssue.IssueDate = bookIssue.IssueDate == default ? DateTime.Today : bookIssue.IssueDate;
            bookIssue.Status = "Issued";
            bookIssue.IsActive = true;
            bookIssue.IsDeleted = false;
            bookIssue.CreatedDate = DateTime.Now;
            bookIssue.CreatedBy = string.IsNullOrWhiteSpace(bookIssue.CreatedBy) ? "Admin" : bookIssue.CreatedBy;

            bookCopy.Status = "Issued";

            _context.BookIssues.Add(bookIssue);
            await _context.SaveChangesAsync();

            return Ok(bookIssue);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIssue(int id, BookIssue bookIssue)
        {
            if (id != bookIssue.Id)
                return BadRequest();

            var existing = await _context.BookIssues
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (existing == null)
                return NotFound();

            var alreadyReturned = await _context.BookReturns
                .AnyAsync(x => x.BookIssueId == id && !x.IsDeleted);

            if (alreadyReturned)
                return BadRequest("Returned transaction cannot be edited.");

            existing.MemberId = bookIssue.MemberId;
            existing.BookCopyId = bookIssue.BookCopyId;
            existing.IssueDate = bookIssue.IssueDate;
            existing.DueDate = bookIssue.DueDate;
            existing.Remarks = bookIssue.Remarks;
            existing.Status = "Issued";

            existing.IsEdited = true;
            existing.EditedDate = DateTime.Now;
            existing.EditedBy = string.IsNullOrWhiteSpace(bookIssue.EditedBy) ? "Admin" : bookIssue.EditedBy;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        [HttpPost("return/{bookIssueId}")]
        public async Task<IActionResult> ReturnBook(int bookIssueId, BookReturn bookReturn)
        {
            var issue = await _context.BookIssues
                .Include(x => x.BookCopy)
                .FirstOrDefaultAsync(x => x.Id == bookIssueId && !x.IsDeleted);

            if (issue == null)
                return NotFound("Issue transaction not found.");

            var alreadyReturned = await _context.BookReturns
                .AnyAsync(x => x.BookIssueId == bookIssueId && !x.IsDeleted);

            if (alreadyReturned)
                return BadRequest("Book already returned.");

            var returnDate = bookReturn.ReturnDate == default ? DateTime.Today : bookReturn.ReturnDate.Date;
            var overdueDays = returnDate > issue.DueDate.Date
                ? (returnDate - issue.DueDate.Date).Days
                : 0;

            bookReturn.BookIssueId = bookIssueId;
            bookReturn.ReturnDate = returnDate;
            bookReturn.OverdueDays = overdueDays;
            bookReturn.FineAmount = bookReturn.FineAmount;
            bookReturn.IsActive = true;
            bookReturn.IsDeleted = false;
            bookReturn.CreatedDate = DateTime.Now;
            bookReturn.CreatedBy = string.IsNullOrWhiteSpace(bookReturn.CreatedBy) ? "Admin" : bookReturn.CreatedBy;

            issue.Status = "Returned";

            if (issue.BookCopy != null)
            {
                issue.BookCopy.Status = "Available";
            }

            _context.BookReturns.Add(bookReturn);
            await _context.SaveChangesAsync();

            return Ok(bookReturn);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIssue(int id, [FromQuery] string? deletedBy)
        {
            var issue = await _context.BookIssues
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (issue == null)
                return NotFound();

            var returned = await _context.BookReturns
                .AnyAsync(x => x.BookIssueId == id && !x.IsDeleted);

            if (returned)
                return BadRequest("Cannot delete. Book already returned.");

            var bookCopy = await _context.BookCopies.FindAsync(issue.BookCopyId);
            if (bookCopy != null)
                bookCopy.Status = "Available";

            issue.IsDeleted = true;
            issue.IsActive = false;
            issue.DeletedDate = DateTime.Now;
            issue.DeletedBy = string.IsNullOrWhiteSpace(deletedBy) ? "Admin" : deletedBy;

            await _context.SaveChangesAsync();

            return Ok("Transaction deleted successfully.");
        }

        [HttpGet("search-members")]
        public async Task<IActionResult> SearchMembers(string? term)
        {
            term = term ?? "";
            var today = DateTime.Today;

            var data = await _context.Members
                .Include(x => x.MemberType)
                .Where(x =>
                    !x.IsDeleted &&
                    (
                        x.MemberName.Contains(term) ||
                        x.CardexNo.Contains(term) ||
                        x.PhoneNo.Contains(term) ||
                        x.MemberId.Contains(term)
                    ))
                .Select(x => new
                {
                    x.Id,
                    x.MemberId,
                    x.MemberName,
                    x.CardexNo,
                    x.PhoneNo,

                    MemberTypeId = x.MemberTypeId,
                    MemberTypeName = x.MemberType != null ? x.MemberType.MemberTypeName : "",
                    MaxBooksAllowed = x.MemberType != null ? x.MemberType.MaxBooksAllowed : 0,

                    PendingBooksCount = _context.BookIssues
                    .Where(i => i.MemberId == x.Id && !i.IsDeleted)
                    .Count(i => !_context.BookReturns.Any(r => r.BookIssueId == i.Id && !r.IsDeleted)),

                    RemainingBooksAllowed =
                    (x.MemberType != null ? x.MemberType.MaxBooksAllowed : 0) -
                     _context.BookIssues
                     .Where(i => i.MemberId == x.Id && !i.IsDeleted)
                     .Count(i => !_context.BookReturns.Any(r => r.BookIssueId == i.Id && !r.IsDeleted)),

                    MembershipExpiredOn = _context.MembershipRenewals
                        .Where(r => r.MemberDbId == x.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.ValidTo)
                        .Select(r => (DateTime?)r.ValidTo)
                        .FirstOrDefault(),

                    MemberStatus = _context.MembershipRenewals
                        .Where(r => r.MemberDbId == x.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.ValidTo)
                        .Select(r => (DateTime?)r.ValidTo)
                        .FirstOrDefault() == null
                            ? "Not Renewed"
                            : _context.MembershipRenewals
                                .Where(r => r.MemberDbId == x.Id && !r.IsDeleted)
                                .OrderByDescending(r => r.ValidTo)
                                .Select(r => r.ValidTo)
                                .FirstOrDefault() >= today
                                    ? "Active"
                                    : "Expired"
                })
                .Take(20)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("search-books")]
        public async Task<IActionResult> SearchBooks(string? term)
        {
            term = term ?? "";

            var data = await _context.BookCopies
                .Include(x => x.Book)
                .Include(x => x.Shelf)
                .Include(x => x.Rack)
                .Where(x =>
                    !x.IsDeleted &&
                    x.Book != null &&
                    (
                        x.Barcode.Contains(term) ||
                        x.Book.Title.Contains(term) ||
                        x.Book.ISBN.Contains(term) ||
                        x.Book.CustomBarcode.Contains(term)
                    ))
                .Select(x => new
                {
                    x.Id,
                    BookCopyId = x.Id,

                    x.BookId,
                    BookName = x.Book != null ? x.Book.Title : "",
                    BookTitle = x.Book != null ? x.Book.Title : "",

                    ISBN = x.Book != null ? x.Book.ISBN : "",
                    CustomBarcode = x.Book != null ? x.Book.CustomBarcode : "",

                    Barcode = x.Barcode,
                    CopyBarcode = x.Barcode,

                    x.SerialNo,
                    x.Status,
                    x.BookPurchaseDetailId,

                    x.ShelfId,
                    ShelfName = x.Shelf != null ? x.Shelf.ShelfName : "",

                    x.RackId,
                    RackName = x.Rack != null ? x.Rack.RackName : ""
                })
                .OrderBy(x => x.BookTitle)
                .ThenBy(x => x.SerialNo)
                .Take(20)
                .ToListAsync();
            var exactBarcode = data.FirstOrDefault(x =>
                !string.IsNullOrWhiteSpace(x.Barcode) &&
                 x.Barcode.ToLower() == term.ToLower());

            if (exactBarcode != null && exactBarcode.Status != "Available")
            {
                return BadRequest($"Book copy {exactBarcode.Barcode} is already issued.");
            }
            data = data
             .Where(x => x.Status == "Available")
             .ToList();
            return Ok(data);
        }
    }
}