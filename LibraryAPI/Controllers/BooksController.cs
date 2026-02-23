using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers;

/// <summary>
/// Grāmatu pārvaldība bibliotēkas sistēmā
/// </summary>
[ApiController]
[Route("api/books")]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    private readonly LibraryDbContext _db;

    public BooksController(LibraryDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Atgriež visu grāmatu sarakstu ar autoru un žanra informāciju
    /// </summary>
    /// <param name="genre">Filtrēt pēc žanra nosaukuma (neobligāts)</param>
    /// <param name="available">Rādīt tikai pieejamās grāmatas (neobligāts)</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<BookResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? genre, [FromQuery] bool? available)
    {
        var query = _db.Books
            .Include(b => b.Author)
            .Include(b => b.Genre)
            .AsQueryable();

        if (!string.IsNullOrEmpty(genre))
            query = query.Where(b => b.Genre.Name.Contains(genre));

        if (available == true)
            query = query.Where(b => b.AvailableCopies > 0);

        var books = await query
            .Select(b => new BookResponseDto(
                b.Id, b.Title, b.ISBN, b.PublishedYear,
                b.AvailableCopies, b.TotalCopies,
                b.Author.FullName, b.Genre.Name))
            .ToListAsync();

        return Ok(books);
    }

    /// <summary>
    /// Atgriež vienu grāmatu pēc ID
    /// </summary>
    /// <param name="id">Grāmatas ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var book = await _db.Books
            .Include(b => b.Author)
            .Include(b => b.Genre)
            .Where(b => b.Id == id)
            .Select(b => new BookResponseDto(
                b.Id, b.Title, b.ISBN, b.PublishedYear,
                b.AvailableCopies, b.TotalCopies,
                b.Author.FullName, b.Genre.Name))
            .FirstOrDefaultAsync();

        if (book == null)
            return NotFound(new { message = $"Grāmata ar ID {id} nav atrasta." });

        return Ok(book);
    }

    /// <summary>
    /// Pievieno jaunu grāmatu (tikai Admin)
    /// </summary>
    /// <param name="dto">Grāmatas dati: nosaukums, ISBN, gads, autors, žanrs</param>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BookResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] BookCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Nosaukums ir obligāts." });

        if (await _db.Books.AnyAsync(b => b.ISBN == dto.ISBN))
            return BadRequest(new { message = "Grāmata ar šādu ISBN jau eksistē." });

        if (!await _db.Authors.AnyAsync(a => a.Id == dto.AuthorId))
            return BadRequest(new { message = "Autors nav atrasts." });

        if (!await _db.Genres.AnyAsync(g => g.Id == dto.GenreId))
            return BadRequest(new { message = "Žanrs nav atrasts." });

        var book = new Book
        {
            Title = dto.Title,
            ISBN = dto.ISBN,
            PublishedYear = dto.PublishedYear,
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.TotalCopies,
            AuthorId = dto.AuthorId,
            GenreId = dto.GenreId
        };

        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        await _db.Entry(book).Reference(b => b.Author).LoadAsync();
        await _db.Entry(book).Reference(b => b.Genre).LoadAsync();

        var response = new BookResponseDto(
            book.Id, book.Title, book.ISBN, book.PublishedYear,
            book.AvailableCopies, book.TotalCopies,
            book.Author.FullName, book.Genre.Name);

        return CreatedAtAction(nameof(GetById), new { id = book.Id }, response);
    }
}
