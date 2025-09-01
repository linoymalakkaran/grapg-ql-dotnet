# GraphQL Features Guide

This document explains the GraphQL features implemented in this learning project, including middleware, subscriptions, and custom scalar types.

## 🔍 GraphQL Query Features

### 1. UseProjection
**What it does**: Optimizes database queries by only selecting fields requested by the client.

**Example**:
```graphql
# This query will only select id, title, and ISBN from the database
query {
  books {
    id
    title
    isbn
  }
}
```

### 2. UseFiltering
**What it does**: Enables dynamic filtering on collections using the `where` clause.

**Examples**:
```graphql
# Filter books by title containing "Harry"
query {
  books(where: { title: { contains: "Harry" } }) {
    id
    title
    author { fullName }
  }
}

# Filter books by price range and publication date
query {
  books(where: { 
    and: [
      { price: { gte: 10.00 } },
      { price: { lte: 50.00 } },
      { publishedDate: { gte: "2020-01-01" } }
    ]
  }) {
    title
    price
    publishedDate
  }
}

# Complex filtering with nested properties
query {
  books(where: { 
    author: { 
      nationality: { eq: "British" } 
    }
  }) {
    title
    author { fullName, nationality }
  }
}
```

### 3. UseSorting
**What it does**: Enables dynamic sorting using the `order` clause.

**Examples**:
```graphql
# Sort books by title ascending
query {
  books(order: { title: ASC }) {
    title
    publishedDate
  }
}

# Multiple sort criteria
query {
  books(order: [
    { publishedDate: DESC },
    { title: ASC }
  ]) {
    title
    publishedDate
    price
  }
}
```

## 🔄 GraphQL Subscriptions

Subscriptions enable real-time updates when data changes. Here's how they work:

### Available Subscriptions:

1. **onBookCreated** - Triggered when a new book is added
2. **onBookUpdated** - Triggered when a book is modified  
3. **onReviewCreated** - Triggered when a new review is posted
4. **onBorrowingCreated** - Triggered when a book is borrowed
5. **onBookReturned** - Triggered when a book is returned

### How to Use Subscriptions:

**Client-side subscription example**:
```graphql
subscription {
  onBookCreated {
    id
    title
    isbn
    author {
      fullName
    }
  }
}
```

**WebSocket Connection** (JavaScript example):
```javascript
// Using graphql-ws library
import { createClient } from 'graphql-ws';

const client = createClient({
  url: 'ws://localhost:5000/graphql',
});

// Subscribe to book creation events
const unsubscribe = client.subscribe(
  {
    query: `
      subscription {
        onBookCreated {
          id
          title
          author { fullName }
        }
      }
    `,
  },
  {
    next: (data) => {
      console.log('New book created:', data);
    },
    error: (err) => {
      console.error('Subscription error:', err);
    },
    complete: () => {
      console.log('Subscription completed');
    },
  }
);
```

### How Subscriptions are Triggered:

When you create a book via mutation:
```graphql
mutation {
  createBook(input: {
    title: "New Book"
    isbn: "978-1234567890"
    authorId: 1
    categoryId: 1
    pages: 300
    price: 19.99
    publisher: "Test Publisher"
    copiesTotal: 10
    copiesAvailable: 10
    publishedDate: "2024-01-01"
  }) {
    id
    title
  }
}
```

All clients subscribed to `onBookCreated` will receive the new book data in real-time.

## 🛡️ Custom Scalar Types

Custom scalar types provide validation and type safety:

### Available Custom Scalars:

1. **ISBN** - Validates ISBN-10 and ISBN-13 formats
2. **Email** - Validates email addresses and normalizes to lowercase
3. **Phone** - Validates phone number formats
4. **URL** - Validates URL format
5. **PositiveInt** - Ensures integers are positive (> 0)

### Usage in Schema:

```graphql
# These fields use custom scalar validation
input CreateBookInput {
  title: String!
  isbn: ISBN!          # Custom ISBN validation
  pages: PositiveInt!  # Must be > 0
  authorId: Int!
  categoryId: Int!
}

input CreateUserInput {
  firstName: String!
  lastName: String!
  email: Email!        # Custom email validation
  phoneNumber: Phone   # Custom phone validation
}
```

### Validation Examples:

**Valid ISBN formats**:
- `"978-0-123456-78-9"` (ISBN-13 with hyphens)
- `"9780123456789"` (ISBN-13 without hyphens)
- `"0123456789"` (ISBN-10)

**Valid Email formats**:
- `"user@example.com"` (will be stored as lowercase)
- `"User.Name+tag@Example.COM"` (normalized to lowercase)

**Valid Phone formats**:
- `"+1-555-123-4567"`
- `"(555) 123-4567"`
- `"+44 20 7946 0958"`

## 🔧 GraphQL Middleware

### Custom Middleware Features:

1. **Request Logging** - Logs all GraphQL requests with unique IDs
2. **Performance Monitoring** - Tracks request execution time
3. **Authentication Context** - Adds user information to GraphQL context
4. **Rate Limiting** - Limits requests per IP (100 requests/minute)
5. **Error Handling** - Comprehensive error logging and handling

### Middleware Components:

1. **GraphQLMiddleware** - Main middleware for logging and monitoring
2. **GraphQLRateLimitingMiddleware** - Prevents API abuse
3. **GraphQLRequestInterceptor** - Processes requests before execution
4. **GraphQLLoggingInterceptor** - Detailed query execution logging

### Sample Log Output:

```
[12:34:56 INF] GraphQL Request [abc12345] started from 127.0.0.1
[12:34:56 INF] GraphQL Query [abc12345] executed successfully in 145ms
```

## 📊 Advanced Query Examples

### Complex Filtering and Sorting:
```graphql
query GetPopularBooks {
  books(
    where: { 
      reviews: { some: { rating: { gte: 4 } } },
      copiesAvailable: { gt: 0 }
    },
    order: { averageRating: DESC },
    first: 10
  ) {
    title
    averageRating
    reviewCount
    author { fullName }
    reviews(order: { createdAt: DESC }, first: 3) {
      rating
      comment
      user { fullName }
    }
  }
}
```

### Using Custom Scalars in Queries:
```graphql
mutation CreateBookWithValidation {
  createBook(input: {
    title: "Learning GraphQL"
    isbn: "978-1-234567-89-0"    # ISBN validation applied
    pages: 350                    # PositiveInt validation applied
    price: 29.99
    authorId: 1
    categoryId: 2
    publishedDate: "2024-12-01"
    copiesTotal: 100             # PositiveInt validation applied
    copiesAvailable: 100         # PositiveInt validation applied
    publisher: "Tech Books Inc"
  }) {
    id
    title
    isbn    # Returns validated ISBN
    pages   # Returns validated positive integer
  }
}
```

### Real-time Dashboard with Subscriptions:
```graphql
subscription LibraryDashboard {
  onBookCreated { id, title }
  onReviewCreated { rating, bookId }
  onBorrowingCreated { bookId, userId, dueDate }
}
```

## 🚀 Getting Started

1. **Start the server**: `dotnet run`
2. **Open GraphQL IDE**: Navigate to `http://localhost:5000/graphql`
3. **Try the examples** above in the GraphQL playground
4. **Monitor logs** to see middleware in action
5. **Test subscriptions** using WebSocket connections

## 🛠️ Development Tips

- Use the GraphQL playground to explore the schema
- Check server logs to understand middleware behavior  
- Test custom scalar validation with invalid data
- Use subscriptions for real-time features like notifications
- Combine filtering, sorting, and projection for optimal performance

This project demonstrates a production-ready GraphQL API with advanced features for learning and experimentation.
