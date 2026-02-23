namespace LibraryAPI.DTOs;

// AUTH
public record RegisterDto(string Name, string Email, string Password);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, string Name, string Email, string Role);

// BOOKS
public record BookCreateDto(
    string Title,
    string ISBN,
    int PublishedYear,
    int TotalCopies,
    int AuthorId,
    int GenreId
);

public record BookResponseDto(
    int Id,
    string Title,
    string ISBN,
    int PublishedYear,
    int AvailableCopies,
    int TotalCopies,
    string AuthorName,
    string GenreName
);

// AUTHORS
public record AuthorCreateDto(string FullName, string Country, int? BirthYear);
public record AuthorResponseDto(int Id, string FullName, string Country, int? BirthYear, int BookCount);

// GENRES
public record GenreCreateDto(string Name, string Description);
public record GenreResponseDto(int Id, string Name, string Description, int BookCount);

// BORROWS
public record BorrowCreateDto(int BookId, int DaysToReturn);
public record BorrowResponseDto(
    int Id,
    string BookTitle,
    string UserName,
    DateTime BorrowedAt,
    DateTime DueDate,
    DateTime? ReturnedAt,
    string Status
);
