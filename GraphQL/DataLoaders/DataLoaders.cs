using Microsoft.EntityFrameworkCore;
using GraphQLSimple.Data;
using GraphQLSimple.Models;
using HotChocolate;

namespace GraphQLSimple.GraphQL.DataLoaders
{
    public class AuthorByIdDataLoader : BatchDataLoader<int, Author>
    {
        private readonly LibraryContext _context;

        public AuthorByIdDataLoader(
            LibraryContext context,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _context = context;
        }

        protected override async Task<IReadOnlyDictionary<int, Author>> LoadBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            return await _context.Authors
                .Where(author => keys.Contains(author.Id))
                .ToDictionaryAsync(author => author.Id, cancellationToken);
        }
    }

    public class BooksByAuthorIdDataLoader : GroupedDataLoader<int, Book>
    {
        private readonly LibraryContext _context;

        public BooksByAuthorIdDataLoader(
            LibraryContext context,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _context = context;
        }

        protected override async Task<ILookup<int, Book>> LoadGroupedBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            var books = await _context.Books
                .Where(book => keys.Contains(book.AuthorId))
                .ToListAsync(cancellationToken);

            return books.ToLookup(book => book.AuthorId);
        }
    }

    public class CategoryByIdDataLoader : BatchDataLoader<int, Category>
    {
        private readonly LibraryContext _context;

        public CategoryByIdDataLoader(
            LibraryContext context,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _context = context;
        }

        protected override async Task<IReadOnlyDictionary<int, Category>> LoadBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            return await _context.Categories
                .Where(category => keys.Contains(category.Id))
                .ToDictionaryAsync(category => category.Id, cancellationToken);
        }
    }

    public class ReviewsByBookIdDataLoader : GroupedDataLoader<int, Review>
    {
        private readonly LibraryContext _context;

        public ReviewsByBookIdDataLoader(
            LibraryContext context,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _context = context;
        }

        protected override async Task<ILookup<int, Review>> LoadGroupedBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            var reviews = await _context.Reviews
                .Where(review => keys.Contains(review.BookId))
                .Include(r => r.User)
                .ToListAsync(cancellationToken);

            return reviews.ToLookup(review => review.BookId);
        }
    }

    public class UserByIdDataLoader : BatchDataLoader<int, User>
    {
        private readonly LibraryContext _context;

        public UserByIdDataLoader(
            LibraryContext context,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _context = context;
        }

        protected override async Task<IReadOnlyDictionary<int, User>> LoadBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            return await _context.Users
                .Where(user => keys.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id, cancellationToken);
        }
    }

    public class TagsByBookIdDataLoader : GroupedDataLoader<int, Models.Tag>
    {
        private readonly LibraryContext _context;

        public TagsByBookIdDataLoader(
            LibraryContext context,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _context = context;
        }

        protected override async Task<ILookup<int, Models.Tag>> LoadGroupedBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            var bookTags = await _context.BookTags
                .Where(bt => keys.Contains(bt.BookId))
                .Include(bt => bt.Tag)
                .ToListAsync(cancellationToken);

            return bookTags.ToLookup(bt => bt.BookId, bt => bt.Tag);
        }
    }

    public class BorrowingsByUserIdDataLoader : GroupedDataLoader<int, Borrowing>
    {
        private readonly LibraryContext _context;

        public BorrowingsByUserIdDataLoader(
            LibraryContext context,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _context = context;
        }

        protected override async Task<ILookup<int, Borrowing>> LoadGroupedBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            var borrowings = await _context.Borrowings
                .Where(borrowing => keys.Contains(borrowing.UserId))
                .Include(b => b.Book)
                    .ThenInclude(book => book.Author)
                .Include(b => b.User)
                .ToListAsync(cancellationToken);

            return borrowings.ToLookup(borrowing => borrowing.UserId);
        }
    }

    public class BorrowingsByBookIdDataLoader : GroupedDataLoader<int, Borrowing>
    {
        private readonly LibraryContext _context;

        public BorrowingsByBookIdDataLoader(
            LibraryContext context,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _context = context;
        }

        protected override async Task<ILookup<int, Borrowing>> LoadGroupedBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            var borrowings = await _context.Borrowings
                .Where(borrowing => keys.Contains(borrowing.BookId))
                .Include(b => b.Book)
                    .ThenInclude(book => book.Author)
                .Include(b => b.User)
                .ToListAsync(cancellationToken);

            return borrowings.ToLookup(borrowing => borrowing.BookId);
        }
    }

    public class ReviewsByUserIdDataLoader : GroupedDataLoader<int, Review>
    {
        private readonly LibraryContext _context;

        public ReviewsByUserIdDataLoader(
            LibraryContext context,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _context = context;
        }

        protected override async Task<ILookup<int, Review>> LoadGroupedBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            var reviews = await _context.Reviews
                .Where(review => keys.Contains(review.UserId))
                .Include(r => r.Book)
                    .ThenInclude(b => b.Author)
                .Include(r => r.User)
                .ToListAsync(cancellationToken);

            return reviews.ToLookup(review => review.UserId);
        }
    }
}
