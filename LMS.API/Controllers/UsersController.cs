using LMS.API.Data;
using LMS.API.DTOs;
using LMS.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Where(x => !x.IsDeleted)
                .Include(x => x.UserType)
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.FullName,
                    x.Username,

                    // hide password
                    Password = "********",

                    x.UserTypeId,
                    UserTypeName = x.UserType != null ? x.UserType.UserTypeName : "",
                    x.ValidityFrom,
                    x.ValidityTo,
                    x.IsActive,
                    x.CreatedDate,
                    x.CreatedBy,

                    CanCreate = x.UserType != null && x.UserType.CanCreate,
                    CanView = x.UserType != null && x.UserType.CanView,
                    CanEdit = x.UserType != null && x.UserType.CanEdit,
                    CanDelete = x.UserType != null && x.UserType.CanDelete
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(AddUserDto dto)
        {
            var exists = await _context.Users
                .AnyAsync(x => x.Username == dto.Username && !x.IsDeleted);

            if (exists)
                return BadRequest("Username already exists.");

            var userTypeExists = await _context.UserTypes
                .AnyAsync(x => x.Id == dto.UserTypeId && !x.IsDeleted && x.IsActive);

            if (!userTypeExists)
                return BadRequest("Invalid user type.");

            var user = new User
            {
                FullName = dto.FullName,
                Username = dto.Username,
                UserTypeId = dto.UserTypeId,
                ValidityFrom = dto.ValidityFrom,
                ValidityTo = dto.ValidityTo,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                CreatedBy = dto.CreatedBy
            };

            // HASH PASSWORD
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "User created successfully.",
                user.Id,
                user.FullName,
                user.Username
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, AddUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.IsDeleted)
                return NotFound();

            var duplicate = await _context.Users
                .AnyAsync(x => x.Username == dto.Username && x.Id != id && !x.IsDeleted);

            if (duplicate)
                return BadRequest("Username already exists.");

            user.FullName = dto.FullName;
            user.Username = dto.Username;
            user.UserTypeId = dto.UserTypeId;
            user.ValidityFrom = dto.ValidityFrom;
            user.ValidityTo = dto.ValidityTo;
            user.EditedDate = DateTime.Now;
            user.EditedBy = dto.CreatedBy;

            // update password only if changed
            if (!string.IsNullOrWhiteSpace(dto.Password) &&
                dto.Password != "********")
            {
                user.PasswordHash =
                    _passwordHasher.HashPassword(user, dto.Password);
            }

            await _context.SaveChangesAsync();

            return Ok("User updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(
            int id,
            string? deletedBy)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.IsDeleted)
                return NotFound();

            user.IsDeleted = true;
            user.IsActive = false;
            user.DeletedDate = DateTime.Now;
            user.DeletedBy = deletedBy;

            await _context.SaveChangesAsync();

            return Ok("User deleted successfully.");
        }

        // TEMP PASSWORD RESET API
        [HttpPost("reset-password/{id}")]
        public async Task<IActionResult> ResetPassword(
            int id,
            string newPassword)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.IsDeleted)
                return NotFound();

            user.PasswordHash =
                _passwordHasher.HashPassword(user, newPassword);

            user.EditedDate = DateTime.Now;
            user.EditedBy = "System Password Reset";

            await _context.SaveChangesAsync();

            return Ok("Password reset successfully.");
        }
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Username == dto.Username &&
                    x.IsActive &&
                    !x.IsDeleted);

            if (user == null)
                return NotFound("User not found.");

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.OldPassword
            );

            if (result == PasswordVerificationResult.Failed)
                return BadRequest("Old password is incorrect.");

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            user.EditedDate = DateTime.Now;
            user.EditedBy = dto.Username;

            await _context.SaveChangesAsync();

            return Ok("Password changed successfully.");
        }
    }
}