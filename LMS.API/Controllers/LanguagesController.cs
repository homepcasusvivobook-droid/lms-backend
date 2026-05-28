using LMS.API.Data;
using LMS.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LanguagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Languages
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.LanguagePrefix)
                .Select(x => new
                {
                    id = x.Id,
                    languageName = x.LanguageName,
                    languagePrefix = x.LanguagePrefix,
                    name = x.LanguageName
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(LanguageDto dto)
        {
            dto.LanguageName = dto.LanguageName?.Trim();
            dto.LanguagePrefix = dto.LanguagePrefix?.Trim();

            if (string.IsNullOrWhiteSpace(dto.LanguageName))
                return BadRequest("Language Name is required");

            if (string.IsNullOrWhiteSpace(dto.LanguagePrefix))
                return BadRequest("Language Prefix is required");

            bool prefixExists = await _context.Languages.AnyAsync(x =>
                !x.IsDeleted &&
                x.LanguagePrefix != null &&
                x.LanguagePrefix.ToLower() == dto.LanguagePrefix.ToLower());

            if (prefixExists)
                return BadRequest("Language Prefix already exists");

            var item = new Language
            {
                LanguageName = dto.LanguageName,
                LanguagePrefix = dto.LanguagePrefix,
                CreatedBy = "System",
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false
            };

            _context.Languages.Add(item);
            await _context.SaveChangesAsync();

            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, LanguageDto dto)
        {
            var item = await _context.Languages.FindAsync(id);

            if (item == null)
                return NotFound();

            dto.LanguageName = dto.LanguageName?.Trim();
            dto.LanguagePrefix = dto.LanguagePrefix?.Trim();

            if (string.IsNullOrWhiteSpace(dto.LanguageName))
                return BadRequest("Language Name is required");

            if (string.IsNullOrWhiteSpace(dto.LanguagePrefix))
                return BadRequest("Language Prefix is required");

            bool prefixExists = await _context.Languages.AnyAsync(x =>
                x.Id != id &&
                !x.IsDeleted &&
                x.LanguagePrefix != null &&
                x.LanguagePrefix.ToLower() == dto.LanguagePrefix.ToLower());

            if (prefixExists)
                return BadRequest("Language Prefix already exists");

            item.LanguageName = dto.LanguageName;
            item.LanguagePrefix = dto.LanguagePrefix;
            item.EditedDate = DateTime.Now;
            item.EditedBy = "System";

            await _context.SaveChangesAsync();

            return Ok(item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Languages.FindAsync(id);

            if (item == null)
                return NotFound();

            item.IsDeleted = true;
            item.DeletedDate = DateTime.Now;
            item.DeletedBy = "System";

            await _context.SaveChangesAsync();

            return Ok("Language deleted successfully");
        }
    }

    public class LanguageDto
    {
        public int Id { get; set; }
        public string? LanguageName { get; set; }
        public string? LanguagePrefix { get; set; }
        public string? Name { get; set; }
    }
}