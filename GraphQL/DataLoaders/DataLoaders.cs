using Microsoft.EntityFrameworkCore;
using GraphQLSimple.Data;
using GraphQLSimple.Models;
using HotChocolate;
using Tag = GraphQLSimple.Models.Tag;

namespace GraphQLSimple.GraphQL.DataLoaders;

public class AuthorByIdDataLoader : BatchDataLoader<int, Author>
{
    private readonly LibraryContext _context;

    public AuthorByIdDataLoader(
        LibraryContext context,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options ?? new DataLoaderOptions())
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

public class BooksByAuthorDataLoader : GroupedDataLoader<int, Book>
{
    private readonly LibraryContext _context;

    public BooksByAuthorDataLoader(
        LibraryContext context,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options ?? new DataLoaderOptions())
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
        : base(batchScheduler, options ?? new DataLoaderOptions())
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

public class ReviewsByBookDataLoader : GroupedDataLoader<int, Review>
{
    private readonly LibraryContext _context;

    public ReviewsByBookDataLoader(
        LibraryContext context,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options ?? new DataLoaderOptions())
    {
        _context = context;
    }

    protected override async Task<ILookup<int, Review>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var reviews = await _context.Reviews
            .Where(review => keys.Contains(review.BookId))
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
        : base(batchScheduler, options ?? new DataLoaderOptions())
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

public class TagsByBookDataLoader : GroupedDataLoader<int, Tag>
{
    private readonly LibraryContext _context;

    public TagsByBookDataLoader(
        LibraryContext context,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options ?? new DataLoaderOptions())
    {
        _context = context;
    }

    protected override async Task<ILookup<int, Tag>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var bookTags = await _context.Books
            .Where(book => keys.Contains(book.Id))
            .SelectMany(book => book.BookTags.Select(bt => new { BookId = book.Id, Tag = bt.Tag }))
            .ToListAsync(cancellationToken);

        return bookTags.ToLookup(bt => bt.BookId, bt => bt.Tag);
    }
}

public class BorrowingsByUserDataLoader : GroupedDataLoader<int, Borrowing>
{
    private readonly LibraryContext _context;

    public BorrowingsByUserDataLoader(
        LibraryContext context,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options ?? new DataLoaderOptions())
    {
        _context = context;
    }

    protected override async Task<ILookup<int, Borrowing>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var borrowings = await _context.Borrowings
            .Where(borrowing => keys.Contains(borrowing.UserId))
            .ToListAsync(cancellationToken);

        return borrowings.ToLookup(borrowing => borrowing.UserId);
    }
}

public class BorrowingsByBookDataLoader : GroupedDataLoader<int, Borrowing>
{
    private readonly LibraryContext _context;

    public BorrowingsByBookDataLoader(
        LibraryContext context,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options ?? new DataLoaderOptions())
    {
        _context = context;
    }

    protected override async Task<ILookup<int, Borrowing>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var borrowings = await _context.Borrowings
            .Where(borrowing => keys.Contains(borrowing.BookId))
            .ToListAsync(cancellationToken);

        return borrowings.ToLookup(borrowing => borrowing.BookId);
    }
}

public class ReviewsByUserDataLoader : GroupedDataLoader<int, Review>
{
    private readonly LibraryContext _context;

    public ReviewsByUserDataLoader(
        LibraryContext context,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options ?? new DataLoaderOptions())
    {
        _context = context;
    }

    protected override async Task<ILookup<int, Review>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var reviews = await _context.Reviews
            .Where(review => keys.Contains(review.UserId))
            .ToListAsync(cancellationToken);

        return reviews.ToLookup(review => review.UserId);
    }
}
