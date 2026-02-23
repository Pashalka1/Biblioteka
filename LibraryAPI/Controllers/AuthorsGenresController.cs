using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers;

/// <summary>
/// Autoru pārvaldība
/// </summary>
[ApiController]
[Route("api/authors")]
[Produces("application/json")]
public class AuthorsController : ControllerBase
{
    private readonly LibraryDbContext _db;
    public AuthorsController(LibraryDbContext db) => _db = db;

    /// <summary>
    /// Atgriež visu autoru sarakstu ar grāmatu skaitu
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AuthorResponseDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var authors = await _db.Authors
            .Select(a => new AuthorResponseDto(
                a.Id, a.FullName, a.Country, a.BirthYear, a.Books.Count))
            .ToListAsync();
        return Ok(authors);
    }

    /// <summary>
    /// Atgriež autoru pēc ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuthorResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var author = await _db.Authors
            .Where(a => a.Id == id)
            .Select(a => new AuthorResponseDto(a.Id, a.FullName, a.Country, a.BirthYear, a.Books.Count))
            .FirstOrDefaultAsync();

        if (author == null) return NotFound(new { message = "Autors nav atrasts." });
        return Ok(author);
    }

    /// <summary>
    /// Pievieno jaunu autoru (tikai Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AuthorResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] AuthorCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
            return BadRequest(new { message = "Vārds ir obligāts." });

        var author = new Author
        {
            FullName = dto.FullName,
            Country = dto.Country,
            BirthYear = dto.BirthYear
        };

        _db.Authors.Add(author);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = author.Id },
            new AuthorResponseDto(author.Id, author.FullName, author.Country, author.BirthYear, 0));
    }
}

/// <summary>
/// Žanru pārvaldība
/// </summary>
[ApiController]
[Route("api/genres")]
[Produces("application/json")]
public class GenresController : ControllerBase
{
    private readonly LibraryDbContext _db;
    public GenresController(LibraryDbContext db) => _db = db;

    /// <summary>
    /// Atgriež visus žanrus ar grāmatu skaitu
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<GenreResponseDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var genres = await _db.Genres
            .Select(g => new GenreResponseDto(g.Id, g.Name, g.Description, g.Books.Count))
            .ToListAsync();
        return Ok(genres);
    }

    /// <summary>
    /// Pievieno jaunu žanru (tikai Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GenreResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] GenreCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Nosaukums ir obligāts." });

        if (await _db.Genres.AnyAsync(g => g.Name == dto.Name))
            return BadRequest(new { message = "Žanrs ar šādu nosaukumu jau eksistē." });

        var genre = new Genre { Name = dto.Name, Description = dto.Description };
        _db.Genres.Add(genre);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = genre.Id },
            new GenreResponseDto(genre.Id, genre.Name, genre.Description, 0));
    }
}
