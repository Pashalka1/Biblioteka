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

    [HttpPut("{id}")]
    [Authorize(Roles = "Librarian,Admin")]
    [ProducesResponseType(typeof(BookDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] CreateBookRequest req)
    {
        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (book == null)
            return NotFound(new { message = "Grāmata nav atrasta." });

        if (!await _db.Authors.AnyAsync(a => a.Id == req.AuthorId))
            return BadRequest(new { message = "Autors nav atrasts." });

        if (!await _db.Categories.AnyAsync(c => c.Id == req.CategoryId))
            return BadRequest(new { message = "Kategorija nav atrasta." });

        var activeLoans = await _db.Loans.CountAsync(l => l.BookId == id && l.Status == "Active");
        if (req.TotalCopies < activeLoans)
            return BadRequest(new { message = $"Nevar iestatīt TotalCopies={req.TotalCopies}, jo ir {activeLoans} aktīvi aizdevumi." });

        book.Title = req.Title;
        book.ISBN = req.ISBN;
        book.PublishedYear = req.PublishedYear;
        book.Description = req.Description;
        book.AuthorId = req.AuthorId;
        book.CategoryId = req.CategoryId;
        book.TotalCopies = req.TotalCopies;
        book.AvailableCopies = req.TotalCopies - activeLoans;

        await _db.SaveChangesAsync();
        await _db.Entry(book).Reference(b => b.Author).LoadAsync();
        await _db.Entry(book).Reference(b => b.Category).LoadAsync();

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

    [HttpDelete("{id}")]
    [Authorize(Roles = "Librarian,Admin")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (book == null)
            return NotFound(new { message = "Grāmata nav atrasta." });

        var hasActive = await _db.Loans.AnyAsync(l => l.BookId == id && l.Status == "Active");
        if (hasActive)
            return BadRequest(new { message = "Grāmatu nevar dzēst, jo ir aktīvi aizdevumi." });

        _db.Books.Remove(book);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Grāmata dzēsta." });
    }
}
