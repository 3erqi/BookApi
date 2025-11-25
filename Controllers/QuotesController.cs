using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookApi.Data;
using BookApi.Models;
using BookApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class QuotesController : ControllerBase
{
    private readonly AppDbContext _context;

    public QuotesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Quotes - Get all quotes for the authenticated user
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuoteResponseDto>>> GetQuotes()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var quotes = await _context.Quotes
            .Where(q => q.UserId == userId)
            .Select(q => new QuoteResponseDto
            {
                Id = q.Id,
                Text = q.Text,
                Author = q.Author,
                UserId = q.UserId
            })
            .ToListAsync();

        return Ok(quotes);
    }

    // GET: api/Quotes/{id} - Get a specific quote for the authenticated user
    [HttpGet("{id}")]
    public async Task<ActionResult<QuoteResponseDto>> GetQuote(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var quote = await _context.Quotes
            .Where(q => q.Id == id && q.UserId == userId)
            .Select(q => new QuoteResponseDto
            {
                Id = q.Id,
                Text = q.Text,
                Author = q.Author,
                UserId = q.UserId
            })
            .FirstOrDefaultAsync();

        if (quote == null)
            return NotFound();

        return Ok(quote);
    }

    // POST: api/Quotes - Create a new quote for the authenticated user
    [HttpPost]
    public async Task<ActionResult<QuoteResponseDto>> CreateQuote(CreateQuoteDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var quote = new Quote
        {
            Text = dto.Text,
            Author = dto.Author,
            UserId = userId
        };

        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        var responseDto = new QuoteResponseDto
        {
            Id = quote.Id,
            Text = quote.Text,
            Author = quote.Author,
            UserId = quote.UserId
        };

        return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, responseDto);
    }

    // PUT: api/Quotes/{id} - Update a quote for the authenticated user
    [HttpPut("{id}")]
    public async Task<ActionResult<QuoteResponseDto>> UpdateQuote(int id, UpdateQuoteDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var quote = await _context.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

        if (quote == null)
            return NotFound();

        quote.Text = dto.Text;
        quote.Author = dto.Author;

        _context.Entry(quote).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        var responseDto = new QuoteResponseDto
        {
            Id = quote.Id,
            Text = quote.Text,
            Author = quote.Author,
            UserId = quote.UserId
        };

        return Ok(responseDto);
    }

    // DELETE: api/Quotes/{id} - Delete a quote for the authenticated user
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuote(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var quote = await _context.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

        if (quote == null)
            return NotFound();

        _context.Quotes.Remove(quote);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
