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
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Borrowing> Borrowings { get; set; }
        public DbSet<Models.Tag> Tags { get; set; }
        public DbSet<BookTag> BookTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.HasIndex(b => b.ISBN).IsUnique();
                entity.Property(b => b.Price).HasPrecision(18, 2);
                
                entity.HasOne(b => b.Author)
                    .WithMany(a => a.Books)
                    .HasForeignKey(b => b.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(b => b.Category)
                    .WithMany(c => c.Books)
                    .HasForeignKey(b => b.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);
                
                entity.HasOne(r => r.Book)
                    .WithMany(b => b.Reviews)
                    .HasForeignKey(r => r.BookId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.Name).IsUnique();
                
                entity.HasOne(c => c.ParentCategory)
                    .WithMany(c => c.SubCategories)
                    .HasForeignKey(c => c.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<Borrowing>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.FineAmount).HasPrecision(18, 2);
                
                entity.HasOne(b => b.Book)
                    .WithMany(book => book.Borrowings)
                    .HasForeignKey(b => b.BookId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(b => b.User)
                    .WithMany(u => u.Borrowings)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Models.Tag>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.Name).IsUnique();
            });

            modelBuilder.Entity<BookTag>(entity =>
            {
                entity.HasKey(bt => new { bt.BookId, bt.TagId });
                
                entity.HasOne(bt => bt.Book)
                    .WithMany(b => b.BookTags)
                    .HasForeignKey(bt => bt.BookId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne<Models.Tag>()
                    .WithMany(t => t.BookTags)
                    .HasForeignKey(bt => bt.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Fiction", Description = "Fictional works and novels", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Id = 2, Name = "Science Fiction", Description = "Science fiction novels and stories", ParentCategoryId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Id = 3, Name = "Fantasy", Description = "Fantasy novels and stories", ParentCategoryId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Id = 4, Name = "Non-Fiction", Description = "Non-fictional works", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Id = 5, Name = "Biography", Description = "Biographical works", ParentCategoryId = 4, IsActive = true, CreatedAt = DateTime.UtcNow }
            );

            // Authors
            modelBuilder.Entity<Author>().HasData(
                new Author 
                { 
                    Id = 1, 
                    FirstName = "J.K.", 
                    LastName = "Rowling", 
                    DateOfBirth = new DateTime(1965, 7, 31), 
                    Biography = "British author, best known for the Harry Potter series.",
                    Nationality = "British",
                    Email = "jk.rowling@example.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Author 
                { 
                    Id = 2, 
                    FirstName = "George", 
                    LastName = "Orwell", 
                    DateOfBirth = new DateTime(1903, 6, 25), 
                    Biography = "English novelist and essayist, journalist and critic.",
                    Nationality = "British",
                    Email = "george.orwell@example.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Author 
                { 
                    Id = 3, 
                    FirstName = "Isaac", 
                    LastName = "Asimov", 
                    DateOfBirth = new DateTime(1920, 1, 2), 
                    Biography = "American writer and professor of biochemistry, known for science fiction works.",
                    Nationality = "American",
                    Email = "isaac.asimov@example.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Books
            modelBuilder.Entity<Book>().HasData(
                new Book 
                { 
                    Id = 1, 
                    Title = "Harry Potter and the Philosopher's Stone", 
                    ISBN = "9780747532699", 
                    Description = "The first book in the Harry Potter series, following young Harry as he discovers his magical heritage.",
                    PublishedDate = new DateTime(1997, 6, 26), 
                    Pages = 223, 
                    Price = 12.99m,
                    Publisher = "Bloomsbury",
                    Language = "English",
                    IsAvailable = true,
                    CopiesTotal = 10,
                    CopiesAvailable = 8,
                    AuthorId = 1,
                    CategoryId = 3,
                    CreatedAt = DateTime.UtcNow
                },
                new Book 
                { 
                    Id = 2, 
                    Title = "1984", 
                    ISBN = "9780451524935", 
                    Description = "A dystopian social science fiction novel exploring themes of totalitarianism and surveillance.",
                    PublishedDate = new DateTime(1949, 6, 8), 
                    Pages = 328, 
                    Price = 14.99m,
                    Publisher = "Secker & Warburg",
                    Language = "English",
                    IsAvailable = true,
                    CopiesTotal = 15,
                    CopiesAvailable = 12,
                    AuthorId = 2,
                    CategoryId = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new Book 
                { 
                    Id = 3, 
                    Title = "Foundation", 
                    ISBN = "9780553293357", 
                    Description = "The first novel in Asimov's Foundation series, exploring psychohistory and the fall of a galactic empire.",
                    PublishedDate = new DateTime(1951, 5, 1), 
                    Pages = 244, 
                    Price = 13.99m,
                    Publisher = "Gnome Press",
                    Language = "English",
                    IsAvailable = true,
                    CopiesTotal = 8,
                    CopiesAvailable = 5,
                    AuthorId = 3,
                    CategoryId = 2,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Users
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    FirstName = "Alice", 
                    LastName = "Johnson", 
                    Email = "alice.johnson@example.com",
                    PhoneNumber = "+1-555-0101",
                    DateOfBirth = new DateTime(1990, 3, 15),
                    MembershipDate = DateTime.UtcNow.AddYears(-2),
                    IsActive = true,
                    UserType = UserType.Member,
                    Address = "123 Main St, Anytown, USA",
                    CreatedAt = DateTime.UtcNow
                },
                new User 
                { 
                    Id = 2, 
                    FirstName = "Bob", 
                    LastName = "Smith", 
                    Email = "bob.smith@example.com",
                    PhoneNumber = "+1-555-0102",
                    DateOfBirth = new DateTime(1985, 7, 22),
                    MembershipDate = DateTime.UtcNow.AddYears(-1),
                    IsActive = true,
                    UserType = UserType.Member,
                    Address = "456 Oak Ave, Somewhere, USA",
                    CreatedAt = DateTime.UtcNow
                },
                new User 
                { 
                    Id = 3, 
                    FirstName = "Carol", 
                    LastName = "Brown", 
                    Email = "carol.brown@library.com",
                    PhoneNumber = "+1-555-0103",
                    DateOfBirth = new DateTime(1982, 11, 8),
                    MembershipDate = DateTime.UtcNow.AddYears(-5),
                    IsActive = true,
                    UserType = UserType.Librarian,
                    Address = "789 Pine Rd, Library Town, USA",
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Reviews
            modelBuilder.Entity<Review>().HasData(
                new Review 
                { 
                    Id = 1, 
                    BookId = 1, 
                    UserId = 1,
                    Rating = 5, 
                    Comment = "Amazing book! Harry Potter is a masterpiece that captivated me from the first page.",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    IsVerifiedPurchase = true,
                    HelpfulVotes = 15
                },
                new Review 
                { 
                    Id = 2, 
                    BookId = 1, 
                    UserId = 2,
                    Rating = 4, 
                    Comment = "Great read, though it took me a while to get into the magical world.",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    IsVerifiedPurchase = true,
                    HelpfulVotes = 8
                },
                new Review 
                { 
                    Id = 3, 
                    BookId = 2, 
                    UserId = 1,
                    Rating = 5, 
                    Comment = "Thought-provoking and eerily relevant to today's world. Orwell was a visionary.",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    IsVerifiedPurchase = true,
                    HelpfulVotes = 22
                },
                new Review 
                { 
                    Id = 4, 
                    BookId = 3, 
                    UserId = 2,
                    Rating = 4, 
                    Comment = "Excellent science fiction with complex ideas about society and prediction.",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    IsVerifiedPurchase = false,
                    HelpfulVotes = 5
                }
            );

            // Tags
            modelBuilder.Entity<Models.Tag>().HasData(
                new Models.Tag { Id = 1, Name = "Magic", Description = "Books involving magic and supernatural elements", Color = "#9b59b6", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Models.Tag { Id = 2, Name = "Dystopia", Description = "Books set in dystopian societies", Color = "#e74c3c", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Models.Tag { Id = 3, Name = "Space", Description = "Books set in space or involving space travel", Color = "#3498db", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Models.Tag { Id = 4, Name = "Young Adult", Description = "Books targeted at young adult readers", Color = "#f39c12", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Models.Tag { Id = 5, Name = "Classic", Description = "Classic literature", Color = "#2ecc71", IsActive = true, CreatedAt = DateTime.UtcNow }
            );

            // Book Tags
            modelBuilder.Entity<BookTag>().HasData(
                new BookTag { BookId = 1, TagId = 1, CreatedAt = DateTime.UtcNow }, // Harry Potter - Magic
                new BookTag { BookId = 1, TagId = 4, CreatedAt = DateTime.UtcNow }, // Harry Potter - Young Adult
                new BookTag { BookId = 2, TagId = 2, CreatedAt = DateTime.UtcNow }, // 1984 - Dystopia
                new BookTag { BookId = 2, TagId = 5, CreatedAt = DateTime.UtcNow }, // 1984 - Classic
                new BookTag { BookId = 3, TagId = 3, CreatedAt = DateTime.UtcNow }, // Foundation - Space
                new BookTag { BookId = 3, TagId = 5, CreatedAt = DateTime.UtcNow }  // Foundation - Classic
            );

            // Borrowings
            modelBuilder.Entity<Borrowing>().HasData(
                new Borrowing 
                { 
                    Id = 1, 
                    BookId = 1, 
                    UserId = 1,
                    BorrowedDate = DateTime.UtcNow.AddDays(-15),
                    DueDate = DateTime.UtcNow.AddDays(-1),
                    ReturnedDate = DateTime.UtcNow.AddDays(-2),
                    Status = BorrowingStatus.Returned,
                    Notes = "Returned in good condition"
                },
                new Borrowing 
                { 
                    Id = 2, 
                    BookId = 2, 
                    UserId = 2,
                    BorrowedDate = DateTime.UtcNow.AddDays(-10),
                    DueDate = DateTime.UtcNow.AddDays(4),
                    Status = BorrowingStatus.Active,
                    Notes = "Extended loan period requested"
                },
                new Borrowing 
                { 
                    Id = 3, 
                    BookId = 3, 
                    UserId = 1,
                    BorrowedDate = DateTime.UtcNow.AddDays(-25),
                    DueDate = DateTime.UtcNow.AddDays(-11),
                    Status = BorrowingStatus.Overdue,
                    FineAmount = 5.50m,
                    Notes = "Book is overdue, fine applied"
                }
            );
        }
    }
}
