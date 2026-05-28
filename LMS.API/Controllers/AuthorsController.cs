using LMS.API.Data;
using LMS.API.DTOs;
using LMS.API.DTOs.Masters;
using LMS.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Authors
                .Where(x => !x.IsDeleted)
                .Select(x => new MasterDto
                {
                    Id = x.Id,
                    Name = x.AuthorName
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(MasterDto dto)
        {
            var item = new Author
            {
                AuthorName = dto.Name,
                CreatedBy = "System"
            };

            _context.Authors.Add(item);
            await _context.SaveChangesAsync();

            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MasterDto dto)
        {
            var item = await _context.Authors.FindAsync(id);

            if (item == null)
                return NotFound();

            item.AuthorName = dto.Name;
            item.EditedDate = DateTime.Now;
            item.EditedBy = "System";

            await _context.SaveChangesAsync();

            return Ok(item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Authors.FindAsync(id);

            if (item == null)
                return NotFound();

            item.IsDeleted = true;
            item.DeletedDate = DateTime.Now;
            item.DeletedBy = "System";

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}