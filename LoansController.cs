using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Models;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/loans")]
[Authorize]
[Produces("application/json")]
public class LoansController : ControllerBase
{
    private readonly LibraryDbContext _db;
    public LoansController(LibraryDbContext db) => _db = db;

    /// <summary>Iegūst aizdevumu sarakstu</summary>
    /// <remarks>
    /// Lomas nosaka redzamību:
    /// - **Reader** — redz tikai savus aizdevumus
    /// - **Librarian / Admin** — redz visus aizdevumus
    ///
    ///     GET /api/loans
    ///
    /// Pieprasa autentifikāciju (Bearer tokens).
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LoanDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);

        var query = _db.Loans
            .Include(l => l.Book)
            .Include(l => l.User)
            .AsQueryable();

        if (role == "Reader")
            query = query.Where(l => l.UserId == userId);

        var loans = await query.Select(l => new LoanDto
        {
            Id = l.Id,
            BookTitle = l.Book.Title,
            UserName = l.User.Name,
            LoanDate = l.LoanDate,
            DueDate = l.DueDate,
            ReturnDate = l.ReturnDate,
            Status = l.Status
        }).ToListAsync();

        return Ok(loans);
    }

    /// <summary>Izveido jaunu grāmatas aizdevumu</summary>
    /// <remarks>
    /// Samazina pieejamo eksemplāru skaitu par 1. Piemērs:
    ///
    ///     POST /api/loans
    ///     {
    ///         "bookId": 1,
    ///         "durationDays": 14
    ///     }
    ///
    /// Validācija:
    /// - Grāmatai jābūt pieejamiem eksemplāriem
    /// - Lietotājs nevar aizņemties vienu grāmatu divreiz vienlaicīgi
    /// - Aizņemšanās ilgums — no 1 līdz 90 dienām
    ///
    /// Pieprasa autentifikāciju (Bearer tokens).
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(LoanDto), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Create([FromBody] CreateLoanRequest req)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var book = await _db.Books.FindAsync(req.BookId);
        if (book == null)
            return BadRequest(new { message = "Grāmata nav atrasta." });

        if (book.AvailableCopies <= 0)
            return BadRequest(new { message = "Nav pieejamu eksemplāru." });

        var existingLoan = await _db.Loans.AnyAsync(l =>
            l.UserId == userId && l.BookId == req.BookId && l.Status == "Active");
        if (existingLoan)
            return BadRequest(new { message = "Jūs jau esat aizņēmušies šo grāmatu." });

        var loan = new Loan
        {
            UserId = userId,
            BookId = req.BookId,
            DueDate = DateTime.UtcNow.AddDays(req.DurationDays),
            Status = "Active"
        };

        book.AvailableCopies--;
        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);
        return CreatedAtAction(nameof(GetAll), new LoanDto
        {
            Id = loan.Id,
            BookTitle = book.Title,
            UserName = user!.Name,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = null,
            Status = loan.Status
        });
    }

    /// <summary>Atgriež grāmatu (aizdevumu noslēdz)</summary>
    /// <remarks>
    /// Palielina pieejamo eksemplāru skaitu par 1 un maina statusu uz "Returned". Piemērs:
    ///
    ///     POST /api/loans/1/return
    ///
    /// Lomas:
    /// - **Reader** — var atgriezt tikai savus aizdevumus
    /// - **Librarian / Admin** — var atgriezt jebkuru aizdevumu
    ///
    /// Pieprasa autentifikāciju (Bearer tokens).
    /// </remarks>
    [HttpPost("{id}/return")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Return(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);

        var loan = await _db.Loans.Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == id);
        if (loan == null)
            return NotFound(new { message = "Aizdevums nav atrasts." });

        if (role == "Reader" && loan.UserId != userId)
            return Forbid();

        if (loan.Status != "Active")
            return BadRequest(new { message = "Aizdevums jau noslēgts." });

        loan.ReturnDate = DateTime.UtcNow;
        loan.Status = "Returned";
        loan.Book.AvailableCopies++;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Grāmata veiksmīgi atgriezta." });
    }
}
