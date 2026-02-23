using System.Security.Claims;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers;

/// <summary>
/// Grāmatu izsniegšana un atgriešana
/// </summary>
[ApiController]
[Route("api/borrows")]
[Authorize]
[Produces("application/json")]
public class BorrowsController : ControllerBase
{
    private readonly LibraryDbContext _db;
    public BorrowsController(LibraryDbContext db) => _db = db;

    /// <summary>
    /// Atgriež izsniegumu sarakstu. Admin redz visus, lietotājs – tikai savus.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BorrowResponseDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");

        var query = _db.Borrows
            .Include(br => br.Book)
            .Include(br => br.User)
            .AsQueryable();

        if (!isAdmin)
            query = query.Where(br => br.UserId == userId);

        // Atjaunina statuss uz Overdue ja nokavēts
        var borrows = await query.ToListAsync();
        foreach (var b in borrows.Where(b => b.Status == "Active" && b.DueDate < DateTime.UtcNow))
        {
            b.Status = "Overdue";
        }
        await _db.SaveChangesAsync();

        var result = borrows.Select(br => new BorrowResponseDto(
            br.Id, br.Book.Title, br.User.Name,
            br.BorrowedAt, br.DueDate, br.ReturnedAt, br.Status));

        return Ok(result);
    }

    /// <summary>
    /// Izsniedz grāmatu lietotājam (autorizēts lietotājs)
    /// </summary>
    /// <param name="dto">Grāmatas ID un aizņemšanās periods dienās (max 30)</param>
    [HttpPost]
    [ProducesResponseType(typeof(BorrowResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Borrow([FromBody] BorrowCreateDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (dto.DaysToReturn < 1 || dto.DaysToReturn > 30)
            return BadRequest(new { message = "Aizņemšanās periods ir no 1 līdz 30 dienām." });

        var book = await _db.Books.FindAsync(dto.BookId);
        if (book == null)
            return BadRequest(new { message = "Grāmata nav atrasta." });

        if (book.AvailableCopies <= 0)
            return BadRequest(new { message = "Grāmata nav pieejama. Visas kopijas ir izsniegtas." });

        var alreadyBorrowed = await _db.Borrows.AnyAsync(b =>
            b.UserId == userId && b.BookId == dto.BookId && b.Status == "Active");

        if (alreadyBorrowed)
            return BadRequest(new { message = "Jūs jau esat aizņēmies šo grāmatu." });

        var borrow = new Borrow
        {
            UserId = userId,
            BookId = dto.BookId,
            DueDate = DateTime.UtcNow.AddDays(dto.DaysToReturn)
        };

        book.AvailableCopies--;
        _db.Borrows.Add(borrow);
        await _db.SaveChangesAsync();

        await _db.Entry(borrow).Reference(b => b.Book).LoadAsync();
        await _db.Entry(borrow).Reference(b => b.User).LoadAsync();

        return CreatedAtAction(nameof(GetAll), new { id = borrow.Id },
            new BorrowResponseDto(borrow.Id, borrow.Book.Title, borrow.User.Name,
                borrow.BorrowedAt, borrow.DueDate, null, borrow.Status));
    }

    /// <summary>
    /// Atgriež izsniegto grāmatu
    /// </summary>
    /// <param name="id">Izsnieguma ID</param>
    [HttpPost("{id}/return")]
    [ProducesResponseType(typeof(BorrowResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Return(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");

        var borrow = await _db.Borrows
            .Include(b => b.Book)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (borrow == null)
            return NotFound(new { message = "Izsniegums nav atrasts." });

        if (!isAdmin && borrow.UserId != userId)
            return Forbid();

        if (borrow.Status == "Returned")
            return BadRequest(new { message = "Grāmata jau ir atgriezta." });

        borrow.Status = "Returned";
        borrow.ReturnedAt = DateTime.UtcNow;
        borrow.Book.AvailableCopies++;

        await _db.SaveChangesAsync();

        return Ok(new BorrowResponseDto(borrow.Id, borrow.Book.Title, borrow.User.Name,
            borrow.BorrowedAt, borrow.DueDate, borrow.ReturnedAt, borrow.Status));
    }
}
