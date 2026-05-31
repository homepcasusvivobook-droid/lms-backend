using LMS.API.Data;
using LMS.API.DTOs;
using LMS.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMembers()
        {
            var today = DateTime.Today;

            var members = await _context.Members
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.MemberId)
                .Select(m => new
                {
                    m.Id,
                    m.CardexNo,
                    m.MemberName,
                    m.PhoneNo,
                    m.Email,
                    m.Address,
                    m.IsActive,
                    CreatedDate = (DateTime?)m.CreatedDate,
                    m.MemberId,
                    m.Parish,
                    m.CreatedBy,
                    m.MemberTypeId,
                    MemberTypeName = m.MemberType != null ? m.MemberType.MemberTypeName : "",
                    MaxBooksAllowed = m.MemberType != null ? m.MemberType.MaxBooksAllowed : 0,

                    MembershipStatus =
                        _context.MembershipRenewals.Any(r => r.MemberDbId == m.Id && !r.IsDeleted)
                            ? _context.MembershipRenewals
                                .Where(r => r.MemberDbId == m.Id && !r.IsDeleted)
                                .OrderByDescending(r => r.ValidTo)
                                .Select(r => r.ValidTo >= today ? "Active" : "Expired")
                                .FirstOrDefault()
                            : "Not Renewed",

                    ExpiredOn = _context.MembershipRenewals
                        .Where(r => r.MemberDbId == m.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.ValidTo)
                        .Select(r => (DateTime?)r.ValidTo)
                        .FirstOrDefault(),

                    Amount = _context.MembershipRenewals
                        .Where(r => r.MemberDbId == m.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.ValidTo)
                        .Select(r => (decimal?)r.Amount)
                        .FirstOrDefault(),

                    PaymentDate = _context.MembershipRenewals
                        .Where(r => r.MemberDbId == m.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.ValidTo)
                        .Select(r => (DateTime?)r.PaymentDate)
                        .FirstOrDefault(),

                    ValidFrom = _context.MembershipRenewals
                        .Where(r => r.MemberDbId == m.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.ValidTo)
                        .Select(r => (DateTime?)r.ValidFrom)
                        .FirstOrDefault(),

                    ValidTo = _context.MembershipRenewals
                        .Where(r => r.MemberDbId == m.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.ValidTo)
                        .Select(r => (DateTime?)r.ValidTo)
                        .FirstOrDefault(),

                    Remarks = _context.MembershipRenewals
                        .Where(r => r.MemberDbId == m.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.ValidTo)
                        .Select(r => r.Remarks)
                        .FirstOrDefault(),

                    RenewalCreatedDate = _context.MembershipRenewals
                        .Where(r => r.MemberDbId == m.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.ValidTo)
                        .Select(r => (DateTime?)r.CreatedDate)
                        .FirstOrDefault(),

                    RenewalCreatedBy = _context.MembershipRenewals
                        .Where(r => r.MemberDbId == m.Id && !r.IsDeleted)
                        .OrderByDescending(r => r.ValidTo)
                        .Select(r => r.CreatedBy)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(members);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMember(int id)
        {
            var member = await _context.Members
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    x.MemberId,
                    x.CardexNo,
                    x.MemberName,
                    x.PhoneNo,
                    x.Email,
                    x.Address,
                    x.Parish,
                    x.IsActive,
                    x.CreatedDate,
                    x.CreatedBy,
                    x.MemberTypeId,
                    MemberTypeName = x.MemberType != null ? x.MemberType.MemberTypeName : "",
                    MaxBooksAllowed = x.MemberType != null ? x.MemberType.MaxBooksAllowed : 0
                })
                .FirstOrDefaultAsync();

            if (member == null)
                return NotFound();

            return Ok(member);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMember(Member member)
        {
            if (member.MemberTypeId == null || member.MemberTypeId <= 0)
                return BadRequest("Member Type is required.");

            var memberType = await _context.MemberTypes
                .FirstOrDefaultAsync(x => x.Id == member.MemberTypeId && !x.IsDeleted && x.IsActive);

            if (memberType == null)
                return BadRequest("Invalid Member Type.");

            if (string.IsNullOrWhiteSpace(memberType.Prefix))
                return BadRequest("Member Type Prefix is missing.");

            var prefix = memberType.Prefix.Trim().ToUpper();

            var lastSerial = await _context.Members
                .Where(x =>
                    !x.IsDeleted &&
                    x.MemberTypeId == member.MemberTypeId &&
                    x.MemberTypePrefix == prefix)
                .OrderByDescending(x => x.MemberSerialNo)
                .Select(x => x.MemberSerialNo)
                .FirstOrDefaultAsync();

            var nextSerial = lastSerial + 1;

            member.MemberTypePrefix = prefix;
            member.MemberSerialNo = nextSerial;
            member.MemberId = prefix + nextSerial.ToString("D3");

            member.CreatedDate = DateTime.Now;
            member.CreatedBy = member.CreatedBy ?? "Admin";

            member.IsActive = true;
            member.IsDeleted = false;
            member.IsEdited = false;

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return Ok(member);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMember(int id, Member member)
        {
            if (id != member.Id)
                return BadRequest();

            var existingMember = await _context.Members.FindAsync(id);

            if (existingMember == null || existingMember.IsDeleted)
                return NotFound();

            existingMember.CardexNo = member.CardexNo;
            existingMember.MemberName = member.MemberName;
            existingMember.PhoneNo = member.PhoneNo;
            existingMember.Email = member.Email;
            existingMember.Address = member.Address;
            existingMember.Parish = member.Parish;

            existingMember.IsEdited = true;
            existingMember.EditedDate = DateTime.Now;
            existingMember.EditedBy = member.EditedBy;

            await _context.SaveChangesAsync();

            return Ok(existingMember);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(int id, string? deletedBy)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null || member.IsDeleted)
                return NotFound();

            member.IsDeleted = true;
            member.IsActive = false;
            member.DeletedDate = DateTime.Now;
            member.DeletedBy = deletedBy ?? "Admin";

            await _context.SaveChangesAsync();

            return Ok("Member deleted successfully.");
        }

        [HttpPost("renewal")]
        public async Task<IActionResult> RenewMembership(MembershipRenewalDto dto)
        {
            var member = await _context.Members.FindAsync(dto.MemberDbId);

            if (member == null || member.IsDeleted)
                return NotFound("Member not found.");

            var paymentDate = dto.PaymentDate == default
                ? DateTime.Today
                : dto.PaymentDate.Date;

            var renewal = new MembershipRenewal
            {
                MemberDbId = dto.MemberDbId,
                Amount = dto.Amount,
                PaymentDate = paymentDate,
                ValidFrom = paymentDate,
                ValidTo = new DateTime(paymentDate.Year, 12, 31),
                Remarks = dto.Remarks,
                CreatedBy = dto.CreatedBy ?? "Admin",
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false
            };

            _context.MembershipRenewals.Add(renewal);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Membership renewed successfully.",
                renewal.Id,
                renewal.MemberDbId,
                renewal.Amount,
                renewal.PaymentDate,
                renewal.ValidFrom,
                renewal.ValidTo
            });
        }

        [HttpGet("{id}/renewals")]
        public async Task<IActionResult> GetMemberRenewals(int id)
        {
            var renewals = await _context.MembershipRenewals
                .Where(x => x.MemberDbId == id && !x.IsDeleted)
                .OrderByDescending(x => x.ValidTo)
                .Select(x => new
                {
                    x.Id,
                    x.Amount,
                    x.PaymentDate,
                    x.ValidFrom,
                    x.ValidTo,
                    x.Remarks,
                    x.CreatedBy,
                    x.CreatedDate
                })
                .ToListAsync();

            return Ok(renewals);
        }
    }
}