using LMS.API.Data;
using LMS.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Categories
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.CategoryPrefix)
                .Select(x => new
                {
                    id = x.Id,
                    categoryCode = x.CategoryCode,
                    categoryName = x.CategoryName,
                    categoryPrefix = x.CategoryPrefix,
                    name = x.CategoryName
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryDto dto)
        {
            dto.CategoryCode = dto.CategoryCode?.Trim();
            dto.CategoryName = dto.CategoryName?.Trim();
            dto.CategoryPrefix = dto.CategoryPrefix?.Trim();

            if (string.IsNullOrWhiteSpace(dto.CategoryCode))
                return BadRequest("Category Code is required");

            if (string.IsNullOrWhiteSpace(dto.CategoryName))
                return BadRequest("Category Name is required");

            if (string.IsNullOrWhiteSpace(dto.CategoryPrefix))
                return BadRequest("Category Prefix is required");

            bool codeExists = await _context.Categories.AnyAsync(x =>
                !x.IsDeleted &&
                x.CategoryCode != null &&
                x.CategoryCode.ToLower() == dto.CategoryCode.ToLower());

            if (codeExists)
                return BadRequest("Category Code already exists");

            bool prefixExists = await _context.Categories.AnyAsync(x =>
                !x.IsDeleted &&
                x.CategoryPrefix != null &&
                x.CategoryPrefix.ToLower() == dto.CategoryPrefix.ToLower());

            if (prefixExists)
                return BadRequest("Category Prefix already exists");

            var item = new Category
            {
                CategoryCode = dto.CategoryCode,
                CategoryName = dto.CategoryName,
                CategoryPrefix = dto.CategoryPrefix,
                CreatedBy = "System",
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false
            };

            _context.Categories.Add(item);
            await _context.SaveChangesAsync();

            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CategoryDto dto)
        {
            var item = await _context.Categories.FindAsync(id);

            if (item == null)
                return NotFound();

            dto.CategoryCode = dto.CategoryCode?.Trim();
            dto.CategoryName = dto.CategoryName?.Trim();
            dto.CategoryPrefix = dto.CategoryPrefix?.Trim();

            if (string.IsNullOrWhiteSpace(dto.CategoryCode))
                return BadRequest("Category Code is required");

            if (string.IsNullOrWhiteSpace(dto.CategoryName))
                return BadRequest("Category Name is required");

            if (string.IsNullOrWhiteSpace(dto.CategoryPrefix))
                return BadRequest("Category Prefix is required");

            bool codeExists = await _context.Categories.AnyAsync(x =>
                x.Id != id &&
                !x.IsDeleted &&
                x.CategoryCode != null &&
                x.CategoryCode.ToLower() == dto.CategoryCode.ToLower());

            if (codeExists)
                return BadRequest("Category Code already exists");

            bool prefixExists = await _context.Categories.AnyAsync(x =>
                x.Id != id &&
                !x.IsDeleted &&
                x.CategoryPrefix != null &&
                x.CategoryPrefix.ToLower() == dto.CategoryPrefix.ToLower());

            if (prefixExists)
                return BadRequest("Category Prefix already exists");

            item.CategoryCode = dto.CategoryCode;
            item.CategoryName = dto.CategoryName;
            item.CategoryPrefix = dto.CategoryPrefix;
            item.EditedDate = DateTime.Now;
            item.EditedBy = "System";

            await _context.SaveChangesAsync();

            return Ok(item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Categories.FindAsync(id);

            if (item == null)
                return NotFound();

            item.IsDeleted = true;
            item.DeletedDate = DateTime.Now;
            item.DeletedBy = "System";

            await _context.SaveChangesAsync();

            return Ok("Category deleted successfully");
        }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string? CategoryCode { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryPrefix { get; set; }
        public string? Name { get; set; }
    }
}