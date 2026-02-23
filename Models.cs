namespace LibraryAPI.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Reader"; // Reader, Librarian, Admin
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}

public class Author
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public int? BirthYear { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

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
    public string? Description { get; set; }

    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}

public class Loan
{
    public int Id { get; set; }
    public DateTime LoanDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Returned, Overdue

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
}
