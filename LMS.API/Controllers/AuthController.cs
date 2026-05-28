using LMS.API.Data;
using LMS.API.DTOs;
using LMS.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var today = DateTime.Now.Date;

            var user = await _context.Users
                .Include(x => x.UserType)
                .FirstOrDefaultAsync(x =>
                    x.Username.ToLower() == dto.Username.ToLower() &&
                    x.IsActive &&
                    !x.IsDeleted &&
                    x.ValidityFrom.HasValue &&
                    x.ValidityTo.HasValue &&
                    today >= x.ValidityFrom.Value.Date &&
                    today <= x.ValidityTo.Value.Date);

            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password
            );

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid username or password");
            }

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Username,
                user.UserTypeId,
                UserTypeName = user.UserType != null ? user.UserType.UserTypeName : "",
                CanCreate = user.UserType != null && user.UserType.CanCreate,
                CanView = user.UserType != null && user.UserType.CanView,
                CanEdit = user.UserType != null && user.UserType.CanEdit,
                CanDelete = user.UserType != null && user.UserType.CanDelete
            });
        }
    }
}