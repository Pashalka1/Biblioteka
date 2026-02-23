namespace LibraryAPI.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // "Admin" or "User"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();
}

public class Author
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int? BirthYear { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int PublishedYear { get; set; }
    public int TotalCopies { get; set; } = 1;
    public int AvailableCopies { get; set; } = 1;

    // FK
    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;

    public int GenreId { get; set; }
    public Genre Genre { get; set; } = null!;

    public ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();
}

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<Book> Books { get; set; } = new List<Book>();
}

public class Borrow
{
    public int Id { get; set; }
    public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string Status { get; set; } = "Active"; // "Active", "Returned", "Overdue"

    // FK
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
}
