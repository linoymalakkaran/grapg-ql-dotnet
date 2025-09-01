# GraphQL Subscriptions and Authentication Guide

## 🔄 Subscriptions Explained

### The Subscription Mismatch Issue You Found:

You correctly identified that we were sending `"BookReturned"` events but the subscription was listening for different topic names. Here's the complete mapping:

### Event Topics vs Subscription Methods:

| Mutation Event | Subscription Method | Topic Match |
|---------------|-------------------|-------------|
| `"BookCreated"` | `OnBookCreated` | ✅ Matches |
| `"BookUpdated"` | `OnBookUpdated` | ✅ Matches |
| `"BookReturned"` | `OnBookReturned` | ✅ Matches |
| `"BookBorrowed"` | `OnBookBorrowed` | ✅ Added |
| `"BorrowingCreated"` | `OnBorrowingCreated` | ✅ Added |
| `"ReviewCreated"` | `OnReviewCreated` | ✅ Matches |

### How Subscriptions Work:

1. **Client Subscribes**: Opens WebSocket connection
2. **Server Event**: Mutation triggers `eventSender.SendAsync("TopicName", data)`
3. **HotChocolate**: Matches topic to `[Topic("TopicName")]` attribute
4. **Client Receives**: Data pushed to subscriber

### Sample Subscription Usage:

```graphql
# Subscribe to book returns in GraphQL playground
subscription {
  onBookReturned {
    id
    bookId
    userId
    returnedDate
    status
    book {
      title
      isbn
    }
    user {
      fullName
      email
    }
  }
}
```

### JavaScript Client Example:
```javascript
import { createClient } from 'graphql-ws';

const wsClient = createClient({
  url: 'ws://localhost:5000/graphql',
});

// Subscribe to book returns
const unsubscribe = wsClient.subscribe({
  query: `
    subscription {
      onBookReturned {
        id
        book { title }
        user { fullName }
        returnedDate
      }
    }
  `,
}, {
  next: (result) => {
    console.log('Book returned:', result.data.onBookReturned);
    // Update UI with real-time data
  },
  error: (error) => console.error(error),
});
```

## 🔐 Authentication & Authorization

### Current Implementation:

1. **Simple Authentication Service** (demo purposes)
2. **Role-based Access Control**
3. **Permission-based Authorization**
4. **User Context in GraphQL**

### Available Roles:
- **Member**: Regular library users
- **Librarian**: Library staff with admin privileges

### Available Permissions:
- `CanBorrowBooks`
- `CanWriteReviews`
- `CanManageBooks` (Librarian only)
- `CanManageUsers` (Librarian only)
- `CanViewReports` (Librarian only)

### Authentication Flow:

#### 1. Login Mutation:
```graphql
mutation {
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
```

#### 2. Register Mutation:
```graphql
mutation {
  register(
    input: {
      firstName: "John"
      lastName: "Doe"
      email: "john@example.com"
      phoneNumber: "+1-555-123-4567"
      dateOfBirth: "1990-01-01"
      userType: MEMBER
    }
    password: "password123"
  ) {
    success
    message
    user {
      id
      fullName
      email
    }
  }
}
```

#### 3. Get Current User:
```graphql
query {
  me {
    id
    fullName
    email
    userType
    borrowings {
      id
      book { title }
      dueDate
      status
    }
  }
}
```

### Authorization Examples:

#### 1. User-Specific Queries:
```graphql
# Get my borrowings (authenticated users only)
query {
  myBorrowings {
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

# Get my reviews
query {
  myReviews {
    id
    rating
    comment
    book { title }
    createdAt
  }
}
```

#### 2. Admin-Only Queries:
```graphql
# Librarian only - Get all users
query {
  getAllUsers {
    id
    fullName
    email
    userType
    isActive
    borrowings {
      id
      status
    }
  }
}

# Librarian only - Get library statistics
query {
  getLibraryStats {
    totalBooks
    totalUsers
    activeBorrowings
    overdueBorrowings
    generatedAt
  }
}
```

#### 3. Protected Mutations:
```graphql
# Borrow a book (authenticated users)
mutation {
  borrowBookAuthorized(bookId: 1) {
    id
    book { title }
    borrowedDate
    dueDate
    status
  }
}

# Create book (librarian only)
mutation {
  createBookAuthorized(input: {
    title: "New Book"
    isbn: "978-1234567890"
    pages: 300
    price: 19.99
    authorId: 1
    categoryId: 1
    copiesTotal: 10
    copiesAvailable: 10
    publishedDate: "2024-01-01"
    publisher: "Test Publisher"
  }) {
    id
    title
  }
}
```

### Built-in Test Users:

```json
{
  "admin@library.com": {
    "password": "admin123",
    "role": "Librarian"
  },
  "john@example.com": {
    "password": "password123", 
    "role": "Member"
  },
  "librarian@library.com": {
    "password": "librarian123",
    "role": "Librarian"
  }
}
```

### Authorization Patterns:

#### 1. Role-Based:
```csharp
[Authorize(Roles = new[] { "Librarian" })]
public async Task<Book> CreateBook(...)
```

#### 2. User-Specific Resources:
```csharp
[Authorize]
public async Task<Borrowing?> ReturnBook(int borrowingId, ...)
{
    // Check if user owns this borrowing
    if (borrowing.UserId != currentUserId && !user.IsInRole("Librarian"))
    {
        throw new GraphQLException("You can only return your own books");
    }
}
```

#### 3. Permission-Based:
```csharp
public async Task<Book> UpdateBook(...)
{
    if (!claimsPrincipal.CanManageBooks())
    {
        throw new GraphQLException("You don't have permission to update books");
    }
}
```

## 🧪 Testing Subscriptions & Auth

### 1. Test Subscriptions:

1. Open two browser tabs with GraphQL Playground (`http://localhost:5000/graphql`)
2. In Tab 1: Start a subscription
3. In Tab 2: Execute a mutation that triggers the event
4. Watch Tab 1 receive real-time updates

### 2. Test Authentication:

#### Step 1: Login
```graphql
mutation {
  login(email: "admin@library.com", password: "admin123") {
    success
    user { id, fullName, userType }
  }
}
```

#### Step 2: Set Headers (if using JWT - not implemented in simple version)
```json
{
  "Authorization": "Bearer your-jwt-token-here"
}
```

#### Step 3: Test Protected Operations
```graphql
query {
  me {
    id
    fullName
  }
}
```

### Real-World Production Considerations:

#### 1. JWT Implementation:
- Proper token generation and validation
- Refresh token mechanism
- Secure token storage

#### 2. Password Security:
- Bcrypt/Argon2 password hashing
- Password complexity requirements
- Account lockout policies

#### 3. Advanced Authorization:
- Resource-based permissions
- Dynamic role assignment
- Permission inheritance

#### 4. Subscription Security:
- User-specific subscriptions
- Rate limiting for subscription events
- Connection authentication

### Custom Scalar Types Integration:

The custom scalar types (ISBN, Email, Phone, etc.) work seamlessly with authentication:

```graphql
# Email validation applied during registration
mutation {
  register(
    input: {
      firstName: "John"
      lastName: "Doe"
      email: "JOHN@EXAMPLE.COM"  # Will be normalized to lowercase
      phoneNumber: "+1-555-123-4567"  # Phone format validated
    }
    password: "password123"
  ) {
    user {
      email  # Returns: "john@example.com"
      phoneNumber  # Returns validated format
    }
  }
}
```

This implementation provides a solid foundation for learning GraphQL subscriptions, authentication, and authorization patterns!
