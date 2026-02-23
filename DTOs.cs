using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs;

// ─── Auth DTOs ───────────────────────────────────────────────
public class LoginRequest
{
    /// <example>admin@library.lv</example>
    [Required(ErrorMessage = "E-pasts ir obligāts")]
    [EmailAddress(ErrorMessage = "Nepareizs e-pasta formāts")]
    public string Email { get; set; } = string.Empty;

    /// <example>Admin123!</example>
    [Required(ErrorMessage = "Parole ir obligāta")]
    [MinLength(6, ErrorMessage = "Parole — minimums 6 simboli")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    /// <example>Jānis Bērziņš</example>
    [Required(ErrorMessage = "Vārds ir obligāts")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Vārds — no 2 līdz 100 simboliem")]
    public string Name { get; set; } = string.Empty;

    /// <example>janis@library.lv</example>
    [Required(ErrorMessage = "E-pasts ir obligāts")]
    [EmailAddress(ErrorMessage = "Nepareizs e-pasta formāts")]
    public string Email { get; set; } = string.Empty;

    /// <example>Parole123!</example>
    [Required(ErrorMessage = "Parole ir obligāta")]
    [MinLength(6, ErrorMessage = "Parole — minimums 6 simboli")]
    public string Password { get; set; } = string.Empty;
}

public record AuthResponse(string Token, string Name, string Email, string Role);

// ─── Book DTOs ───────────────────────────────────────────────
public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int PublishedYear { get; set; }
    public int AvailableCopies { get; set; }
    public int TotalCopies { get; set; }
    public string? Description { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}

public class CreateBookRequest
{
    /// <example>Uguns un nakts</example>
    [Required(ErrorMessage = "Nosaukums ir obligāts")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Nosaukums — no 1 līdz 200 simboliem")]
    public string Title { get; set; } = string.Empty;

    /// <example>978-9984-00-001-1</example>
    [Required(ErrorMessage = "ISBN ir obligāts")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "ISBN — no 10 līdz 20 simboliem")]
    public string ISBN { get; set; } = string.Empty;

    /// <example>1905</example>
    [Range(1000, 2100, ErrorMessage = "Gads — no 1000 līdz 2100")]
    public int PublishedYear { get; set; }

    /// <example>3</example>
    [Range(1, 1000, ErrorMessage = "Eksemplāru skaits — no 1 līdz 1000")]
    public int TotalCopies { get; set; } = 1;

    /// <example>Leģendārā Raiņa luga par mīlestību un brīvību</example>
    [StringLength(1000, ErrorMessage = "Apraksts — maksimums 1000 simboli")]
    public string? Description { get; set; }

    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Autora ID ir obligāts")]
    public int AuthorId { get; set; }

    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Kategorijas ID ir obligāts")]
    public int CategoryId { get; set; }
}

// ─── Author DTOs ─────────────────────────────────────────────
public class AuthorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public int? BirthYear { get; set; }
    public int BookCount { get; set; }
}

public class CreateAuthorRequest
{
    /// <example>Jānis</example>
    [Required(ErrorMessage = "Vārds ir obligāts")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Vārds — no 2 līdz 100 simboliem")]
    public string FirstName { get; set; } = string.Empty;

    /// <example>Rainis</example>
    [Required(ErrorMessage = "Uzvārds ir obligāts")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Uzvārds — no 2 līdz 100 simboliem")]
    public string LastName { get; set; } = string.Empty;

    /// <example>Latviešu dzejnieks un dramaturgs, nacionālās atmodas simbols</example>
    [StringLength(2000, ErrorMessage = "Biogrāfija — maksimums 2000 simboli")]
    public string? Biography { get; set; }

    /// <example>1865</example>
    [Range(0, 2100, ErrorMessage = "Dzimšanas gads — no 0 līdz 2100")]
    public int? BirthYear { get; set; }
}

// ─── Category DTOs ───────────────────────────────────────────
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BookCount { get; set; }
}

public class CreateCategoryRequest
{
    /// <example>Daiļliteratūra</example>
    [Required(ErrorMessage = "Nosaukums ir obligāts")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Nosaukums — no 2 līdz 100 simboliem")]
    public string Name { get; set; } = string.Empty;

    /// <example>Romāni, stāsti, dzeja un citi daiļliteratūras darbi</example>
    [StringLength(500, ErrorMessage = "Apraksts — maksimums 500 simboli")]
    public string? Description { get; set; }
}

// ─── Loan DTOs ───────────────────────────────────────────────
public class LoanDto
{
    public int Id { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateLoanRequest
{
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Grāmatas ID ir obligāts")]
    public int BookId { get; set; }

    /// <example>14</example>
    [Range(1, 90, ErrorMessage = "Aizņemšanās ilgums — no 1 līdz 90 dienām")]
    public int DurationDays { get; set; } = 14;
}
