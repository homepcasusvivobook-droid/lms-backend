using LMS.API.Data;
using LMS.API.DTOs.Masters;
using LMS.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PublishersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Publishers
                .Where(x => !x.IsDeleted)
                .Select(x => new MasterDto
                {
                    Id = x.Id,
                    Name = x.PublisherName
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(MasterDto dto)
        {
            var item = new Publisher
            {
                PublisherName = dto.Name,
                CreatedBy = "System"
            };

            _context.Publishers.Add(item);

            await _context.SaveChangesAsync();

            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MasterDto dto)
        {
            var item = await _context.Publishers.FindAsync(id);

            if (item == null)
                return NotFound();

            item.PublisherName = dto.Name;
            item.EditedDate = DateTime.Now;
            item.EditedBy = "System";

            await _context.SaveChangesAsync();

            return Ok(item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Publishers.FindAsync(id);

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