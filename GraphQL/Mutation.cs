using Microsoft.EntityFrameworkCore;
using GraphQLSimple.Data;
using GraphQLSimple.Models;

namespace GraphQLSimple.GraphQL
{
    public class Mutation
    {
        public async Task<Author> AddAuthor(string firstName, string lastName, DateTime dateOfBirth, string? biography, LibraryContext context)
        {
            var author = new Author
            {
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Biography = biography
            };

            context.Authors.Add(author);
            await context.SaveChangesAsync();
            return author;
        }

        public async Task<Book> AddBook(string title, string isbn, DateTime publishedDate, int pages, int authorId, LibraryContext context)
        {
            var book = new Book
            {
                Title = title,
                ISBN = isbn,
                PublishedDate = publishedDate,
                Pages = pages,
                AuthorId = authorId
            };

            context.Books.Add(book);
            await context.SaveChangesAsync();
            
            return await context.Books
                .Include(b => b.Author)
                .FirstAsync(b => b.Id == book.Id);
        }

        public async Task<Review> AddReview(int bookId, string reviewerName, int rating, string? comment, LibraryContext context)
        {
            var review = new Review
            {
                BookId = bookId,
                ReviewerName = reviewerName,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            context.Reviews.Add(review);
            await context.SaveChangesAsync();
            
            return await context.Reviews
                .Include(r => r.Book)
                .FirstAsync(r => r.Id == review.Id);
        }
    }
}
