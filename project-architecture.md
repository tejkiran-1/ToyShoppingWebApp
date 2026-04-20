
***

# ✅ STEP 1: System Overview & End‑to‑End Architecture

### Toy Shopping Web App (Admin + Customer)

***

## 1️⃣ What Are We Building? (Clear & Simple)

We are building a **Toy Shopping Web Application** where:

### 👤 Users

*   **Customer**
    *   Browse toys
    *   View toy details
    *   Pagination, filtering
    *   Place orders
    *   View own orders

*   **Admin**
    *   Manage toys (CRUD)
    *   Manage categories
    *   View all orders
    *   Restricted access

***

### 🧠 Core Architectural Goals (VERY IMPORTANT)

This project is designed to be:

✅ **Enterprise‑grade**  
✅ **REST‑compliant**  
✅ **Secure**  
✅ **Scalable**  
✅ **Cloud‑ready (Azure)**  
✅ **Interview‑ready**

***

## 2️⃣ High‑Level Architecture (Bird’s‑Eye View)

    [ Angular UI ]
         |
         |  HTTPS + JWT
         v
    [ ASP.NET Core Web API ]
         |
         |  EF Core
         v
    [ SQL Database ]

    Secrets → Azure Key Vault
    Logs & Metrics → App Insights

Let’s break this down properly.

***

## 3️⃣ Frontend Architecture (Angular)

### Responsibilities of Angular (IMPORTANT)

Angular is **NOT** responsible for:
❌ Business rules  
❌ Authorization decisions  
❌ Pagination logic

Angular **IS** responsible for:
✅ UI rendering  
✅ User interaction  
✅ Sending API requests  
✅ Displaying paginated data  
✅ Hiding/showing UI based on roles

***

### Angular Key Concepts Used

*   Components (ToyList, ToyDetail, AdminDashboard)
*   Services (AuthService, ToyService, OrderService)
*   Route Guards (AdminGuard, AuthGuard)
*   Interceptors (JWT token injection)

✅ **Security rule**:

> Angular hides UI, but backend ENFORCES security.

***

## 4️⃣ Backend Architecture (ASP.NET Core Web API)

### Backend is the **source of truth**

It enforces:
✅ Authentication  
✅ Authorization  
✅ Business rules  
✅ Data access  
✅ Pagination  
✅ Validation

***

### Clean Architecture Layers (MANDATORY)

    Controller Layer
       ↓
    Service Layer
       ↓
    Repository Layer
       ↓
    Database

***

### 🔹 Controller Layer

*   Handles HTTP requests
*   No business logic
*   Returns HTTP responses
*   Uses REST standards

Example responsibility:

> “Receive request → call service → return response”

***

### 🔹 Service Layer (MOST IMPORTANT)

*   Contains **business logic**
*   Validations (stock, ownership, rules)
*   Authorization checks (if needed)

Example:

> “Can this user place an order for this toy?”

***

### 🔹 Repository Layer

*   Talks to EF Core
*   No business logic
*   Only data access

Example:

> “Get toys with pagination from DB”

***

✅ **Why this separation?**

*   Testability
*   Maintainability
*   Real‑world enterprise standard
*   Interviewers LOVE this

***

## 5️⃣ Authentication & Authorization Architecture

### Authentication (WHO are you?)

*   JWT‑based
*   Stateless
*   Token issued on login

### Authorization (WHAT can you do?)

*   Role‑based:
    *   Admin
    *   Customer
*   Enforced via:
    *   `[Authorize(Roles = "Admin")]`

***

### JWT Flow (High Level)

    Login → JWT Issued
    Client stores JWT
    Client sends JWT with every request
    API validates JWT
    Authorization applied

✅ **Stateless → Easy horizontal scaling**

***

## 6️⃣ Request Pipeline Architecture (Middleware)

### Standard Middleware Order

    Exception Handling
    HTTPS Redirection
    Routing
    Authentication
    Authorization
    MapControllers

***

### Why This Matters

*   Global exception handling
*   Secure communication
*   Correct endpoint selection
*   Proper security enforcement

✅ Middleware handles **cross‑cutting concerns**

***

## 7️⃣ Database & EF Core Architecture

### Database Design Principles

*   Relational database (SQL Server)
*   Normalized tables
*   Each micro‑concern separated

Example tables:

*   Users
*   Roles
*   Toys
*   Categories
*   Orders
*   OrderItems

***

### EF Core Usage

*   DbContext (Scoped)
*   LINQ → SQL
*   Pagination via `Skip / Take`
*   `AsNoTracking()` for reads

✅ Pagination happens at **DB level**, not UI.

***

## 8️⃣ REST API Design Principles Used

✅ Resource‑based URLs  
✅ Correct HTTP verbs  
✅ Proper status codes  
✅ Pagination, filtering, sorting  
✅ Stateless  
✅ Version‑ready

Example:

    GET /api/toys?page=1&pageSize=10
    POST /api/orders

***

## 9️⃣ Azure & Cloud Architecture

### Azure Key Vault

*   Stores:
    *   DB connection string
    *   JWT secret
*   No secrets in code
*   Loaded via configuration providers

***

### Cloud‑Ready Design

✅ Stateless backend  
✅ No in‑memory session  
✅ Externalized configuration  
✅ Horizontal scaling possible

***

## 🔟 Logging & Observability

*   Centralized logging
*   Exception logging
*   Request tracing

Example:

> “Why did order creation fail in production?”

✅ Answerable via logs.

***

## 🔟A Request Flow Example (End-to-End Trace)

### Scenario: Customer Requests Toys with Pagination & Filter

**HTTP Request:**
```
GET /api/toys?categoryId=5&page=2&pageSize=10
Headers: Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Step-by-Step Flow Through Architecture

#### **STEP 1: Angular Component (UI Layer)**
```typescript
// ToyListComponent requests toys
this.toyService.getToys(categoryId: 5, page: 2, pageSize: 10)
  .subscribe(response => {
    this.toys = response.data;
    this.totalCount = response.totalCount;
  });
```

**What happens:**
- Component calls service
- Service injects HttpClient
- Interceptor automatically adds JWT token to headers
- Request sent to backend

---

#### **STEP 2: HTTP Request Arrives at API**
```
GET /api/toys?categoryId=5&page=2&pageSize=10
Authorization: Bearer [JWT_TOKEN]
```

---

#### **STEP 3: Middleware Pipeline (in order)**
1. **Exception Handling Middleware** — catches any errors
2. **HTTPS Redirection** — ensures secure communication
3. **Routing** — maps request to correct controller/action
4. **Authentication Middleware** — validates JWT token
   - Extracts token from header
   - Verifies signature (using secret from Key Vault)
   - Extracts claims (UserId, Role, Email)
   - Attaches ClaimsPrincipal to HttpContext
5. **Authorization Middleware** — (for protected endpoints)
6. **MapControllers** — routes to controller action

---

#### **STEP 4: Controller Action**
```csharp
[ApiController]
[Route("api/[controller]")]
public class ToysController : ControllerBase
{
    private readonly IToyService _toyService;
    
    public ToysController(IToyService toyService)
    {
        _toyService = toyService;
    }
    
    // No [Authorize] needed — customers can view toys
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ToyDto>>> GetToys(
        [FromQuery] int categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _toyService.GetToysAsync(categoryId, page, pageSize);
        return Ok(result);
    }
}
```

**Controller responsibility:**
- Receive HTTP request
- Validate inputs (page, pageSize not negative)
- Call service
- Return HTTP response (200 OK)
- **NO business logic here**

---

#### **STEP 5: Service Layer (Business Logic)**
```csharp
public class ToyService : IToyService
{
    private readonly IToyRepository _toyRepository;
    
    public ToyService(IToyRepository toyRepository)
    {
        _toyRepository = toyRepository;
    }
    
    public async Task<PaginatedResponse<ToyDto>> GetToysAsync(
        int categoryId, int page, int pageSize)
    {
        // Validation
        if (page < 1 || pageSize < 1)
            throw new ArgumentException("Invalid pagination parameters");
        
        if (pageSize > 100)
            pageSize = 100; // Safety cap
        
        // Call repository with pagination info
        var (toys, totalCount) = await _toyRepository
            .GetToysByategoryAsync(categoryId, page, pageSize);
        
        // Map to DTO
        var toyDtos = toys.Select(t => new ToyDto
        {
            Id = t.Id,
            Name = t.Name,
            Price = t.Price,
            Stock = t.Stock
        }).ToList();
        
        // Return with pagination metadata
        return new PaginatedResponse<ToyDto>
        {
            Data = toyDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }
}
```

**Service responsibility:**
- Validate inputs
- Enforce business rules
- Call repository
- Transform data
- Return to controller

---

#### **STEP 6: Repository Layer (Data Access)**
```csharp
public class ToyRepository : IToyRepository
{
    private readonly ToyShoppingDbContext _context;
    
    public ToyRepository(ToyShoppingDbContext context)
    {
        _context = context;
    }
    
    public async Task<(List<Toy>, int)> GetToysByategoryAsync(
        int categoryId, int page, int pageSize)
    {
        var query = _context.Toys
            .AsNoTracking() // Read-only, no change tracking
            .Where(t => t.CategoryId == categoryId && t.IsActive);
        
        // Get total count (before pagination)
        int totalCount = await query.CountAsync();
        
        // Apply pagination at DB level
        var toys = await query
            .OrderByDescending(t => t.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (toys, totalCount);
    }
}
```

**Repository responsibility:**
- Only data access
- Pagination via `Skip/Take` at DB level (NOT in-memory)
- LINQ → SQL translation
- Return raw domain entities

---

#### **STEP 7: Database Query**
```sql
-- EF Core translates to something like:
SELECT COUNT(*) FROM Toys WHERE CategoryId = 5 AND IsActive = 1

SELECT * FROM Toys 
WHERE CategoryId = 5 AND IsActive = 1
ORDER BY CreatedDate DESC
OFFSET 10 ROWS  -- (page 2, pageSize 10) = skip 10
FETCH NEXT 10 ROWS ONLY
```

**Why this matters:**
- Only 10 rows fetched from DB (not all 1000)
- Database does the heavy lifting
- Efficient for large datasets

---

#### **STEP 8: Response Built & Returned**
```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "data": [
    {
      "id": 15,
      "name": "Remote Control Car",
      "price": 29.99,
      "stock": 50
    },
    // ... 9 more items
  ],
  "totalCount": 237,
  "page": 2,
  "pageSize": 10,
  "totalPages": 24
}
```

---

#### **STEP 9: Angular Receives & Renders**
```typescript
this.toys = response.data;           // 10 toys
this.totalCount = response.totalCount; // 237 total toys
this.currentPage = response.page;      // 2
this.totalPages = response.totalPages; // 24

// UI shows: "Page 2 of 24"
// Shows pagination buttons
```

---

### 🎯 Key Points from This Flow

✅ **Stateless** — no session stored server-side  
✅ **Pagination at DB level** — only 10 rows fetched  
✅ **Separation of concerns** — each layer has one job  
✅ **Security** — JWT validated in middleware  
✅ **DTO usage** — database entities NOT exposed to client  

---

## 🔟B Roles & Permissions Matrix

### What Actions Are Allowed?

| **Action** | **Customer** | **Admin** | **Enforcement Point** |
|---|---|---|---|
| View public toys | ✅ | ✅ | No `[Authorize]` needed |
| Filter/sort toys | ✅ | ✅ | No `[Authorize]` needed |
| View own orders | ✅ | ✅ | `[Authorize]` in controller; service checks `userId == ClaimsIdentity.UserId` |
| Create order | ✅ | ✅ | `[Authorize]` in controller |
| View ALL orders (system-wide) | ❌ | ✅ | `[Authorize(Roles = "Admin")]` in controller |
| Create toy | ❌ | ✅ | `[Authorize(Roles = "Admin")]` in controller |
| Update toy | ❌ | ✅ | `[Authorize(Roles = "Admin")]` in controller |
| Delete toy | ❌ | ✅ | `[Authorize(Roles = "Admin")]` in controller |
| Manage categories | ❌ | ✅ | `[Authorize(Roles = "Admin")]` in controller |
| Delete any order | ❌ | ✅ | `[Authorize(Roles = "Admin")]` in controller |

---

### How Roles Are Stored

```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; } // "Customer" or "Admin"
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

---

### When JWT is Issued (Login)

```csharp
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, user.Role) // "Admin" or "Customer"
};

var token = new JwtSecurityToken(
    issuer: "ToyShoppingApp",
    audience: "ToyShoppingAppUsers",
    claims: claims,
    expires: DateTime.UtcNow.AddHours(1),
    signingCredentials: new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        SecurityAlgorithms.HmacSha256)
);
```

---

## 🔟C Authorization Strategy & Enforcement Points

### Where Is Authorization Checked?

#### **1. Controller Level** ✅ PRIMARY

```csharp
[ApiController]
[Route("api/[controller]")]
public class ToysController : ControllerBase
{
    // Anyone can view toys
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ToyDto>>> GetToys(...)
    {
        // No [Authorize] attribute
    }
    
    // Only admins can create toys
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ToyDto>> CreateToy([FromBody] CreateToyRequest request)
    {
        // If user is not Admin, 403 Forbidden returned BEFORE method executes
    }
    
    // Only authenticated users (both Admin & Customer) can place orders
    [Authorize]
    [HttpPost("orders")]
    public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] CreateOrderRequest request)
    {
        // Checked in service: does order belong to authenticated user?
    }
}
```

**Why controller level?**
- Fast rejection (400ms before reaching service layer)
- No wasted computation
- Clear and explicit in code
- Intercepted by middleware

---

#### **2. Service Level** ✅ SECONDARY (Business Logic)

```csharp
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public OrderService(
        IOrderRepository orderRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _orderRepository = orderRepository;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<OrderDto> PlaceOrderAsync(CreateOrderRequest request)
    {
        // Get authenticated user from context
        var userId = int.Parse(
            _httpContextAccessor.HttpContext.User
                .FindFirst(ClaimTypes.NameIdentifier).Value);
        
        // Validate user can create order for THEMSELVES
        // (not for another user)
        if (request.UserId != userId && !IsAdmin())
            throw new UnauthorizedAccessException(
                "You cannot create orders for other users");
        
        // Check business rules: stock, pricing, etc.
        var order = new Order
        {
            UserId = userId,
            Items = request.Items,
            CreatedDate = DateTime.UtcNow
        };
        
        return await _orderRepository.CreateOrderAsync(order);
    }
    
    private bool IsAdmin()
    {
        return _httpContextAccessor.HttpContext.User
            .IsInRole("Admin");
    }
}
```

**Why service level?**
- Validates **business ownership** (can customer A view customer B's orders? NO)
- Enforces business rules (can we sell out-of-stock items? NO)
- **Safety net** if controller layer is bypassed

---

#### **3. Repository Layer** ❌ WRONG PLACE

```csharp
// ❌ DO NOT DO THIS
public class OrderRepository : IOrderRepository
{
    public async Task<OrderDto> GetOrderByIdAsync(int orderId)
    {
        // BAD: Authorization should not be here
        var userId = _httpContextAccessor.HttpContext.User
            .FindFirst(ClaimTypes.NameIdentifier);
        
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId);
        
        if (order.UserId != userId)
            throw new UnauthorizedAccessException(); // ❌ WRONG
    }
}
```

**Why NOT repository?**
- Repository is for **data access only**
- Authorization is not a data access concern
- Violates Single Responsibility Principle
- Makes testing harder
- Mixes data access with security logic

---

### Authorization Flow Diagram

```
Request arrives with JWT
      ↓
[Authentication Middleware]
   Validates JWT signature
   Extracts claims (UserId, Role)
      ↓
[Controller Attribute: @Authorize(Roles="Admin")]
   Is user in "Admin" role?
   ❌ NO → return 403 Forbidden (STOP HERE)
   ✅ YES → proceed
      ↓
[Controller Action Executes]
   Calls service
      ↓
[Service Layer]
   Checks business rules & ownership
   ❌ Business rule violated → throw exception
   ✅ All good → execute logic
      ↓
[Repository Layer]
   Execute query
   Return data
```

---

### What Happens If Authorization Fails at Each Point?

| **Layer** | **Failure** | **Response** | **When** |
|---|---|---|---|
| Middleware (JWT validation) | Invalid/expired token | 401 Unauthorized | Token signature wrong, expired, or missing |
| Controller `[Authorize]` | User not authenticated | 401 Unauthorized | `[Authorize]` on unauthenticated request |
| Controller `[Authorize(Roles="Admin")]` | Wrong role | 403 Forbidden | Customer tries admin endpoint |
| Service (ownership check) | User doesn't own resource | 403 Forbidden | Customer tries to view another's order |
| Service (business rule) | Rule violated | 400 Bad Request + error message | Trying to order out-of-stock item |

---

### Why This Multi-Layer Approach?

✅ **Performance**: Fast rejection at controller level  
✅ **Security**: Defense in depth (multiple checkpoints)  
✅ **Clarity**: Each layer has explicit responsibility  
✅ **Maintainability**: Easy to audit who can do what  
✅ **Testability**: Can test each layer independently  

---

### Example: Accessing Another User's Orders (Exploit Attempt)

```
Attacker (User ID: 5) tries:
GET /api/orders/123
Authorization: Bearer [VALID_TOKEN_FOR_USER_5]

Flow:
1. ✅ Middleware validates JWT → OK (token is valid)
2. ✅ Controller [Authorize] → OK (user is authenticated)
3. ✅ Controller action executes
4. ✅ Service is called with orderId=123
5. ❌ Service checks: does order 123 belong to user 5?
   → Order belongs to user 7 → throws UnauthorizedAccessException
6. Global exception handler returns 403 Forbidden
```

**Even with a valid JWT, user cannot access another's data.**

---

## ✅ FINAL STEP‑1 SUMMARY (MEMORIZE)

> "The Toy Shopping Web App follows a clean, layered architecture with Angular as the UI client and ASP.NET Core Web API as the backend. The system is stateless, secured using JWT authentication and role‑based authorization enforced at the controller level (fast rejection) and service level (business logic validation), follows REST principles, handles pagination at the database level, and integrates with Azure Key Vault for secure secret management. Middleware manages cross‑cutting concerns, ensuring scalability, security, and maintainability."

---

### 🎓 Interview-Ready Summary

**If asked: "How do you handle authorization in a web app?"**

> "We use a multi-layer approach. First, the controller uses `[Authorize]` and `[Authorize(Roles="...")]` attributes for fast role-based rejection. Second, the service layer validates business logic — for example, a customer can only view their own orders, even if they somehow bypass the controller. Third, all requests go through middleware that validates the JWT token and extracts claims. This provides defense in depth: fast performance, clear security boundaries, and compliance with enterprise standards."

***

