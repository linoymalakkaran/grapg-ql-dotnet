# GraphQL Authorization & Subscription Mechanics Explained

## 🔐 How `[Authorize]` Attribute Works

### 1. **Authorization Flow in HotChocolate GraphQL**

```csharp
[Authorize(Roles = new[] { "Librarian" })]
public async Task<Book> CreateBookAuthorized(
    CreateBookInput input,
    [Service] BookService bookService,
    ClaimsPrincipal claimsPrincipal)  // <-- This gets injected automatically!
{
    // Additional permission check
    if (!claimsPrincipal.CanManageBooks())
    {
        throw new GraphQLException("You don't have permission to create books");
    }
    
    return await bookService.CreateAsync(input);
}
```

### **Step-by-Step Authorization Process:**

1. **Request Arrives**: Client sends GraphQL request to `/graphql`
2. **Middleware Chain**: Request passes through authentication middleware
3. **User Identity**: If authenticated, `ClaimsPrincipal` is created with user claims
4. **GraphQL Execution**: HotChocolate checks `[Authorize]` attribute
5. **Role Validation**: Framework verifies user has required role ("Librarian")
6. **Method Injection**: `ClaimsPrincipal` is automatically injected as parameter
7. **Custom Logic**: Additional business logic checks (like `CanManageBooks()`)
8. **Execution**: If all checks pass, method executes

### **Authorization Attribute Variants:**

```csharp
// 1. Basic authentication required
[Authorize]
public async Task<User> GetMe(ClaimsPrincipal claimsPrincipal) { ... }

// 2. Specific role required
[Authorize(Roles = new[] { "Librarian" })]
public async Task<Book> DeleteBook(int id) { ... }

// 3. Multiple roles allowed
[Authorize(Roles = new[] { "Librarian", "Admin" })]
public async Task<LibraryStats> GetStats() { ... }

// 4. Policy-based (more advanced)
[Authorize(Policy = "CanManageBooks")]
public async Task<Book> CreateBook(...) { ... }
```

## 🔄 Subscription Event Flow: Publisher → Subscriber

### **The Complete Journey of `await eventSender.SendAsync("BookReturned", borrowing);`**

#### **Publisher Side (Mutation):**
```csharp
// In Mutation.cs
public async Task<Borrowing?> ReturnBook(
    int borrowingId,
    [Service] BorrowingService borrowingService,
    [Service] ITopicEventSender eventSender)  // <-- HotChocolate injects this
{
    var borrowing = await borrowingService.ReturnBookAsync(borrowingId);
    
    if (borrowing != null)
    {
        // 🚀 PUBLISH EVENT: This sends the event to the subscription system
        await eventSender.SendAsync("BookReturned", borrowing);
        //                    ↑ Topic Name    ↑ Data
    }

    return borrowing;
}
```

#### **Subscriber Side (Subscription):**
```csharp
// In Subscription.cs
[Subscribe]                           // ← Marks this as a subscription method
[Topic("BookReturned")]              // ← Listens to this exact topic name
public Borrowing OnBookReturned([EventMessage] Borrowing borrowing)
//                ↑ Method name       ↑ Receives the published data
    => borrowing;                    // ← Returns data to subscribed clients
```

#### **Client Side (WebSocket Connection):**
```graphql
# Client subscribes to this query
subscription {
  onBookReturned {     # ← This maps to OnBookReturned method
    id
    bookId
    userId
    returnedDate
    book {
      title
      author { fullName }
    }
    user {
      fullName
    }
  }
}
```

### **Internal HotChocolate Magic:**

1. **Topic Registration**: `[Topic("BookReturned")]` registers the subscription method
2. **Event Bus**: `ITopicEventSender` uses an internal event bus (in-memory by default)
3. **WebSocket Connection**: Clients connect via WebSocket to `/graphql`
4. **Event Matching**: When `SendAsync("BookReturned", data)` is called:
   - HotChocolate finds all methods with `[Topic("BookReturned")]`
   - Calls those methods with the provided data
   - Sends results to all subscribed WebSocket clients
5. **Real-time Delivery**: Clients receive updates immediately

### **Visual Flow Diagram:**
```
Client Mutation Request
        ↓
   Mutation Method
        ↓
 eventSender.SendAsync("BookReturned", data)
        ↓
   HotChocolate Event Bus
        ↓
  Find [Topic("BookReturned")] methods
        ↓
   Call OnBookReturned(data)
        ↓
   Send to WebSocket Clients
        ↓
   Client receives real-time update
```

## 🔌 `ClaimsPrincipal` Injection Mechanism

### **How `ClaimsPrincipal claimsPrincipal` Gets Injected:**

#### **1. ASP.NET Core Integration:**
```csharp
// In Program.cs - this is already configured
builder.Services.AddHttpContextAccessor();
```

#### **2. HotChocolate Automatic Injection:**
HotChocolate automatically recognizes these parameter types and injects them:

- `ClaimsPrincipal` - Current user's identity and claims
- `HttpContext` - Current HTTP request context
- `CancellationToken` - For cancellation support
- `[Service] SomeService` - Services from DI container

#### **3. Behind the Scenes:**
```csharp
// This is what HotChocolate does internally (simplified):
public class GraphQLParameterInjector
{
    public object[] GetParameters(MethodInfo method, HttpContext httpContext)
    {
        var parameters = new List<object>();
        
        foreach (var param in method.GetParameters())
        {
            if (param.ParameterType == typeof(ClaimsPrincipal))
            {
                // Get user from HTTP context
                parameters.Add(httpContext.User);
            }
            else if (param.HasAttribute<ServiceAttribute>())
            {
                // Get from DI container
                parameters.Add(httpContext.RequestServices.GetService(param.ParameterType));
            }
            // ... other parameter types
        }
        
        return parameters.ToArray();
    }
}
```

### **What's Inside ClaimsPrincipal:**

```csharp
public async Task<User?> ExampleMethod(ClaimsPrincipal claimsPrincipal)
{
    // Available claim types:
    var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
    var name = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
    var roles = claimsPrincipal.FindAll(ClaimTypes.Role).Select(c => c.Value);
    
    // Custom extension methods we created:
    var canManageBooks = claimsPrincipal.CanManageBooks();
    var userIdInt = claimsPrincipal.GetUserId();
    
    // Check if authenticated:
    if (!claimsPrincipal.Identity?.IsAuthenticated == true)
    {
        throw new GraphQLException("Authentication required");
    }
    
    return null;
}
```

## 🧪 **Testing the Flow**

### **1. Test Subscription Event Flow:**

#### **Terminal 1 - Start Subscription:**
```graphql
subscription {
  onBookReturned {
    id
    book { title }
    user { fullName }
    returnedDate
  }
}
```

#### **Terminal 2 - Trigger Event:**
```graphql
mutation {
  returnBook(borrowingId: 1) {
    id
    returnedDate
  }
}
```

#### **Result:** Terminal 1 immediately receives the returned book data!

### **2. Test Authorization Flow:**

#### **Step 1 - Try without auth (should fail):**
```graphql
mutation {
  createBookAuthorized(input: {
    title: "Test Book"
    isbn: "978-1234567890"
    # ... other fields
  }) {
    id
    title
  }
}
# Result: Error - Authentication required
```

#### **Step 2 - Login first:**
```graphql
mutation {
  login(email: "admin@library.com", password: "admin123") {
    success
    user { userType }
  }
}
```

#### **Step 3 - Try again with auth (should work):**
```graphql
mutation {
  createBookAuthorized(input: {
    title: "Test Book"
    isbn: "978-1234567890"
    # ... other fields
  }) {
    id
    title
  }
}
# Result: Success - Book created
```

## 🔧 **Advanced Configuration**

### **Custom Authorization Policies:**
```csharp
// In Program.cs (for more advanced scenarios)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageBooks", policy =>
        policy.RequireRole("Librarian")
              .RequireClaim("Permission", "CanManageBooks"));
    
    options.AddPolicy("CanAccessUserData", policy =>
        policy.RequireAuthenticatedUser()
              .AddRequirements(new ResourceOwnerRequirement()));
});
```

### **Custom Subscription Authorization:**
```csharp
[Subscribe]
[Topic("UserNotification_{userId}")]
[Authorize] // ← Require authentication for subscription
public UserNotification OnUserNotification(
    [EventMessage] UserNotification notification, 
    int userId,
    ClaimsPrincipal claimsPrincipal)
{
    // Only allow users to subscribe to their own notifications
    var currentUserId = claimsPrincipal.GetUserId();
    if (currentUserId != userId)
    {
        throw new GraphQLException("You can only subscribe to your own notifications");
    }
    
    return notification;
}
```

## 📋 **Key Takeaways:**

1. **`[Authorize]`** is processed by HotChocolate before method execution
2. **`ClaimsPrincipal`** is automatically injected from HTTP context
3. **Subscription events** use topic-based routing (`SendAsync` → `[Topic]`)
4. **Real-time updates** flow through HotChocolate's event bus to WebSocket clients
5. **Security** can be applied at multiple levels (method, type, field)

This architecture provides a clean separation between authentication, authorization, and business logic while enabling real-time features!
