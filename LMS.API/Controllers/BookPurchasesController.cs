using LMS.API.Data;
using LMS.API.DTOs;
using LMS.API.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookPurchasesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookPurchasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurchase(AddBookPurchaseDto dto)
        {
            dto.PurchaseType = string.IsNullOrWhiteSpace(dto.PurchaseType)
                ? "Purchase"
                : dto.PurchaseType.Trim();

            dto.InvoiceNo = dto.InvoiceNo?.Trim() ?? "";
            dto.StoreName = dto.StoreName?.Trim();

            if (dto.PurchaseDate == default)
                return BadRequest("Purchase date is required.");

            if (dto.PurchaseType == "Purchase")
            {
                if (string.IsNullOrWhiteSpace(dto.InvoiceNo))
                    return BadRequest("Invoice number is required.");

                if (string.IsNullOrWhiteSpace(dto.StoreName))
                    return BadRequest("Store name is required.");
            }

            if (dto.ConversionRate <= 0)
                return BadRequest("Conversion rate must be greater than zero.");

            dto.TotalCostAed = dto.Currency == "AED"
                ? dto.TotalCost
                : dto.TotalCost / dto.ConversionRate;
            if (dto.Details == null || dto.Details.Count == 0)
                return BadRequest("Please add at least one book item.");

            bool invoiceExists = await _context.BookPurchases
                .AnyAsync(x => x.InvoiceNo == dto.InvoiceNo && !x.IsDeleted);

            if (invoiceExists)
                return BadRequest("This invoice number already exists.");

            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var purchase = new BookPurchase
                {
                    InvoiceNo = dto.InvoiceNo,
                    StoreName = dto.StoreName,
                    PurchaseDate = dto.PurchaseDate,
                    TotalCopies = dto.Details.Sum(x => x.NoOfCopies),
                    TotalCost = dto.TotalCost,
                    Remarks = dto.Remarks,
                    CreatedBy = dto.CreatedBy ?? "Admin",
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    PurchaseType = dto.PurchaseType,
                    Currency = dto.Currency,
                    ConversionRate = dto.ConversionRate,
                    TotalCostAed = dto.TotalCostAed,
                    SponsorName = dto.SponsorName
                };

                _context.BookPurchases.Add(purchase);
                await _context.SaveChangesAsync();

                var generatedBarcodes = new List<string>();

                foreach (var item in dto.Details)
                {
                    var book = await _context.Books
                        .FirstOrDefaultAsync(x => x.ISBN == item.ISBN && !x.IsDeleted);

                    if (book == null)
                        return BadRequest($"ISBN {item.ISBN} not found.");

                    var shelf = await _context.Shelves.FindAsync(item.ShelfId);
                    var rack = await _context.Racks.FindAsync(item.RackId);

                    if (shelf == null)
                        return BadRequest("Shelf not found.");

                    if (rack == null)
                        return BadRequest("Rack not found.");

                    var detail = new BookPurchaseDetail
                    {
                        BookPurchaseId = purchase.Id,
                        BookId = book.Id,
                        CategoryId = book.CategoryId,
                        ShelfId = item.ShelfId,
                        RackId = item.RackId,
                        NoOfCopies = item.NoOfCopies,
                        Cost = item.Cost,
                        Remarks = item.Remarks,
                        CreatedBy = dto.CreatedBy ?? "Admin",
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now
                    };

                    _context.BookPurchaseDetails.Add(detail);
                    await _context.SaveChangesAsync();

                    int lastSerialNo = await _context.BookCopies
                        .Where(x =>
                            x.BookId == book.Id &&
                            x.ShelfId == item.ShelfId &&
                            x.RackId == item.RackId &&
                            !x.IsDeleted)
                        .MaxAsync(x => (int?)x.SerialNo) ?? 0;

                    for (int i = 1; i <= item.NoOfCopies; i++)
                    {
                        int serialNo = lastSerialNo + i;
                            string barcode =
                            $"{book.CustomBarcode}-C{serialNo.ToString("D2")}";

                        var copy = new BookCopy
                        {
                            BookId = detail.BookId,
                            ShelfId = detail.ShelfId,
                            RackId = detail.RackId,
                            BookPurchaseDetailId = detail.Id,
                            SerialNo = serialNo,
                            Barcode = barcode,
                            Status = "Available",
                            IsActive = true,
                            IsDeleted = false,
                            CreatedDate = DateTime.Now,
                            CreatedBy = dto.CreatedBy
                        };

                        generatedBarcodes.Add(barcode);
                        _context.BookCopies.Add(copy);
                    }

                    await _context.SaveChangesAsync();
                }

                await dbTransaction.CommitAsync();

                return Ok(new
                {
                    Message = "Purchase saved successfully",
                    PurchaseId = purchase.Id,
                    purchase.InvoiceNo,
                    purchase.TotalCopies,
                    purchase.TotalCost,
                    Barcodes = generatedBarcodes
                });
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPurchases()
        {
            var purchases = await _context.BookPurchases
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new
                {
                    x.Id,
                    x.InvoiceNo,
                    x.StoreName,
                    x.PurchaseDate,
                    x.TotalCopies,
                    x.TotalCost,
                    x.Remarks,
                    x.CreatedDate,
                    x.CreatedBy,
                    x.PurchaseType,
                    x.Currency,
                    x.ConversionRate,
                    x.TotalCostAed,
                    x.SponsorName,
                })
                .ToListAsync();

            return Ok(purchases);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPurchase(int id)
        {
            var purchase = await _context.BookPurchases
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    x.InvoiceNo,
                    x.StoreName,
                    x.PurchaseDate,
                    x.TotalCopies,
                    x.TotalCost,
                    x.Remarks,
                    x.CreatedDate,
                    x.CreatedBy,
                    x.PurchaseType,
                    x.Currency,
                    x.ConversionRate,
                    x.TotalCostAed,
                    x.SponsorName,

                    Details = _context.BookPurchaseDetails
                        .Where(d => d.BookPurchaseId == x.Id && !d.IsDeleted)
                        .Select(d => new
                        {
                            d.Id,
                            d.BookId,
                            d.CategoryId,
                            d.ShelfId,
                            d.RackId,
                            d.NoOfCopies,
                            d.Cost,
                            d.Remarks,

                            ISBN = d.Book != null ? d.Book.ISBN : "",
                            Title = d.Book != null ? d.Book.Title : "",
                            CustomBarcode = d.Book != null ? d.Book.CustomBarcode : "",

                            Author = d.Book != null && d.Book.Author != null ? d.Book.Author.AuthorName : "",
                            Publisher = d.Book != null && d.Book.Publisher != null ? d.Book.Publisher.PublisherName : "",
                            Language = d.Book != null && d.Book.Language != null ? d.Book.Language.LanguageName : "",
                            CategoryName = d.Category != null ? d.Category.CategoryName : "",
                            ShelfName = d.Shelf != null ? d.Shelf.ShelfName : "",
                            RackName = d.Rack != null ? d.Rack.RackName : "",

                            Copies = _context.BookCopies
                                .Where(c => c.BookPurchaseDetailId == d.Id && !c.IsDeleted)
                                .Select(c => new
                                {
                                    c.Id,
                                    c.SerialNo,
                                    c.Barcode,
                                    c.Status
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (purchase == null)
                return NotFound();

            return Ok(purchase);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePurchase(int id, AddBookPurchaseDto dto)
        {
            var purchase = await _context.BookPurchases
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (purchase == null)
                return NotFound("Purchase not found.");
            dto.PurchaseType = string.IsNullOrWhiteSpace(dto.PurchaseType)
                 ? "Purchase"
                : dto.PurchaseType.Trim();

            dto.InvoiceNo = dto.InvoiceNo?.Trim() ?? "";
            dto.StoreName = dto.StoreName?.Trim();

            if (dto.PurchaseDate == default)
                return BadRequest("Purchase date is required.");

            if (dto.PurchaseType == "Purchase")
            {
                if (string.IsNullOrWhiteSpace(dto.InvoiceNo))
                    return BadRequest("Invoice number is required.");

                if (string.IsNullOrWhiteSpace(dto.StoreName))
                    return BadRequest("Store name is required.");
            }

            if (dto.ConversionRate <= 0)
                return BadRequest("Conversion rate must be greater than zero.");

            dto.TotalCostAed = dto.Currency == "AED"
                ? dto.TotalCost
                : dto.TotalCost / dto.ConversionRate;

            if (dto.Details == null || dto.Details.Count == 0)
                return BadRequest("Please add at least one book item.");

            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                purchase.InvoiceNo = dto.InvoiceNo;
                purchase.StoreName = dto.StoreName;
                purchase.PurchaseDate = dto.PurchaseDate;
                purchase.TotalCost = dto.TotalCost;
                purchase.PurchaseType = dto.PurchaseType;
                purchase.Currency = dto.Currency;
                purchase.ConversionRate = dto.ConversionRate;
                purchase.TotalCostAed = dto.TotalCostAed;
                purchase.SponsorName = dto.SponsorName;
                purchase.TotalCopies = dto.Details.Sum(x => x.NoOfCopies);
                purchase.Remarks = dto.Remarks;
                purchase.EditedDate = DateTime.Now;
                purchase.EditedBy = dto.EditedBy ?? "Admin";
                purchase.IsEdited = true;

                var oldDetails = await _context.BookPurchaseDetails
                    .Where(x => x.BookPurchaseId == purchase.Id && !x.IsDeleted)
                    .ToListAsync();

                var oldDetailIds = oldDetails.Select(x => x.Id).ToList();

                var oldCopies = await _context.BookCopies
                    .Where(x => x.BookPurchaseDetailId != null &&
                                oldDetailIds.Contains(x.BookPurchaseDetailId.Value) &&
                                !x.IsDeleted)
                    .ToListAsync();

                foreach (var copy in oldCopies)
                {
                    copy.IsDeleted = true;
                    copy.IsActive = false;
                    copy.DeletedDate = DateTime.Now;
                    copy.DeletedBy = dto.EditedBy ?? "Admin";
                }

                foreach (var detail in oldDetails)
                {
                    detail.IsDeleted = true;
                    detail.IsActive = false;

                    detail.DeletedDate = DateTime.Now;
                    detail.DeletedBy = dto.EditedBy ?? "Admin";
                }

                await _context.SaveChangesAsync();

                foreach (var item in dto.Details)
                {
                    var book = await _context.Books
                        .FirstOrDefaultAsync(x => x.ISBN == item.ISBN && !x.IsDeleted);

                    if (book == null)
                        return BadRequest($"ISBN {item.ISBN} not found.");

                    var shelf = await _context.Shelves.FindAsync(item.ShelfId);
                    var rack = await _context.Racks.FindAsync(item.RackId);

                    if (shelf == null)
                        return BadRequest("Shelf not found.");

                    if (rack == null)
                        return BadRequest("Rack not found.");

                    var detail = new BookPurchaseDetail
                    {
                        BookPurchaseId = purchase.Id,
                        BookId = book.Id,
                        CategoryId = book.CategoryId,
                        ShelfId = item.ShelfId,
                        RackId = item.RackId,
                        NoOfCopies = item.NoOfCopies,
                        Cost = item.Cost,
                        Remarks = item.Remarks,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now
                    };

                    _context.BookPurchaseDetails.Add(detail);
                    await _context.SaveChangesAsync();

                    int lastSerialNo = await _context.BookCopies
                        .Where(x =>
                            x.BookId == book.Id &&
                            x.ShelfId == item.ShelfId &&
                            x.RackId == item.RackId &&
                            !x.IsDeleted)
                        .MaxAsync(x => (int?)x.SerialNo) ?? 0;

                    for (int i = 1; i <= item.NoOfCopies; i++)
                    {
                        int serialNo = lastSerialNo + i;

                        string barcode =
                            $"{book.CustomBarcode}-C{serialNo.ToString("D2")}";

                        var copy = new BookCopy
                        {
                            BookId = detail.BookId,
                            ShelfId = detail.ShelfId,
                            RackId = detail.RackId,
                            BookPurchaseDetailId = detail.Id,
                            SerialNo = serialNo,
                            Barcode = barcode,
                            Status = "Available",
                            IsActive = true,
                            IsDeleted = false,
                            CreatedDate = DateTime.Now,
                            CreatedBy = dto.CreatedBy
                        };

                        _context.BookCopies.Add(copy);
                    }

                    await _context.SaveChangesAsync();
                }

                await dbTransaction.CommitAsync();

                return Ok(new
                {
                    Message = "Purchase updated successfully"
                });
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchase(int id, string? deletedBy)
        {
            var purchase = await _context.BookPurchases
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (purchase == null)
                return NotFound();

            var details = await _context.BookPurchaseDetails
                .Where(x => x.BookPurchaseId == id && !x.IsDeleted)
                .ToListAsync();

            var detailIds = details.Select(x => x.Id).ToList();

            var copies = await _context.BookCopies
                .Where(x => x.BookPurchaseDetailId != null &&
                            detailIds.Contains(x.BookPurchaseDetailId.Value) &&
                            !x.IsDeleted)
                .ToListAsync();

            // ==============================
            // CHECK ISSUED COPIES
            // ==============================
            bool hasIssuedCopies = copies.Any(x =>
                x.Status == "Issued" ||
                _context.BookIssues.Any(i =>
                     i.BookCopyId == x.Id &&
                     !i.IsDeleted &&
                     i.Status == "Issued"
                    )
            );

            if (hasIssuedCopies)
            {
                return BadRequest(
                    "Cannot delete this purchase because one or more book copies are already issued."
                );
            }

            // ==============================
            // DELETE COPIES
            // ==============================
            foreach (var copy in copies)
            {
                copy.IsDeleted = true;
                copy.IsActive = false;
                copy.DeletedDate = DateTime.Now;
                copy.DeletedBy = deletedBy ?? "Admin";
            }

            // ==============================
            // DELETE DETAILS
            // ==============================
            foreach (var detail in details)
            {
                detail.IsDeleted = true;
                detail.IsActive = false;
            }

            // ==============================
            // DELETE PURCHASE
            // ==============================
            purchase.IsDeleted = true;
            purchase.IsActive = false;
            purchase.DeletedBy = deletedBy ?? "Admin";
            purchase.DeletedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok("Purchase deleted successfully.");
        }
    }
}