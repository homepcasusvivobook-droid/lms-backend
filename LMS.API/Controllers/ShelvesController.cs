using LMS.API.Data;
using LMS.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShelvesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShelvesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetShelves()
        {
            var data = await _context.Shelves
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ShelfCode)
                .Select(x => new
                {
                    id = x.Id,
                    shelfCode = x.ShelfCode,
                    shelfName = x.ShelfName
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetShelf(int id)
        {
            var shelf = await _context.Shelves.FindAsync(id);

            if (shelf == null)
                return NotFound();

            return Ok(shelf);
        }

        [HttpPost]
        public async Task<IActionResult> CreateShelf(ShelfDto dto)
        {
            dto.ShelfCode = dto.ShelfCode?.Trim();
            dto.ShelfName = dto.ShelfName?.Trim();

            if (string.IsNullOrWhiteSpace(dto.ShelfCode))
                return BadRequest("Shelf Code is required");

            if (string.IsNullOrWhiteSpace(dto.ShelfName))
                return BadRequest("Shelf Name is required");

            bool codeExists = await _context.Shelves.AnyAsync(x =>
                !x.IsDeleted &&
                x.ShelfCode != null &&
                x.ShelfCode.ToLower() == dto.ShelfCode.ToLower());

            if (codeExists)
                return BadRequest("Shelf Code already exists");

            var shelf = new Shelf
            {
                ShelfCode = dto.ShelfCode,
                ShelfName = dto.ShelfName,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false
            };

            _context.Shelves.Add(shelf);
            await _context.SaveChangesAsync();

            return Ok(shelf);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShelf(int id, ShelfDto dto)
        {
            var shelf = await _context.Shelves.FindAsync(id);

            if (shelf == null)
                return NotFound();

            dto.ShelfCode = dto.ShelfCode?.Trim();
            dto.ShelfName = dto.ShelfName?.Trim();

            if (string.IsNullOrWhiteSpace(dto.ShelfCode))
                return BadRequest("Shelf Code is required");

            if (string.IsNullOrWhiteSpace(dto.ShelfName))
                return BadRequest("Shelf Name is required");

            bool codeExists = await _context.Shelves.AnyAsync(x =>
                x.Id != id &&
                !x.IsDeleted &&
                x.ShelfCode != null &&
                x.ShelfCode.ToLower() == dto.ShelfCode.ToLower());

            if (codeExists)
                return BadRequest("Shelf Code already exists");

            shelf.ShelfCode = dto.ShelfCode;
            shelf.ShelfName = dto.ShelfName;
            shelf.EditedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(shelf);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShelf(int id)
        {
            var shelf = await _context.Shelves.FindAsync(id);

            if (shelf == null)
                return NotFound();

            shelf.IsDeleted = true;
            shelf.DeletedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok("Shelf deleted successfully");
        }
    }

    public class ShelfDto
    {
        public int Id { get; set; }
        public string? ShelfCode { get; set; }
        public string? ShelfName { get; set; }
    }
}