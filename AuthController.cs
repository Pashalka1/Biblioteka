using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Models;
using LibraryAPI.Services;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly LibraryDbContext _db;
    private readonly JwtService _jwt;

    public AuthController(LibraryDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    /// <summary>Lietotāja reģistrācija</summary>
    /// <remarks>
    /// Izveido jaunu lietotāja kontu ar lomu "Reader". Piemērs:
    ///
    ///     POST /api/auth/register
    ///     {
    ///         "name": "Jānis Bērziņš",
    ///         "email": "janis@library.lv",
    ///         "password": "Parole123!"
    ///     }
    ///
    /// Atgriež JWT tokenu, ko izmanto autorizācijai.
    /// </remarks>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return BadRequest(new { message = "E-pasts jau reģistrēts." });

        var user = new User
        {
            Name = req.Name,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = "Reader"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwt.GenerateToken(user);
        return CreatedAtAction(nameof(Register), new AuthResponse(token, user.Name, user.Email, user.Role));
    }

    /// <summary>Lietotāja pieslēgšanās</summary>
    /// <remarks>
    /// Atgriež JWT tokenu veiksmīgas autentifikācijas gadījumā. Piemērs:
    ///
    ///     POST /api/auth/login
    ///     {
    ///         "email": "admin@library.lv",
    ///         "password": "Admin123!"
    ///     }
    ///
    /// Saņemto tokenu izmanto Swagger UI — nospied "Authorize" un ievadi: Bearer {tokens}
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Nepareizs e-pasts vai parole." });

        var token = _jwt.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Name, user.Email, user.Role));
    }
}
