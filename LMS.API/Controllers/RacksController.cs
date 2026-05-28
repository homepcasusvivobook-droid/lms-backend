using LMS.API.Data;
using LMS.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RacksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RacksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Racks
        [HttpGet]
        public async Task<IActionResult> GetRacks()
        {
            var data = await _context.Racks
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.RackCode)
                .Select(x => new
                {
                    id = x.Id,
                    rackCode = x.RackCode,
                    rackName = x.RackName,
                    shelfId = x.ShelfId,

                    shelfName = _context.Shelves
                        .Where(s => s.Id == x.ShelfId)
                        .Select(s => s.ShelfCode + " - " + s.ShelfName)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/Racks/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRack(int id)
        {
            var rack = await _context.Racks
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new
                {
                    id = x.Id,
                    rackCode = x.RackCode,
                    rackName = x.RackName,
                    shelfId = x.ShelfId
                })
                .FirstOrDefaultAsync();

            if (rack == null)
                return NotFound();

            return Ok(rack);
        }

        // POST: api/Racks
        [HttpPost]
        public async Task<IActionResult> CreateRack(Rack rack)
        {
            if (string.IsNullOrWhiteSpace(rack.RackCode))
                return BadRequest("Rack Code is required");

            if (string.IsNullOrWhiteSpace(rack.RackName))
                return BadRequest("Rack Name is required");

            if (rack.ShelfId <= 0)
                return BadRequest("Shelf is required");

            bool codeExists = await _context.Racks.AnyAsync(x =>
                !x.IsDeleted &&
                x.RackCode != null &&
                x.RackCode.ToLower() == rack.RackCode.ToLower());

            if (codeExists)
                return BadRequest("Rack Code already exists");

            rack.CreatedDate = DateTime.Now;
            rack.IsActive = true;
            rack.IsDeleted = false;

            _context.Racks.Add(rack);
            await _context.SaveChangesAsync();

            return Ok(rack);
        }

        // PUT: api/Racks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRack(int id, Rack rack)
        {
            if (id != rack.Id)
                return BadRequest();

            if (string.IsNullOrWhiteSpace(rack.RackCode))
                return BadRequest("Rack Code is required");

            if (string.IsNullOrWhiteSpace(rack.RackName))
                return BadRequest("Rack Name is required");

            if (rack.ShelfId <= 0)
                return BadRequest("Shelf is required");

            var existingRack = await _context.Racks.FindAsync(id);

            if (existingRack == null)
                return NotFound();

            bool codeExists = await _context.Racks.AnyAsync(x =>
                x.Id != id &&
                !x.IsDeleted &&
                x.RackCode != null &&
                x.RackCode.ToLower() == rack.RackCode.ToLower());

            if (codeExists)
                return BadRequest("Rack Code already exists");

            existingRack.RackCode = rack.RackCode;
            existingRack.RackName = rack.RackName;
            existingRack.ShelfId = rack.ShelfId;
            existingRack.EditedDate = DateTime.Now;
            existingRack.IsEdited = true;

            await _context.SaveChangesAsync();

            return Ok(existingRack);
        }

        // DELETE: api/Racks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRack(int id)
        {
            var rack = await _context.Racks.FindAsync(id);

            if (rack == null)
                return NotFound();

            rack.IsDeleted = true;
            rack.DeletedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok("Rack deleted successfully");
        }
    }
}