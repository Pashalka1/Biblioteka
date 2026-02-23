using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Models;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/authors")]
[Authorize]
[Produces("application/json")]
public class AuthorsController : ControllerBase
{
    private readonly LibraryDbContext _db;
    public AuthorsController(LibraryDbContext db) => _db = db;

    /// <summary>Iegūst visu autoru sarakstu</summary>
    /// <remarks>
    /// Atgriež visus autorus ar grāmatu skaitu. Piemērs:
    ///
    ///     GET /api/authors
    ///
    /// Pieprasa autentifikāciju (Bearer tokens).
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AuthorDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll()
    {
        var authors = await _db.Authors
            .Select(a => new AuthorDto
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Biography = a.Biography,
                BirthYear = a.BirthYear,
                BookCount = a.Books.Count
            }).ToListAsync();

        return Ok(authors);
    }

    /// <summary>Iegūst autoru pēc ID</summary>
    /// <remarks>
    /// Atgriež konkrētu autoru ar grāmatu skaitu. Piemērs:
    ///
    ///     GET /api/authors/1
    ///
    /// Pieprasa autentifikāciju (Bearer tokens).
    /// </remarks>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuthorDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var author = await _db.Authors.Include(a => a.Books).FirstOrDefaultAsync(a => a.Id == id);
        if (author == null)
            return NotFound(new { message = "Autors nav atrasts." });

        return Ok(new AuthorDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            Biography = author.Biography,
            BirthYear = author.BirthYear,
            BookCount = author.Books.Count
        });
    }

    /// <summary>Pievieno jaunu autoru</summary>
    /// <remarks>
    /// Pieprasa Librarian vai Admin lomu. Piemērs:
    ///
    ///     POST /api/authors
    ///     {
    ///         "firstName": "Jānis",
    ///         "lastName": "Rainis",
    ///         "biography": "Latviešu dzejnieks un dramaturgs",
    ///         "birthYear": 1865
    ///     }
    ///
    /// Validācija: vārds un uzvārds obligāti (2–100 simboli), gads 0–2100.
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Librarian,Admin")]
    [ProducesResponseType(typeof(AuthorDto), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] CreateAuthorRequest req)
    {
        var author = new Author
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            Biography = req.Biography,
            BirthYear = req.BirthYear
        };
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = author.Id }, new AuthorDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            Biography = author.Biography,
            BirthYear = author.BirthYear,
            BookCount = 0
        });
    }
}

[ApiController]
[Route("api/categories")]
[Authorize]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly LibraryDbContext _db;
    public CategoriesController(LibraryDbContext db) => _db = db;

    /// <summary>Iegūst visu kategoriju sarakstu</summary>
    /// <remarks>
    /// Atgriež visas kategorijas ar grāmatu skaitu. Piemērs:
    ///
    ///     GET /api/categories
    ///
    /// Pieprasa autentifikāciju (Bearer tokens).
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll()
    {
        var cats = await _db.Categories
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                BookCount = c.Books.Count
            }).ToListAsync();
        return Ok(cats);
    }

    /// <summary>Pievieno jaunu kategoriju</summary>
    /// <remarks>
    /// Pieprasa Admin lomu. Piemērs:
    ///
    ///     POST /api/categories
    ///     {
    ///         "name": "Daiļliteratūra",
    ///         "description": "Romāni, stāsti, dzeja un citi daiļliteratūras darbi"
    ///     }
    ///
    /// Validācija: nosaukums obligāts (2–100 simboli), apraksts maksimums 500 simboli.
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest req)
    {
        var cat = new Category { Name = req.Name, Description = req.Description };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new CategoryDto
        {
            Id = cat.Id,
            Name = cat.Name,
            Description = cat.Description,
            BookCount = 0
        });
    }
}
