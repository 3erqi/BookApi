using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookApi.Data;
using BookApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly AppDbContext _context;

    public BooksController(AppDbContext context)
    {
        _context = context;
    }

    //GET: api/Books
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        return await _context.Books
            .Where(b => b.UserId == userId)
            .ToListAsync();
    }

    //GET: api/Books/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        if (book == null)
        {
            return NotFound();
        }

        return book;
    }

    //POST: api/Books
    [HttpPost]
    public async Task<ActionResult<Book>> CreateBook(Book book)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        book.UserId = userId;
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    //PUT api/Books/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(int id, Book updatedBook)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        Console.WriteLine($"PUT /api/books/{id} - Received book ID: {updatedBook.Id}, URL ID: {id}");
        
        if(id != updatedBook.Id)
        {
            return BadRequest($"URL ID ({id}) does not match book ID ({updatedBook.Id})");
        }

        var existingBook = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        
        if (existingBook == null)
        {
            return NotFound();
        }

        existingBook.Title = updatedBook.Title;
        existingBook.Author = updatedBook.Author;
        existingBook.PublishedDate = updatedBook.PublishedDate;
        existingBook.Genre = updatedBook.Genre;
        existingBook.Isbn = updatedBook.Isbn;

        await _context.SaveChangesAsync();
        return Ok(existingBook);
    }

    //Delete: api/Books/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        if(book == null)
        {
            return NotFound();
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/Books/all - Get all books in the database (public view)
    [HttpGet("all")]
    [AllowAnonymous] // Allow access without authentication
    public async Task<ActionResult<IEnumerable<object>>> GetAllBooks()
    {
        var books = await _context.Books
            .Include(b => b.User) // Include user information
            .Select(b => new {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                PublishedDate = b.PublishedDate,
                Genre = b.Genre,
                Isbn = b.Isbn,
                AddedBy = b.User != null ? b.User.Username : "Unknown" // Include who added the book
            })
            .ToListAsync();

        return Ok(books);
    }

}