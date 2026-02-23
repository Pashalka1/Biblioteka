using Microsoft.EntityFrameworkCore;
using LibraryAPI.Models;

namespace LibraryAPI.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Borrow> Borrows { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique constraints
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Book>().HasIndex(b => b.ISBN).IsUnique();

        // Relationships
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Book>()
            .HasOne(b => b.Genre)
            .WithMany(g => g.Books)
            .HasForeignKey(b => b.GenreId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Borrow>()
            .HasOne(br => br.User)
            .WithMany(u => u.Borrows)
            .HasForeignKey(br => br.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Borrow>()
            .HasOne(br => br.Book)
            .WithMany(b => b.Borrows)
            .HasForeignKey(br => br.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed data
        modelBuilder.Entity<Genre>().HasData(
            new Genre { Id = 1, Name = "Fantāzija", Description = "Fantāzijas grāmatas ar maģisku pasauli" },
            new Genre { Id = 2, Name = "Detektīvs", Description = "Noziegumu izmeklēšanas stāsti" },
            new Genre { Id = 3, Name = "Zinātnes fantastika", Description = "Nākotnes tehnoloģijas un kosmoss" },
            new Genre { Id = 4, Name = "Vēsturiskais romāns", Description = "Stāsti, kas notiek vēsturiskā vidē" }
        );

        modelBuilder.Entity<Author>().HasData(
            new Author { Id = 1, FullName = "J.R.R. Tolkien", Country = "Lielbritānija", BirthYear = 1892 },
            new Author { Id = 2, FullName = "Agatha Christie", Country = "Lielbritānija", BirthYear = 1890 },
            new Author { Id = 3, FullName = "Frank Herbert", Country = "ASV", BirthYear = 1920 }
        );

        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "Gredzenu pavēlnieks", ISBN = "978-0618640157", PublishedYear = 1954, TotalCopies = 3, AvailableCopies = 3, AuthorId = 1, GenreId = 1 },
            new Book { Id = 2, Title = "Hobits", ISBN = "978-0547928227", PublishedYear = 1937, TotalCopies = 2, AvailableCopies = 2, AuthorId = 1, GenreId = 1 },
            new Book { Id = 3, Title = "Un tad viņu nebija neviena", ISBN = "978-0007136834", PublishedYear = 1939, TotalCopies = 4, AvailableCopies = 4, AuthorId = 2, GenreId = 2 },
            new Book { Id = 4, Title = "Dune", ISBN = "978-0441013593", PublishedYear = 1965, TotalCopies = 2, AvailableCopies = 2, AuthorId = 3, GenreId = 3 }
        );
    }
}
