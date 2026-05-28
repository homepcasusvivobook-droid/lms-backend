using LMS.API.Data;
using LMS.API.DTOs;
using LMS.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembershipRenewalsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MembershipRenewalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{memberId}")]
        public async Task<IActionResult> GetRenewals(int memberId)
        {
            var data = await _context.MembershipRenewals
                .Where(x => x.MemberDbId == memberId && !x.IsDeleted)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> SaveRenewal(MembershipRenewalDto dto)
        {
            var renewal = new MembershipRenewal
            {
                MemberDbId = dto.MemberDbId,
                Amount = dto.Amount,
                PaymentDate = DateTime.Now,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                Remarks = dto.Remarks,
                CreatedBy = "Admin",
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false
            };

            _context.MembershipRenewals.Add(renewal);

            await _context.SaveChangesAsync();

            return Ok(renewal);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRenewal(int id, MembershipRenewalDto dto)
        {
            var renewal = await _context.MembershipRenewals.FindAsync(id);

            if (renewal == null)
                return NotFound();

            renewal.Amount = dto.Amount;
            renewal.ValidFrom = dto.ValidFrom;
            renewal.ValidTo = dto.ValidTo;
            renewal.Remarks = dto.Remarks;

            renewal.EditedBy = "Admin";
            renewal.EditedDate = DateTime.Now;
            renewal.IsEdited = true;

            await _context.SaveChangesAsync();

            return Ok(renewal);
        }
    }
}