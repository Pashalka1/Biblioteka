using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Models;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/books")]
[Authorize]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    private readonly LibraryDbContext _db;

    public BooksController(LibraryDbContext db) => _db = db;

    /// <summary>Iegūst visu grāmatu sarakstu</summary>
    /// <remarks>
    /// Atgriež visas grāmatas ar autora un kategorijas informāciju.
    /// Var filtrēt pēc nosaukuma/autora un kategorijas:
    ///
    ///     GET /api/books
    ///     GET /api/books?search=Rainis
    ///     GET /api/books?categoryId=1
    ///     GET /api/books?search=uguns&amp;categoryId=1
    ///
    /// Pieprasa autentifikāciju (Bearer tokens).
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BookDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? categoryId)
    {
        var query = _db.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => b.Title.Contains(search) ||
                                     b.Author.FirstName.Contains(search) ||
                                     b.Author.LastName.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(b => b.CategoryId == categoryId);

        var books = await query.Select(b => new BookDto
        {
            Id = b.Id,
            Title = b.Title,
            ISBN = b.ISBN,
            PublishedYear = b.PublishedYear,
            AvailableCopies = b.AvailableCopies,
            TotalCopies = b.TotalCopies,
            Description = b.Description,
            AuthorName = b.Author.FirstName + " " + b.Author.LastName,
            CategoryName = b.Category.Name
        }).ToListAsync();

        return Ok(books);
    }

    /// <summary>Iegūst grāmatu pēc ID</summary>
    /// <remarks>
    /// Atgriež konkrētu grāmatu ar pilnu informāciju. Piemērs:
    ///
    ///     GET /api/books/1
    ///
    /// Pieprasa autentifikāciju (Bearer tokens).
    /// </remarks>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var book = await _db.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
            return NotFound(new { message = "Grāmata nav atrasta." });

        return Ok(new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            ISBN = book.ISBN,
            PublishedYear = book.PublishedYear,
            AvailableCopies = book.AvailableCopies,
            TotalCopies = book.TotalCopies,
            Description = book.Description,
            AuthorName = book.Author.FirstName + " " + book.Author.LastName,
            CategoryName = book.Category.Name
        });
    }

    /// <summary>Pievieno jaunu grāmatu</summary>
    /// <remarks>
    /// Pieprasa Librarian vai Admin lomu. Piemērs:
    ///
    ///     POST /api/books
    ///     {
    ///         "title": "Uguns un nakts",
    ///         "isbn": "978-9984-00-001-1",
    ///         "publishedYear": 1905,
    ///         "totalCopies": 3,
    ///         "description": "Leģendārā Raiņa luga",
    ///         "authorId": 1,
    ///         "categoryId": 1
    ///     }
    ///
    /// Validācija: nosaukums obligāts, gads 1000–2100, eksemplāri 1–1000.
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Librarian,Admin")]
    [ProducesResponseType(typeof(BookDto), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] CreateBookRequest req)
    {
        if (!await _db.Authors.AnyAsync(a => a.Id == req.AuthorId))
            return BadRequest(new { message = "Autors nav atrasts." });

        if (!await _db.Categories.AnyAsync(c => c.Id == req.CategoryId))
            return BadRequest(new { message = "Kategorija nav atrasta." });

        var book = new Book
        {
            Title = req.Title,
            ISBN = req.ISBN,
            PublishedYear = req.PublishedYear,
            TotalCopies = req.TotalCopies,
            AvailableCopies = req.TotalCopies,
            Description = req.Description,
            AuthorId = req.AuthorId,
            CategoryId = req.CategoryId
        };

        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        await _db.Entry(book).Reference(b => b.Author).LoadAsync();
        await _db.Entry(book).Reference(b => b.Category).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = book.Id }, new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            ISBN = book.ISBN,
            PublishedYear = book.PublishedYear,
            AvailableCopies = book.AvailableCopies,
            TotalCopies = book.TotalCopies,
            Description = book.Description,
            AuthorName = book.Author.FirstName + " " + book.Author.LastName,
            CategoryName = book.Category.Name
        });
    }
}
