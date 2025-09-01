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
        /// Subscribe to borrowing events (when books are borrowed)
        /// </summary>
        [Subscribe]
        [Topic("BookBorrowed")]
        public Borrowing OnBookBorrowed([EventMessage] Borrowing borrowing)
            => borrowing;

        /// <summary>
        /// Subscribe to borrowing creation events
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

        /// <summary>
        /// Subscribe to user-specific notifications
        /// </summary>
        [Subscribe]
        [Topic("UserNotification_{userId}")]
        public UserNotification OnUserNotification([EventMessage] UserNotification notification, int userId)
            => notification;

        /// <summary>
        /// Subscribe to author updates
        /// </summary>
        [Subscribe]
        [Topic("AuthorUpdated")]
        public Author OnAuthorUpdated([EventMessage] Author author)
            => author;

        /// <summary>
        /// Subscribe to book deletion events
        /// </summary>
        [Subscribe]
        [Topic("BookDeleted")]
        public BookDeletionEvent OnBookDeleted([EventMessage] BookDeletionEvent bookDeletion)
            => bookDeletion;
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

    public class UserNotification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class BookDeletionEvent
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
        public string DeletedBy { get; set; } = string.Empty;
    }

    public enum NotificationType
    {
        BookDue,
        BookOverdue,
        BookAvailable,
        ReviewPosted,
        AccountUpdate,
        SystemNotification
    }
}
