using LMS.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserTypes()
        {
            var data = await _context.UserTypes
                .Where(x => !x.IsDeleted && x.IsActive)
                .OrderBy(x => x.UserTypeName)
                .Select(x => new
                {
                    x.Id,
                    x.UserTypeName,
                    x.CanCreate,
                    x.CanView,
                    x.CanEdit,
                    x.CanDelete
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}