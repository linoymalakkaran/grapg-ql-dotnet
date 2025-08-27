using Microsoft.EntityFrameworkCore;
using GraphQLSimple.Models;

namespace GraphQLSimple.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed data
            modelBuilder.Entity<Author>().HasData(
                new Author { Id = 1, FirstName = "J.K.", LastName = "Rowling", DateOfBirth = new DateTime(1965, 7, 31), Biography = "British author, best known for the Harry Potter series." },
                new Author { Id = 2, FirstName = "George", LastName = "Orwell", DateOfBirth = new DateTime(1903, 6, 25), Biography = "English novelist and essayist, journalist and critic." }
            );

            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, Title = "Harry Potter and the Philosopher's Stone", ISBN = "978-0747532699", PublishedDate = new DateTime(1997, 6, 26), Pages = 223, AuthorId = 1 },
                new Book { Id = 2, Title = "1984", ISBN = "978-0451524935", PublishedDate = new DateTime(1949, 6, 8), Pages = 328, AuthorId = 2 }
            );

            modelBuilder.Entity<Review>().HasData(
                new Review { Id = 1, BookId = 1, ReviewerName = "Alice Johnson", Rating = 5, Comment = "Amazing book!", CreatedAt = DateTime.Now.AddDays(-10) },
                new Review { Id = 2, BookId = 1, ReviewerName = "Bob Smith", Rating = 4, Comment = "Great read", CreatedAt = DateTime.Now.AddDays(-5) },
                new Review { Id = 3, BookId = 2, ReviewerName = "Carol Brown", Rating = 5, Comment = "Thought-provoking", CreatedAt = DateTime.Now.AddDays(-3) }
            );
        }
    }
}
