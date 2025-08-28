using GraphQLSimple.Models;
using HotChocolate.Subscriptions;
using HotChocolate.Types;

namespace GraphQLSimple.GraphQL
{
    public class Subscription
    {
        /// <summary>
        /// Subscribe to book creation events
        /// </summary>
        [Subscribe]
        [Topic("BookCreated")]
        public Book OnBookCreated([EventMessage] Book book)
            => book;

        /// <summary>
        /// Subscribe to book updates
        /// </summary>
        [Subscribe]
        [Topic("BookUpdated")]
        public Book OnBookUpdated([EventMessage] Book book)
            => book;

        /// <summary>
        /// Subscribe to new reviews
        /// </summary>
        [Subscribe]
        [Topic("ReviewCreated")]
        public Review OnReviewCreated([EventMessage] Review review)
            => review;

        /// <summary>
        /// Subscribe to borrowing events
        /// </summary>
        [Subscribe]
        [Topic("BorrowingCreated")]
        public Borrowing OnBorrowingCreated([EventMessage] Borrowing borrowing)
            => borrowing;

        /// <summary>
        /// Subscribe to book return events
        /// </summary>
        [Subscribe]
        [Topic("BookReturned")]
        public Borrowing OnBookReturned([EventMessage] Borrowing borrowing)
            => borrowing;

        /// <summary>
        /// Subscribe to overdue book notifications
        /// </summary>
        [Subscribe]
        [Topic("OverdueNotification")]
        public Borrowing OnOverdueNotification([EventMessage] Borrowing borrowing)
            => borrowing;

        /// <summary>
        /// Subscribe to library statistics updates
        /// </summary>
        [Subscribe]
        [Topic("StatsUpdated")]
        public LibraryStats OnStatsUpdated([EventMessage] LibraryStats stats)
            => stats;

        /// <summary>
        /// Subscribe to book availability changes
        /// </summary>
        [Subscribe]
        [Topic("AvailabilityChanged")]
        public BookAvailabilityUpdate OnAvailabilityChanged([EventMessage] BookAvailabilityUpdate update)
            => update;
    }

    // Supporting types for subscriptions
    public class LibraryStats
    {
        public int TotalBooks { get; set; }
        public int AvailableBooks { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveBorrowings { get; set; }
        public int OverdueBorrowings { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class BookAvailabilityUpdate
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int CopiesAvailable { get; set; }
        public int CopiesTotal { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
