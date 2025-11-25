using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookApi.Data;
using BookApi.Models;
using BookApi.DTOs;
using BookApi.Services;

namespace BookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AuthService _authService;

    public AuthController(AppDbContext context, AuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<object>> Register(UserRegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email already exists.");

        // Use BCrypt hash only
        var hash = _authService.CreatePasswordHash(dto.Password);

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = hash
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate JWT token for the new user
        var token = _authService.CreateToken(user);

        return Ok(new
        {
            token = token,
            user = new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email
            }
        });
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<object>> Login(UserLoginDto dto)
    {
        // Try to find user by username or email
        var user = await _context.Users.FirstOrDefaultAsync(x => 
            x.Username == dto.Username || x.Email == dto.Email);

        if (user == null)
            return BadRequest("User not found");

        if (!_authService.VerifyPassword(dto.Password, user.PasswordHash))
            return BadRequest("Incorrect password");

        var token = _authService.CreateToken(user);

        return Ok(new
        {
            token = token,
            user = new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email
            }
        });
    }
}
