using Microsoft.EntityFrameworkCore;
using LibraryAPI.Models;

namespace LibraryAPI.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Loan> Loans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Daiļliteratūra", Description = "Romāni, stāsti, dzeja" },
            new Category { Id = 2, Name = "Zinātne", Description = "Zinātniski un izglītojoši darbi" },
            new Category { Id = 3, Name = "Vēsture", Description = "Vēsturiski darbi un biogrāfijas" },
            new Category { Id = 4, Name = "Tehnoloģijas", Description = "IT, programmēšana, inženierija" }
        );

        // Seed Authors
        modelBuilder.Entity<Author>().HasData(
            new Author { Id = 1, FirstName = "Jānis", LastName = "Rainis", BirthYear = 1865, Biography = "Latviešu dzejnieks un dramaturgs" },
            new Author { Id = 2, FirstName = "Robert", LastName = "Martin", BirthYear = 1952, Biography = "Software engineering expert" }
        );

        // Seed Books
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "Uguns un nakts", ISBN = "978-9984-00-001-1", PublishedYear = 1905, TotalCopies = 3, AvailableCopies = 3, AuthorId = 1, CategoryId = 1, Description = "Leģendārā Raiņa luga" },
            new Book { Id = 2, Title = "Clean Code", ISBN = "978-0-13-235088-4", PublishedYear = 2008, TotalCopies = 5, AvailableCopies = 5, AuthorId = 2, CategoryId = 4, Description = "A Handbook of Agile Software Craftsmanship" }
        );

        // Seed admin user (password: Admin123!)
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "Administrators",
                Email = "admin@library.lv",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "Admin",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
