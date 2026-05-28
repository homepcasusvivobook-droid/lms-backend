using LMS.API.Data;
using LMS.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MemberTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/MemberTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberType>>> GetMemberTypes()
        {
            return await _context.MemberTypes
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        // GET: api/MemberTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MemberType>> GetMemberType(int id)
        {
            var memberType = await _context.MemberTypes.FindAsync(id);

            if (memberType == null || memberType.IsDeleted)
            {
                return NotFound();
            }

            return memberType;
        }

        // POST: api/MemberTypes
        [HttpPost]
        public async Task<ActionResult<MemberType>> PostMemberType(MemberType memberType)
        {
            memberType.CreatedDate = DateTime.Now;
            memberType.CreatedBy = "System";
            memberType.IsDeleted = false;

            _context.MemberTypes.Add(memberType);
            await _context.SaveChangesAsync();

            return Ok(memberType);
        }

        // PUT: api/MemberTypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMemberType(int id, MemberType memberType)
        {
            if (id != memberType.Id)
            {
                return BadRequest();
            }

            var existing = await _context.MemberTypes.FindAsync(id);

            if (existing == null)
            {
                return NotFound();
            }

            existing.MemberTypeName = memberType.MemberTypeName;
            existing.MaxBooksAllowed = memberType.MaxBooksAllowed;
            existing.IsActive = memberType.IsActive;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // DELETE: api/MemberTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMemberType(int id)
        {
            var memberType = await _context.MemberTypes.FindAsync(id);

            if (memberType == null)
            {
                return NotFound();
            }

            memberType.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Deleted successfully"
            });
        }
    }
}