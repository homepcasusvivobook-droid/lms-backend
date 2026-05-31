using LMS.API.Data;
using LMS.API.DTOs;
using LMS.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _context.Books
                .Where(x => !x.IsDeleted)
                .Include(x => x.Category)
                .Include(x => x.Author)
                .Include(x => x.Publisher)
                .Include(x => x.Language)
                .OrderBy(x => x.CustomBarcode)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.ISBN,
                    x.BookPrefix,
                    x.CodeNo,
                    x.CustomBarcode,

                    x.CategoryId,
                    CategoryName = x.Category != null ? x.Category.CategoryName : "",

                    x.AuthorId,
                    AuthorName = x.Author != null ? x.Author.AuthorName : "",

                    x.PublisherId,
                    PublisherName = x.Publisher != null ? x.Publisher.PublisherName : "",

                    x.LanguageId,
                    LanguageName = x.Language != null ? x.Language.LanguageName : "",

                    TotalCopies = _context.BookCopies.Count(c =>
                        c.BookId == x.Id &&
                        !c.IsDeleted),

                    AvailableCopies = _context.BookCopies.Count(c =>
                        c.BookId == x.Id &&
                        !c.IsDeleted &&
                        c.Status == "Available"),

                    x.IsActive,
                    x.CreatedDate,
                    x.CreatedBy
                })
                .ToListAsync();

            return Ok(books);
        }

        [HttpGet("{id}/rack-shelf-summary")]
        public async Task<IActionResult> GetRackShelfSummary(int id)
        {
            var summary = await _context.BookCopies
                .Where(x => x.BookId == id && !x.IsDeleted)
                .Include(x => x.Shelf)
                .Include(x => x.Rack)
                .GroupBy(x => new
                {
                    ShelfName = x.Shelf != null
                        ? x.Shelf.ShelfCode + " - " + x.Shelf.ShelfName
                        : "",

                    RackName = x.Rack != null
                        ? x.Rack.RackCode + " - " + x.Rack.RackName
                        : ""
                })
                .Select(g => new
                {
                    ShelfName = g.Key.ShelfName,
                    RackName = g.Key.RackName,
                    TotalCount = g.Count(),
                    AvailableCount = g.Count(x => x.Status == "Available")
                })
                .OrderBy(x => x.ShelfName)
                .ThenBy(x => x.RackName)
                .ToListAsync();

            return Ok(summary);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _context.Books
                .Where(x => x.Id == id && !x.IsDeleted)
                .Include(x => x.Category)
                .Include(x => x.Author)
                .Include(x => x.Publisher)
                .Include(x => x.Language)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.ISBN,
                    x.BookPrefix,
                    x.CodeNo,
                    x.CustomBarcode,

                    x.CategoryId,
                    CategoryName = x.Category != null ? x.Category.CategoryName : "",

                    x.AuthorId,
                    AuthorName = x.Author != null ? x.Author.AuthorName : "",

                    x.PublisherId,
                    PublisherName = x.Publisher != null ? x.Publisher.PublisherName : "",

                    x.LanguageId,
                    LanguageName = x.Language != null ? x.Language.LanguageName : "",

                    TotalCopies = _context.BookCopies.Count(c =>
                        c.BookId == x.Id &&
                        !c.IsDeleted),

                    AvailableCopies = _context.BookCopies.Count(c =>
                        c.BookId == x.Id &&
                        !c.IsDeleted &&
                        c.Status == "Available"),

                    x.IsActive,
                    x.CreatedDate,
                    x.CreatedBy
                })
                .FirstOrDefaultAsync();

            if (book == null)
                return NotFound();

            return Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> PostBook(Book book)
        {
            book.CreatedDate = DateTime.Now;
            book.IsActive = true;
            book.IsDeleted = false;

            if (string.IsNullOrWhiteSpace(book.Title))
                return BadRequest("Book title is required.");

            book.ISBN = string.IsNullOrWhiteSpace(book.ISBN) ? null : book.ISBN.Trim();

            if (!string.IsNullOrWhiteSpace(book.ISBN))
            {
                bool isbnExists = await _context.Books
                    .AnyAsync(x => x.ISBN == book.ISBN && !x.IsDeleted);

                if (isbnExists)
                    return BadRequest($"ISBN already exists: {book.ISBN}");
            }

            if (!string.IsNullOrWhiteSpace(book.BookPrefix) &&
                !string.IsNullOrWhiteSpace(book.CodeNo))
            {
                book.BookPrefix = book.BookPrefix.Trim();
                book.CodeNo = book.CodeNo.Trim().PadLeft(4, '0');
                book.CustomBarcode = book.BookPrefix + book.CodeNo;
            }
            else
            {
                await GenerateCustomBarcode(book);
            }

            if (!string.IsNullOrWhiteSpace(book.CustomBarcode))
            {
                bool barcodeExists = await _context.Books
                    .AnyAsync(x => x.CustomBarcode == book.CustomBarcode && !x.IsDeleted);

                if (barcodeExists)
                    return BadRequest($"Custom barcode already exists: {book.CustomBarcode}");
            }

            book.CreatedBy = string.IsNullOrWhiteSpace(book.CreatedBy)
                ? "Admin"
                : book.CreatedBy;

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return Ok(book);
        }

        [HttpPost("bulk-import")]
        public async Task<IActionResult> BulkImportBooks(List<BookImportDto> books)
        {
            if (books == null || books.Count == 0)
                return BadRequest("No books found for import.");

            int inserted = 0;
            int skipped = 0;
            var errors = new List<object>();
            int rowNo = 1;

            foreach (var item in books)
            {
                rowNo++;

                try
                {
                    if (string.IsNullOrWhiteSpace(item.BookName))
                    {
                        skipped++;
                        errors.Add(new
                        {
                            RowNo = rowNo,
                            BookName = item.BookName,
                            Reason = "Book name is empty."
                        });
                        continue;
                    }

                    string? isbn = string.IsNullOrWhiteSpace(item.ISBN)
                        ? null
                        : item.ISBN.Trim();

                    if (!string.IsNullOrWhiteSpace(isbn))
                    {
                        bool isbnExists = await _context.Books
                            .AnyAsync(x => x.ISBN == isbn && !x.IsDeleted);

                        if (isbnExists)
                        {
                            skipped++;
                            errors.Add(new
                            {
                                RowNo = rowNo,
                                BookName = item.BookName,
                                ISBN = isbn,
                                Reason = "ISBN already exists."
                            });
                            continue;
                        }
                    }

                    if (item.CategoryId <= 0)
                    {
                        skipped++;
                        errors.Add(new
                        {
                            RowNo = rowNo,
                            BookName = item.BookName,
                            Reason = "CategoryId is missing or invalid."
                        });
                        continue;
                    }

                    bool categoryExists = await _context.Categories
                        .AnyAsync(x => x.Id == item.CategoryId && !x.IsDeleted);

                    if (!categoryExists)
                    {
                        skipped++;
                        errors.Add(new
                        {
                            RowNo = rowNo,
                            BookName = item.BookName,
                            CategoryId = item.CategoryId,
                            Reason = "CategoryId does not exist in Categories table."
                        });
                        continue;
                    }

                    if (item.AuthorId.HasValue && item.AuthorId.Value > 0)
                    {
                        bool authorExists = await _context.Authors
                            .AnyAsync(x => x.Id == item.AuthorId.Value && !x.IsDeleted);

                        if (!authorExists)
                        {
                            skipped++;
                            errors.Add(new
                            {
                                RowNo = rowNo,
                                BookName = item.BookName,
                                AuthorId = item.AuthorId,
                                Reason = "AuthorId does not exist in Authors table."
                            });
                            continue;
                        }
                    }

                    if (item.PublisherId.HasValue && item.PublisherId.Value > 0)
                    {
                        bool publisherExists = await _context.Publishers
                            .AnyAsync(x => x.Id == item.PublisherId.Value && !x.IsDeleted);

                        if (!publisherExists)
                        {
                            skipped++;
                            errors.Add(new
                            {
                                RowNo = rowNo,
                                BookName = item.BookName,
                                PublisherId = item.PublisherId,
                                Reason = "PublisherId does not exist in Publishers table."
                            });
                            continue;
                        }
                    }

                    if (!item.LanguageId.HasValue || item.LanguageId.Value <= 0)
                    {
                        skipped++;
                        errors.Add(new
                        {
                            RowNo = rowNo,
                            BookName = item.BookName,
                            Reason = "LanguageId is missing or invalid."
                        });
                        continue;
                    }

                    bool languageExists = await _context.Languages
                        .AnyAsync(x => x.Id == item.LanguageId.Value && !x.IsDeleted);

                    if (!languageExists)
                    {
                        skipped++;
                        errors.Add(new
                        {
                            RowNo = rowNo,
                            BookName = item.BookName,
                            LanguageId = item.LanguageId,
                            Reason = "LanguageId does not exist in Languages table."
                        });
                        continue;
                    }

                    string? codePrefix = string.IsNullOrWhiteSpace(item.CodePrefix)
                        ? null
                        : item.CodePrefix.Trim();

                    string? codeNo = string.IsNullOrWhiteSpace(item.CodeNo)
                        ? null
                        : item.CodeNo.Trim().PadLeft(4, '0');

                    string? customBarcode = null;

                    if (!string.IsNullOrWhiteSpace(codePrefix) &&
                        !string.IsNullOrWhiteSpace(codeNo))
                    {
                        customBarcode = codePrefix + codeNo;
                    }

                    if (!string.IsNullOrWhiteSpace(customBarcode))
                    {
                        bool barcodeExists = await _context.Books
                            .AnyAsync(x => x.CustomBarcode == customBarcode && !x.IsDeleted);

                        if (barcodeExists)
                        {
                            skipped++;
                            errors.Add(new
                            {
                                RowNo = rowNo,
                                BookName = item.BookName,
                                Barcode = customBarcode,
                                Reason = "Custom barcode already exists."
                            });
                            continue;
                        }
                    }

                    var book = new Book
                    {
                        Title = item.BookName.Trim(),
                        ISBN = isbn,

                        CategoryId = item.CategoryId,
                        AuthorId = item.AuthorId.HasValue && item.AuthorId.Value > 0 ? item.AuthorId : null,
                        PublisherId = item.PublisherId.HasValue && item.PublisherId.Value > 0 ? item.PublisherId : null,
                        LanguageId = item.LanguageId,

                        BookPrefix = codePrefix,
                        CodeNo = codeNo,
                        CustomBarcode = customBarcode,

                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now,
                        CreatedBy = string.IsNullOrWhiteSpace(item.CreatedBy) ? "Admin" : item.CreatedBy
                    };

                    if (string.IsNullOrWhiteSpace(book.CustomBarcode))
                    {
                        await GenerateCustomBarcode(book);
                    }

                    _context.Books.Add(book);
                    await _context.SaveChangesAsync();

                    inserted++;
                }
                catch (Exception ex)
                {
                    skipped++;

                    errors.Add(new
                    {
                        RowNo = rowNo,
                        BookName = item.BookName,
                        Error = ex.InnerException?.Message ?? ex.Message
                    });

                    _context.ChangeTracker.Clear();
                }
            }

            return Ok(new
            {
                Message = "Bulk import completed.",
                TotalRows = books.Count,
                Inserted = inserted,
                Skipped = skipped,
                Errors = errors
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, Book book)
        {
            if (id != book.Id)
                return BadRequest();

            var existingBook = await _context.Books.FindAsync(id);

            if (existingBook == null || existingBook.IsDeleted)
                return NotFound();

            if (string.IsNullOrWhiteSpace(book.Title))
                return BadRequest("Book title is required.");

            book.ISBN = string.IsNullOrWhiteSpace(book.ISBN) ? null : book.ISBN.Trim();

            if (!string.IsNullOrWhiteSpace(book.ISBN))
            {
                bool isbnExists = await _context.Books
                    .AnyAsync(x => x.Id != id && x.ISBN == book.ISBN && !x.IsDeleted);

                if (isbnExists)
                    return BadRequest($"ISBN already exists: {book.ISBN}");
            }

            bool prefixChanged =
                existingBook.CategoryId != book.CategoryId ||
                existingBook.LanguageId != book.LanguageId;

            existingBook.Title = book.Title.Trim();
            existingBook.ISBN = book.ISBN;
            existingBook.CategoryId = book.CategoryId;
            existingBook.AuthorId = book.AuthorId;
            existingBook.PublisherId = book.PublisherId;
            existingBook.LanguageId = book.LanguageId;

            if (!string.IsNullOrWhiteSpace(book.BookPrefix) &&
                !string.IsNullOrWhiteSpace(book.CodeNo))
            {
                existingBook.BookPrefix = book.BookPrefix.Trim();
                existingBook.CodeNo = book.CodeNo.Trim().PadLeft(4, '0');
                existingBook.CustomBarcode = existingBook.BookPrefix + existingBook.CodeNo;
            }
            else if (prefixChanged || string.IsNullOrWhiteSpace(existingBook.CustomBarcode))
            {
                await GenerateCustomBarcode(existingBook);
            }

            if (!string.IsNullOrWhiteSpace(existingBook.CustomBarcode))
            {
                bool barcodeExists = await _context.Books
                    .AnyAsync(x =>
                        x.Id != id &&
                        x.CustomBarcode == existingBook.CustomBarcode &&
                        !x.IsDeleted);

                if (barcodeExists)
                    return BadRequest($"Custom barcode already exists: {existingBook.CustomBarcode}");
            }

            existingBook.IsEdited = true;
            existingBook.EditedDate = DateTime.Now;
            existingBook.EditedBy = string.IsNullOrWhiteSpace(book.EditedBy) ? "Admin" : book.EditedBy;

            await _context.SaveChangesAsync();

            return Ok(existingBook);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id, [FromQuery] string? deletedBy)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null || book.IsDeleted)
                return NotFound();

            book.IsDeleted = true;
            book.IsActive = false;
            book.DeletedDate = DateTime.Now;
            book.DeletedBy = deletedBy ?? "Admin";

            await _context.SaveChangesAsync();

            return Ok("Book deleted successfully.");
        }

        [HttpGet("isbn/{isbn}")]
        public async Task<IActionResult> GetBookByISBN(string isbn)
        {
            var book = await _context.Books
                .Include(x => x.Category)
                .Include(x => x.Author)
                .Include(x => x.Publisher)
                .Include(x => x.Language)
                .Where(x => x.ISBN == isbn && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.ISBN,
                    x.BookPrefix,
                    x.CodeNo,
                    x.CustomBarcode,

                    x.CategoryId,
                    CategoryName = x.Category != null ? x.Category.CategoryName : "",

                    x.AuthorId,
                    AuthorName = x.Author != null ? x.Author.AuthorName : "",

                    x.PublisherId,
                    PublisherName = x.Publisher != null ? x.Publisher.PublisherName : "",

                    x.LanguageId,
                    LanguageName = x.Language != null ? x.Language.LanguageName : "",

                    TotalCopies = _context.BookCopies.Count(c =>
                        c.BookId == x.Id &&
                        !c.IsDeleted),

                    AvailableCopies = _context.BookCopies.Count(c =>
                        c.BookId == x.Id &&
                        !c.IsDeleted &&
                        c.Status == "Available")
                })
                .FirstOrDefaultAsync();

            if (book == null)
                return NotFound("Book not found.");

            return Ok(book);
        }
        [HttpGet("copies-for-reprint")]
        public async Task<IActionResult> GetBookCopiesForReprint()
        {
            var copies = await _context.BookCopies
                .Where(c => !c.IsDeleted && c.Book != null && !c.Book.IsDeleted)
                .Include(c => c.Book)
                .Include(c => c.Shelf)
                .Include(c => c.Rack)
                .OrderByDescending(c => c.Id)
                .Select(c => new
                {
                    copyId = c.Id,
                    bookId = c.BookId,

                    barcode = c.Barcode,
                    serialNo = c.SerialNo,

                    title = c.Book != null ? c.Book.Title : "",
                    isbn = c.Book != null ? c.Book.ISBN : "",
                    customBarcode = c.Book != null ? c.Book.CustomBarcode : "",

                    shelfId = c.ShelfId,
                    shelfName = c.Shelf != null ? c.Shelf.ShelfName : "",

                    rackId = c.RackId,
                    rackName = c.Rack != null ? c.Rack.RackName : "",

                    status = c.Status
                })
                .ToListAsync();

            return Ok(copies);
        }
        private async Task GenerateCustomBarcode(Book book)
        {
            var category = await _context.Categories.FindAsync(book.CategoryId);

            var language = book.LanguageId.HasValue
                ? await _context.Languages.FindAsync(book.LanguageId.Value)
                : null;

            if (category == null)
                throw new Exception("Category not found.");

            if (language == null)
                throw new Exception("Language not found.");

            if (string.IsNullOrWhiteSpace(category.CategoryPrefix))
                throw new Exception("CategoryPrefix missing.");

            if (string.IsNullOrWhiteSpace(language.LanguagePrefix))
                throw new Exception("LanguagePrefix missing.");

            var bookPrefix = $"{language.LanguagePrefix}{category.CategoryPrefix}";

            var codeList = await _context.Books
                .Where(x => !x.IsDeleted && x.BookPrefix == bookPrefix && x.CodeNo != null)
                .Select(x => x.CodeNo)
                .ToListAsync();

            int lastNo = 0;

            foreach (var code in codeList)
            {
                if (int.TryParse(code, out int number) && number > lastNo)
                    lastNo = number;
            }

            int nextNo = lastNo + 1;

            book.BookPrefix = bookPrefix;
            book.CodeNo = nextNo.ToString("D4");
            book.CustomBarcode = bookPrefix + book.CodeNo;
        }
    }
}