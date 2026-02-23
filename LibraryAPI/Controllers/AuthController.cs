using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Models;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers;

/// <summary>
/// Lietotāju autentifikācija un reģistrācija
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly LibraryDbContext _db;
    private readonly IJwtService _jwt;

    public AuthController(LibraryDbContext db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    /// <summary>
    /// Reģistrē jaunu lietotāju sistēmā
    /// </summary>
    /// <param name="dto">Reģistrācijas dati: vārds, e-pasts, parole</param>
    /// <returns>JWT tokens un lietotāja informācija</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Visi lauki ir obligāti." });

        if (dto.Password.Length < 6)
            return BadRequest(new { message = "Parolei jābūt vismaz 6 rakstzīmēm." });

        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { message = "Šis e-pasts jau ir reģistrēts." });

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwt.GenerateToken(user);
        return Ok(new AuthResponseDto(token, user.Name, user.Email, user.Role));
    }

    /// <summary>
    /// Autorizē lietotāju un atgriež JWT tokenu
    /// </summary>
    /// <param name="dto">E-pasts un parole</param>
    /// <returns>JWT tokens derīgs 24 stundas</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Nepareizs e-pasts vai parole." });

        var token = _jwt.GenerateToken(user);
        return Ok(new AuthResponseDto(token, user.Name, user.Email, user.Role));
    }
}
