using Microsoft.EntityFrameworkCore;
using GraphQLSimple.Data;
using GraphQLSimple.Models;

namespace GraphQLSimple.GraphQL.DataLoaders
{
    public class AuthorByIdDataLoader : BatchDataLoader<int, Author>
    {
        private readonly IDbContextFactory<LibraryContext> _dbContextFactory;

        public AuthorByIdDataLoader(
            IDbContextFactory<LibraryContext> dbContextFactory,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task<IReadOnlyDictionary<int, Author>> LoadBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            
            return await context.Authors
                .Where(author => keys.Contains(author.Id))
                .ToDictionaryAsync(author => author.Id, cancellationToken);
        }
    }

    public class BooksByAuthorIdDataLoader : GroupedDataLoader<int, Book>
    {
        private readonly IDbContextFactory<LibraryContext> _dbContextFactory;

        public BooksByAuthorIdDataLoader(
            IDbContextFactory<LibraryContext> dbContextFactory,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task<ILookup<int, Book>> LoadGroupedBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            
            var books = await context.Books
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .Include(b => b.BookTags)
                    .ThenInclude(bt => bt.Tag)
                .Where(book => keys.Contains(book.AuthorId))
                .ToListAsync(cancellationToken);

            return books.ToLookup(book => book.AuthorId);
        }
    }

    public class CategoryByIdDataLoader : BatchDataLoader<int, Category>
    {
        private readonly IDbContextFactory<LibraryContext> _dbContextFactory;

        public CategoryByIdDataLoader(
            IDbContextFactory<LibraryContext> dbContextFactory,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task<IReadOnlyDictionary<int, Category>> LoadBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            
            return await context.Categories
                .Where(category => keys.Contains(category.Id))
                .ToDictionaryAsync(category => category.Id, cancellationToken);
        }
    }

    public class ReviewsByBookIdDataLoader : GroupedDataLoader<int, Review>
    {
        private readonly IDbContextFactory<LibraryContext> _dbContextFactory;

        public ReviewsByBookIdDataLoader(
            IDbContextFactory<LibraryContext> dbContextFactory,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task<ILookup<int, Review>> LoadGroupedBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            
            var reviews = await context.Reviews
                .Include(r => r.User)
                .Where(review => keys.Contains(review.BookId))
                .ToListAsync(cancellationToken);

            return reviews.ToLookup(review => review.BookId);
        }
    }

    public class UserByIdDataLoader : BatchDataLoader<int, User>
    {
        private readonly IDbContextFactory<LibraryContext> _dbContextFactory;

        public UserByIdDataLoader(
            IDbContextFactory<LibraryContext> dbContextFactory,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task<IReadOnlyDictionary<int, User>> LoadBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            
            return await context.Users
                .Where(user => keys.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id, cancellationToken);
        }
    }

    public class TagsByBookIdDataLoader : GroupedDataLoader<int, Models.Tag>
    {
        private readonly IDbContextFactory<LibraryContext> _dbContextFactory;

        public TagsByBookIdDataLoader(
            IDbContextFactory<LibraryContext> dbContextFactory,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task<ILookup<int, Models.Tag>> LoadGroupedBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            
            var bookTags = await context.BookTags
                .Include(bt => bt.Tag)
                .Where(bt => keys.Contains(bt.BookId))
                .ToListAsync(cancellationToken);

            return bookTags
                .Where(bt => bt.Tag != null)
                .ToLookup(bt => bt.BookId, bt => bt.Tag);
        }
    }

    public class BorrowingsByBookIdDataLoader : GroupedDataLoader<int, Borrowing>
    {
        private readonly IDbContextFactory<LibraryContext> _dbContextFactory;

        public BorrowingsByBookIdDataLoader(
            IDbContextFactory<LibraryContext> dbContextFactory,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task<ILookup<int, Borrowing>> LoadGroupedBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            
            var borrowings = await context.Borrowings
                .Include(b => b.User)
                .Where(borrowing => keys.Contains(borrowing.BookId))
                .ToListAsync(cancellationToken);

            return borrowings.ToLookup(borrowing => borrowing.BookId);
        }
    }

    public class BorrowingsByUserIdDataLoader : GroupedDataLoader<int, Borrowing>
    {
        private readonly IDbContextFactory<LibraryContext> _dbContextFactory;

        public BorrowingsByUserIdDataLoader(
            IDbContextFactory<LibraryContext> dbContextFactory,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task<ILookup<int, Borrowing>> LoadGroupedBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            
            var borrowings = await context.Borrowings
                .Include(b => b.Book)
                    .ThenInclude(book => book.Author)
                .Where(borrowing => keys.Contains(borrowing.UserId))
                .ToListAsync(cancellationToken);

            return borrowings.ToLookup(borrowing => borrowing.UserId);
        }
    }

    public class ReviewsByUserIdDataLoader : GroupedDataLoader<int, Review>
    {
        private readonly IDbContextFactory<LibraryContext> _dbContextFactory;

        public ReviewsByUserIdDataLoader(
            IDbContextFactory<LibraryContext> dbContextFactory,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task<ILookup<int, Review>> LoadGroupedBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            
            var reviews = await context.Reviews
                .Include(r => r.Book)
                    .ThenInclude(b => b.Author)
                .Where(review => keys.Contains(review.UserId))
                .ToListAsync(cancellationToken);

            return reviews.ToLookup(review => review.UserId);
        }
    }
}
