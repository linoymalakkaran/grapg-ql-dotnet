# GraphQL Learning Project with .NET 8 & HotChocolate

This comprehensive project demonstrates advanced GraphQL API implementation using .NET 8, HotChocolate, Entity Framework Core with SQLite, including real-time subscriptions, authentication, authorization, custom scalar types, and middleware.

## 🚀 Project Overview

A fully-featured library management system showcasing modern GraphQL patterns and best practices for learning and development.

## 📁 Project Structure

```
GraphQLSimple/
├── Models/                    # Data models (Book, Author, User, Review, Borrowing)
├── Data/                      # Entity Framework DbContext and seed data
├── GraphQL/                   # GraphQL Types, Queries, Mutations, Subscriptions
│   ├── Types/                 # Custom scalar types and input types
│   ├── DataLoaders/           # DataLoader implementations for N+1 problem
│   ├── Auth/                  # Authentication mutations and types
│   ├── Query.cs               # GraphQL queries with filtering, sorting, projection
│   ├── Mutation.cs            # GraphQL mutations with validation
│   ├── Subscription.cs        # Real-time subscriptions
│   └── AuthorizedOperations.cs # Protected operations with role-based access
├── Services/                  # Business logic and authentication services
├── Extensions/                # Custom middleware, validators, and extensions
├── Properties/                # Launch settings
├── GRAPHQL_FEATURES.md       # Comprehensive feature documentation
├── SUBSCRIPTIONS_AUTH_GUIDE.md # Subscriptions and auth implementation guide
└── Program.cs                # Application startup with advanced configuration
```

## ✨ Advanced Features

### 🎯 Core GraphQL Features
- ✅ **Comprehensive GraphQL API** with HotChocolate 15.x
- ✅ **Advanced Queries** with projection, filtering, and sorting
- ✅ **Complex Mutations** with validation and error handling
- ✅ **Real-time Subscriptions** with WebSocket support
- ✅ **DataLoaders** for efficient data fetching (N+1 problem resolution)
- ✅ **Custom Scalar Types** with validation (ISBN, Email, Phone, URL, PositiveInt)

### 🔐 Security & Authentication
- ✅ **Role-based Authorization** (Member, Librarian roles)
- ✅ **Permission-based Access Control** with custom attributes
- ✅ **User Authentication** with login/register mutations
- ✅ **Protected Operations** with user context validation
- ✅ **Resource-level Authorization** (users can only access their own data)

### 🛠️ Advanced Middleware
- ✅ **Custom GraphQL Middleware** with logging and monitoring
- ✅ **Rate Limiting** (100 requests/minute per IP)
- ✅ **Performance Monitoring** with request timing
- ✅ **Error Handling** with comprehensive logging
- ✅ **Request Interception** with custom headers and user context

### 📊 Data Management
- ✅ **Entity Framework Core** with SQLite database
- ✅ **Rich Data Models** with relationships and computed properties
- ✅ **Comprehensive Seed Data** for testing
- ✅ **Soft Delete** patterns for data integrity
- ✅ **Audit Fields** (CreatedAt, UpdatedAt) on all entities

### 🎛️ Developer Experience
- ✅ **Fluent Validation** with custom validators
- ✅ **AutoMapper** for object mapping
- ✅ **Serilog** for structured logging
- ✅ **Health Checks** for monitoring
- ✅ **CORS** configuration for cross-origin requests

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code
- Git

### Installation & Setup

1. **Clone the Repository**:
   ```bash
   git clone <your-repo-url>
   cd GraphQLSimple
   ```

2. **Restore Dependencies**:
   ```bash
   dotnet restore
   ```

3. **Run the Application**:
   ```bash
   dotnet run
   ```

4. **Access GraphQL Playground**:
   Navigate to `http://localhost:5000/graphql` in your browser

5. **View Documentation**:
   - GraphQL Schema: `http://localhost:5000/graphql` (Schema tab)
   - API Health: `http://localhost:5000/health`
   - Schema Download: `http://localhost:5000/schema`

## 📚 Sample Usage Examples

### 🔍 Advanced Queries with Filtering & Sorting

```graphql
# Complex query with filtering, sorting, and projection
query GetPopularBooks {
  books(
    where: { 
      reviews: { some: { rating: { gte: 4 } } },
      copiesAvailable: { gt: 0 }
    },
    order: { averageRating: DESC },
    first: 10
  ) {
    id
    title
    isbn
    averageRating
    reviewCount
    author {
      fullName
      nationality
    }
    category {
      name
    }
    reviews(order: { createdAt: DESC }, first: 3) {
      rating
      comment
      user { fullName }
      createdAt
    }
  }
}
```

### 🔐 Authentication & Authorization

```graphql
# Login to get user context
mutation Login {
  login(email: "admin@library.com", password: "admin123") {
    success
    message
    user {
      id
      fullName
      email
      userType
    }
  }
}

# Get current user's borrowings (requires authentication)
query MyBorrowings {
  myBorrowings {
    id
    book {
      title
      isbn
      author { fullName }
    }
    borrowedDate
    dueDate
    status
  }
}

# Admin-only: Get library statistics (requires Librarian role)
query LibraryStats {
  getLibraryStats {
    totalBooks
    totalUsers
    activeBorrowings
    overdueBorrowings
    generatedAt
  }
}
```

### 🎯 Custom Scalar Types in Action

```graphql
# Create book with custom scalar validation
mutation CreateBookWithValidation {
  createBook(input: {
    title: "Learning GraphQL"
    isbn: "978-1-234567-89-0"    # ISBN validation applied
    pages: 350                    # PositiveInt validation
    price: 29.99
    authorId: 1
    categoryId: 2
    publishedDate: "2024-12-01"
    copiesTotal: 100             # PositiveInt validation
    copiesAvailable: 100         # PositiveInt validation
    publisher: "Tech Books Inc"
  }) {
    id
    title
    isbn    # Returns validated ISBN format
    pages   # Returns positive integer
    author { fullName }
  }
}

# Create user with email and phone validation
mutation RegisterUser {
  register(
    input: {
      firstName: "John"
      lastName: "Doe"
      email: "JOHN@EXAMPLE.COM"      # Email normalization applied
      phoneNumber: "+1-555-123-4567" # Phone format validation
      dateOfBirth: "1990-01-01"
      userType: MEMBER
    }
    password: "securePassword123"
  ) {
    success
    user {
      email        # Returns: "john@example.com" (normalized)
      phoneNumber  # Returns validated phone format
    }
  }
}
```

### 🔄 Real-time Subscriptions

```graphql
# Subscribe to book creation events
subscription BookCreatedEvents {
  onBookCreated {
    id
    title
    isbn
    author {
      fullName
    }
    createdAt
  }
}

# Subscribe to borrowing events
subscription BorrowingEvents {
  onBookBorrowed {
    id
    book {
      title
      author { fullName }
    }
    user {
      fullName
    }
    borrowedDate
    dueDate
  }
}

# Subscribe to user-specific notifications
subscription UserNotifications {
  onUserNotification(userId: 1) {
    id
    title
    message
    type
    createdAt
  }
}
```

### 🛠️ Advanced Mutations with Validation

```graphql
# Protected: Borrow book (requires authentication)
mutation BorrowBook {
  borrowBookAuthorized(bookId: 1) {
    id
    book {
      title
      author { fullName }
    }
    borrowedDate
    dueDate
    status
  }
}

# Admin-only: Create book (requires Librarian role)
mutation CreateBookAdmin {
  createBookAuthorized(input: {
    title: "Advanced GraphQL"
    isbn: "978-0-123456-78-9"
    pages: 500
    price: 49.99
    authorId: 2
    categoryId: 1
    copiesTotal: 20
    copiesAvailable: 20
    publishedDate: "2024-11-01"
    publisher: "Advanced Tech Publishing"
  }) {
    id
    title
    author { fullName }
    createdAt
  }
}
```

## 🎛️ Custom Scalar Types

The project implements comprehensive custom scalar validation:

| Scalar Type | Purpose | Validation |
|-------------|---------|------------|
| `ISBN` | Book identification | ISBN-10/ISBN-13 format validation |
| `Email` | User email addresses | Email format + lowercase normalization |
| `Phone` | Phone numbers | International phone format validation |
| `URL` | Web addresses | Valid URL format validation |
| `PositiveInt` | Positive numbers | Ensures integers > 0 |

### Usage in Schema:
```graphql
type Book {
  isbn: ISBN!           # Custom ISBN validation
  pages: PositiveInt!   # Must be positive integer
}

type User {
  email: Email!         # Email validation + normalization
  phoneNumber: Phone    # Phone format validation
}
```

## 🔄 Subscription Events

| Event Topic | Triggered By | Subscription Method |
|-------------|-------------|-------------------|
| `BookCreated` | Book creation mutation | `onBookCreated` |
| `BookUpdated` | Book update mutation | `onBookUpdated` |
| `BookBorrowed` | Book borrow mutation | `onBookBorrowed` |
| `BookReturned` | Book return mutation | `onBookReturned` |
| `ReviewCreated` | Review creation | `onReviewCreated` |
| `UserNotification_{userId}` | System events | `onUserNotification` |

## 🔐 Authentication & Roles

### Built-in Test Users:
```json
{
  "admin@library.com": {
    "password": "admin123",
    "role": "Librarian",
    "permissions": ["CanManageBooks", "CanManageUsers", "CanViewReports"]
  },
  "john@example.com": {
    "password": "password123",
    "role": "Member", 
    "permissions": ["CanBorrowBooks", "CanWriteReviews"]
  },
  "librarian@library.com": {
    "password": "librarian123",
    "role": "Librarian",
    "permissions": ["CanManageBooks", "CanManageUsers", "CanViewReports"]
  }
}
```

### Authorization Levels:
- **Public**: Book listings, author information
- **Authenticated**: Borrowing, reviews, profile management
- **Librarian**: User management, book CRUD, statistics

## 📊 Database Schema

### Core Entities:
- **Books**: Title, ISBN, pages, price, availability
- **Authors**: Personal info, biography, book relationships
- **Users**: Authentication, profile, borrowing history
- **Reviews**: Ratings, comments, verification status
- **Borrowings**: Loan tracking, due dates, return status
- **Categories**: Book categorization and organization

### Rich Relationships:
- Authors ↔ Books (One-to-Many)
- Users ↔ Borrowings (One-to-Many)
- Books ↔ Reviews (One-to-Many)
- Books ↔ Categories (Many-to-One)
- Books ↔ Tags (Many-to-Many)

## 🧪 Testing & Development

### Testing Subscriptions:
1. Open two GraphQL Playground tabs
2. Start subscription in Tab 1
3. Execute triggering mutation in Tab 2
4. Observe real-time updates in Tab 1

### Testing Authentication:
1. Execute login mutation
2. Copy user context (in production: JWT token)
3. Use authenticated operations
4. Test role-based restrictions

### Custom Scalar Testing:
- Try invalid ISBN formats (should fail validation)
- Test email normalization (uppercase → lowercase)
- Verify phone number format validation
- Test positive integer constraints

## 🏗️ Architecture Patterns

### GraphQL Best Practices:
- **DataLoader Pattern** for N+1 problem resolution
- **Relay Cursor Pagination** for large datasets  
- **Input Type Validation** with FluentValidation
- **Error Handling** with custom exception types
- **Schema-First Design** with code-first implementation

### Security Patterns:
- **Role-Based Access Control (RBAC)**
- **Permission-Based Authorization**
- **Resource-Level Security** (users own their data)
- **Input Sanitization** and **Output Encoding**
- **Rate Limiting** and **Request Throttling**

## 📈 Performance Features

### Optimization Techniques:
- **Projection** - Only query requested fields
- **DataLoaders** - Batch database queries
- **Filtering at DB Level** - Efficient WHERE clauses
- **Connection Pooling** - Optimized EF Core configuration
- **Response Caching** - Built-in GraphQL caching

### Monitoring & Observability:
- **Structured Logging** with Serilog
- **Performance Metrics** - Request timing
- **Health Checks** - Application monitoring
- **Error Tracking** - Comprehensive exception handling

## 🛠️ Technology Stack

- **Backend**: .NET 8, HotChocolate GraphQL 15.x
- **Database**: Entity Framework Core with SQLite
- **Authentication**: Custom role-based system (demo)
- **Validation**: FluentValidation with custom validators
- **Logging**: Serilog with structured logging
- **Mapping**: AutoMapper for object transformations
- **Real-time**: GraphQL subscriptions over WebSocket

## 📖 Additional Resources

### Documentation Files:
- `GRAPHQL_FEATURES.md` - Comprehensive feature guide
- `SUBSCRIPTIONS_AUTH_GUIDE.md` - Detailed subscription and auth implementation
- `PROJECT_STATUS.md` - Development progress and status
- `TESTING_GUIDE.md` - Testing procedures and examples

### Learning Resources:
- [HotChocolate Documentation](https://chillicream.com/docs/hotchocolate)
- [GraphQL Specification](https://graphql.org/learn/)
- [Entity Framework Core Guide](https://docs.microsoft.com/ef/core/)

## 🤝 Contributing

This is a learning project designed for educational purposes. Feel free to:
- Explore the codebase
- Test different GraphQL patterns
- Experiment with authentication flows
- Try advanced subscription scenarios
- Add new custom scalar types
- Implement additional middleware

## 📝 License

This project is for educational purposes and learning GraphQL concepts with .NET 8 and HotChocolate.

---

**Happy Learning! 🚀** 

Explore the comprehensive GraphQL implementation with real-time features, advanced security, and custom types!
      lastName
    }
    reviews {
      id
      reviewerName
      rating
      comment
    }
  }
}
```

### 2. Get Specific Author with Books
```graphql
query {
  author(id: 1) {
    id
    firstName
    lastName
    dateOfBirth
    biography
    books {
      id
      title
      isbn
    }
  }
}
```

### 3. Get All Authors
```graphql
query {
  authors {
    id
    firstName
    lastName
    books {
      title
    }
  }
}
```

### 4. Get All Reviews
```graphql
query {
  reviews {
    id
    reviewerName
    rating
    comment
    createdAt
    book {
      title
      author {
        firstName
        lastName
      }
    }
  }
}
```

## Sample Mutations

### 1. Add New Author
```graphql
mutation {
  addAuthor(
    firstName: "Agatha"
    lastName: "Christie"
    dateOfBirth: "1890-09-15T00:00:00"
    biography: "British crime novelist"
  ) {
    id
    firstName
    lastName
  }
}
```

### 2. Add New Book
```graphql
mutation {
  addBook(
    title: "Murder on the Orient Express"
    isbn: "978-0007119318"
    publishedDate: "1934-01-01T00:00:00"
    pages: 256
    authorId: 3
  ) {
    id
    title
    author {
      firstName
      lastName
    }
  }
}
```

### 3. Add New Review
```graphql
mutation {
  addReview(
    bookId: 1
    reviewerName: "John Doe"
    rating: 5
    comment: "Absolutely fantastic!"
  ) {
    id
    reviewerName
    rating
    book {
      title
    }
  }
}
```

## Advanced Queries with Filtering

HotChocolate automatically provides filtering capabilities:

```graphql
query {
  books(where: { 
    author: { 
      firstName: { eq: "J.K." } 
    } 
  }) {
    title
    author {
      firstName
      lastName
    }
  }
}
```

## Technology Stack

- **.NET 8** - Latest .NET framework
- **HotChocolate 15.1.9** - GraphQL server
- **Entity Framework Core 9.0** - ORM
- **SQLite** - Database (portable, no setup required)

## Database Schema

The application creates the following tables:
- **Authors** (Id, FirstName, LastName, DateOfBirth, Biography)
- **Books** (Id, Title, ISBN, PublishedDate, Pages, AuthorId)
- **Reviews** (Id, BookId, ReviewerName, Rating, Comment, CreatedAt)

## Sample Data Included

The application comes with sample data:
- **Authors**: J.K. Rowling, George Orwell
- **Books**: Harry Potter and the Philosopher's Stone, 1984
- **Reviews**: Several reviews for the sample books

## Next Steps for Learning

1. **Try the sample queries** in the GraphQL playground
2. **Experiment with filtering** using `where` clauses
3. **Add more complex relationships** between entities
4. **Implement authentication** and authorization
5. **Add subscriptions** for real-time updates
6. **Add custom scalars** for specialized data types
7. **Implement DataLoaders** to solve N+1 query problems

## Troubleshooting

- Make sure you're running from the `GraphQLSimple` directory
- The SQLite database file (`library.db`) will be created automatically
- If you get port conflicts, the application will use a different port automatically

Happy GraphQL learning! 🚀
