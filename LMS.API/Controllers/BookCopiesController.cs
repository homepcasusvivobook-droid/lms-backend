using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.API.Models;
using LMS.API.Data;

[Route("api/[controller]")]
[ApiController]
public class BookCopiesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public BookCopiesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/BookCopy
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookCopy>>> GetBookCopy()
    {
        return await _context.BookCopies.ToListAsync();
    }

    // GET: api/BookCopy/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BookCopy>> GetBookCopy(int id)
    {
        var bookcopy = await _context.BookCopies.FindAsync(id);

        if (bookcopy == null)
        {
            return NotFound();
        }

        return bookcopy;
    }

    // PUT: api/BookCopy/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBookCopy(int? id, BookCopy bookcopy)
    {
        if (id != bookcopy.Id)
        {
            return BadRequest();
        }

        _context.Entry(bookcopy).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BookCopyExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/BookCopy
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<BookCopy>> PostBookCopy(BookCopy bookcopy)
    {
        _context.BookCopies.Add(bookcopy);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetBookCopy", new { id = bookcopy.Id }, bookcopy);
    }

    // DELETE: api/BookCopy/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBookCopy(int? id)
    {
        var bookcopy = await _context.BookCopies.FindAsync(id);
        if (bookcopy == null)
        {
            return NotFound();
        }

        _context.BookCopies.Remove(bookcopy);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BookCopyExists(int? id)
    {
        return _context.BookCopies.Any(e => e.Id == id);
    }
}
