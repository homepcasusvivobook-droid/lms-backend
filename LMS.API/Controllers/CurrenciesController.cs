using LMS.API.Data;
using LMS.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrenciesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CurrenciesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/currencies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Currency>>> GetCurrencies()
        {
            return await _context.Currencies
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.CurrencyCode)
                .ToListAsync();
        }

        // GET: api/currencies/default
        [HttpGet("default")]
        public async Task<ActionResult<Currency>> GetDefaultCurrency()
        {
            var currency = await _context.Currencies
                .FirstOrDefaultAsync(x => x.IsDefault && !x.IsDeleted);

            if (currency == null)
                return NotFound();

            return currency;
        }

        // GET: api/currencies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Currency>> GetCurrency(int id)
        {
            var currency = await _context.Currencies
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (currency == null)
                return NotFound();

            return currency;
        }

        // POST: api/currencies
        [HttpPost]
        public async Task<ActionResult<Currency>> AddCurrency(Currency currency)
        {
            currency.CreatedDate = DateTime.Now;
            currency.IsActive = true;
            currency.IsDeleted = false;

            if (currency.IsDefault)
            {
                var existingDefaults = await _context.Currencies
                    .Where(x => x.IsDefault)
                    .ToListAsync();

                foreach (var item in existingDefaults)
                    item.IsDefault = false;
            }

            _context.Currencies.Add(currency);
            await _context.SaveChangesAsync();

            return Ok(currency);
        }

        // PUT: api/currencies/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCurrency(int id, Currency currency)
        {
            var existing = await _context.Currencies.FindAsync(id);

            if (existing == null)
                return NotFound();

            existing.CurrencyCode = currency.CurrencyCode;
            existing.CurrencyName = currency.CurrencyName;
            existing.IsActive = currency.IsActive;
            existing.EditedDate = DateTime.Now;
            existing.EditedBy = currency.EditedBy;

            if (currency.IsDefault)
            {
                var existingDefaults = await _context.Currencies
                    .Where(x => x.IsDefault)
                    .ToListAsync();

                foreach (var item in existingDefaults)
                    item.IsDefault = false;

                existing.IsDefault = true;
            }

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // DELETE: api/currencies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCurrency(
            int id,
            [FromQuery] string deletedBy)
        {
            var currency = await _context.Currencies.FindAsync(id);

            if (currency == null)
                return NotFound();

            currency.IsDeleted = true;
            currency.IsActive = false;
            currency.DeletedDate = DateTime.Now;
            currency.DeletedBy = deletedBy;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}