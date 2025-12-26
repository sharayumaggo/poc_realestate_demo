# 06 - Standards & Guidelines

## Development Standards

### Code Quality

#### C# Coding Standards
```csharp
// ✅ Good: Clear naming, proper structure
public async Task<IActionResult> GetPropertyAsync(Guid id)
{
    var property = await _context.Properties
        .Include(p => p.Seller)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (property == null)
        return NotFound();

    var dto = MapToPropertyDto(property);
    return Ok(dto);
}

// ❌ Bad: Inconsistent naming, magic numbers
public async Task<IActionResult> getprop(Guid id)
{
    var p = await _context.Properties.FindAsync(id);
    if (p == null) return NotFound();
    return Ok(p);
}
```

#### TypeScript/JavaScript Standards
```typescript
// ✅ Good: Type safety, clear interfaces
interface Property {
  id: string;
  title: string;
  price: number;
  status: PropertyStatus;
}

const updateProperty = async (id: string, updates: Partial<Property>): Promise<Property> => {
  const response = await api.patch(`/properties/${id}`, updates);
  return response.data;
};

// ❌ Bad: Any types, unclear contracts
const updateProperty = async (id, updates) => {
  const response = await api.patch(`/properties/${id}`, updates);
  return response.data;
};
```

### Commit Message Standards
```
type(scope): description

[optional body]

[optional footer]

Types:
- feat: new feature
- fix: bug fix
- docs: documentation
- style: formatting
- refactor: code restructuring
- test: testing
- chore: maintenance

Examples:
feat(auth): add JWT token refresh
fix(api): handle null reference in property search
docs(readme): update deployment instructions
```

### Branch Naming Convention
```
feature/{ticket-number}-{description}
bugfix/{ticket-number}-{description}
hotfix/{description}
release/{version}

Examples:
feature/REAL-123-add-favorites
bugfix/REAL-456-fix-login-validation
release/v1.2.0
```

## API Design Guidelines

### REST API Standards
- **Resource Naming**: Use nouns, plural form
  - ✅ `/api/properties`
  - ❌ `/api/getProperties`

- **HTTP Methods**: Standard REST methods
  - `GET` - Retrieve resources
  - `POST` - Create resources
  - `PUT` - Update entire resource
  - `PATCH` - Partial updates
  - `DELETE` - Remove resources

- **Status Codes**:
  - `200 OK` - Successful request
  - `201 Created` - Resource created
  - `400 Bad Request` - Invalid request
  - `401 Unauthorized` - Authentication required
  - `403 Forbidden` - Insufficient permissions
  - `404 Not Found` - Resource not found
  - `500 Internal Server Error` - Server error

### API Response Format
```json
// Success response
{
  "data": { /* resource data */ },
  "message": "Operation successful",
  "timestamp": "2024-01-01T12:00:00Z"
}

// Error response
{
  "type": "https://api.realestate.com/errors/validation-error",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": {
    "email": ["Email is required"],
    "password": ["Password must be at least 8 characters"]
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### Versioning Strategy
```
Accept: application/vnd.realestate.v1+json
/api/v1/properties
```

## Database Design Standards

### Naming Conventions
```sql
-- Tables: snake_case, plural
users
properties
user_favorites

-- Columns: snake_case
user_id
created_at
updated_at

-- Indexes: idx_table_column
idx_properties_price
idx_properties_location
```

### Constraints and Keys
```sql
-- Primary keys
id UUID PRIMARY KEY DEFAULT gen_random_uuid()

-- Foreign keys
seller_id UUID REFERENCES users(id) ON DELETE CASCADE

-- Unique constraints
UNIQUE(email)

-- Check constraints
CHECK (price > 0)
CHECK (bedrooms >= 0)
```

### Indexing Strategy
```sql
-- Single column indexes
CREATE INDEX idx_properties_status ON properties (status);
CREATE INDEX idx_properties_price ON properties (price);

-- Composite indexes
CREATE INDEX idx_properties_location_price ON properties (location, price);

-- Partial indexes
CREATE INDEX idx_active_properties ON properties (status) WHERE status = 'Active';

-- Functional indexes
CREATE INDEX idx_properties_price_range ON properties ((price / 100000));
```

## Security Guidelines

### Authentication & Authorization
- **JWT Tokens**: Short expiration (15-60 minutes)
- **Refresh Tokens**: Secure storage, rotation
- **Password Policy**: 8+ chars, mixed case, numbers, symbols
- **Rate Limiting**: API endpoints protection
- **CORS**: Configured for allowed origins only

### Data Protection
- **Encryption**: Data at rest and in transit
- **PII Handling**: GDPR/CCPA compliance
- **Input Validation**: Server-side validation always
- **SQL Injection**: Parameterized queries only
- **XSS Protection**: Sanitize user inputs

### Secrets Management
```csharp
// ✅ Good: Environment variables or secret manager
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

// ❌ Bad: Hardcoded secrets
var secretKey = "my-secret-key-123";
```

## Testing Standards

### Unit Testing
```csharp
[Fact]
public async Task CreateProperty_ValidData_ReturnsCreated()
{
    // Arrange
    var command = new CreatePropertyCommand { /* valid data */ };
    var handler = new CreatePropertyHandler(_context, _mapper);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(command.Title, result.Title);
}
```

### Integration Testing
```csharp
[Fact]
public async Task GetProperties_ReturnsFromDatabase()
{
    // Arrange
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

    // Act
    var response = await client.GetAsync("/api/properties");

    // Assert
    response.EnsureSuccessStatusCode();
    var properties = await response.Content.ReadFromJsonAsync<List<PropertyDto>>();
    Assert.NotEmpty(properties);
}
```

### Frontend Testing
```typescript
import { render, screen, fireEvent, waitFor } from '@testing-library/react';

test('displays property details', async () => {
  render(<PropertyCard property={mockProperty} />);

  expect(screen.getByText(mockProperty.title)).toBeInTheDocument();
  expect(screen.getByText(`$${mockProperty.price.toLocaleString()}`)).toBeInTheDocument();
});

test('handles favorite toggle', async () => {
  render(<PropertyCard property={mockProperty} />);

  const favoriteButton = screen.getByRole('button', { name: /favorite/i });
  fireEvent.click(favoriteButton);

  await waitFor(() => {
    expect(mockApi.addFavorite).toHaveBeenCalledWith(mockProperty.id);
  });
});
```

### Test Coverage Goals
- **Unit Tests**: 80%+ coverage
- **Integration Tests**: All critical paths
- **E2E Tests**: Happy path and error scenarios

## Performance Guidelines

### Database Optimization
```csharp
// ✅ Good: Efficient queries
var properties = await _context.Properties
    .AsNoTracking()
    .Where(p => p.Status == PropertyStatus.Active)
    .OrderByDescending(p => p.CreatedAt)
    .Take(20)
    .ToListAsync();

// ❌ Bad: N+1 queries
foreach (var property in properties)
{
    property.Seller = await _context.Users.FindAsync(property.SellerId);
}
```

### Caching Strategy
```csharp
// Response caching
[ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
public async Task<IActionResult> GetPropertyTypes()

// Application caching
private readonly IMemoryCache _cache;

public async Task<List<PropertyType>> GetTypesAsync()
{
    const string cacheKey = "property-types";
    if (!_cache.TryGetValue(cacheKey, out List<PropertyType> types))
    {
        types = await _repository.GetAllAsync();
        _cache.Set(cacheKey, types, TimeSpan.FromHours(1));
    }
    return types;
}
```

### Frontend Performance
```typescript
// Code splitting
const PropertyDetails = lazy(() => import('./pages/PropertyDetails'));

// Image optimization
<picture>
  <source media="(min-width: 768px)" srcSet={property.imageLarge} />
  <img src={property.imageSmall} alt={property.title} loading="lazy" />
</picture>

// Memoization
const PropertyCard = memo(({ property }) => {
  // Component logic
});
```

## Error Handling

### Application Layer
```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
            await WriteErrorResponse(context, 400, "Validation failed", ex.Errors);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            await WriteErrorResponse(context, 403, "Access denied");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred");
            await WriteErrorResponse(context, 500, "Internal server error");
        }
    }
}
```

### Client Error Handling
```typescript
const apiClient = axios.create({
  baseURL: process.env.REACT_APP_API_URL,
});

apiClient.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      // Redirect to login
      window.location.href = '/login';
    } else if (error.response?.status >= 500) {
      // Show generic error
      showToast('Something went wrong. Please try again.');
    }
    return Promise.reject(error);
  }
);
```

## Documentation Standards

### API Documentation
```csharp
/// <summary>
/// Retrieves a property by its unique identifier
/// </summary>
/// <param name="id">The unique identifier of the property</param>
/// <returns>A property object if found, otherwise NotFound</returns>
/// <response code="200">Property retrieved successfully</response>
/// <response code="404">Property not found</response>
[HttpGet("{id}")]
[ProducesResponseType(typeof(PropertyDto), 200)]
[ProducesResponseType(404)]
public async Task<IActionResult> GetProperty(Guid id)
```

### Code Comments
```csharp
// ✅ Good: Explains why, not what
// Using eager loading to prevent N+1 queries when displaying property list
var properties = await _context.Properties
    .Include(p => p.Seller)
    .ToListAsync();

// ❌ Bad: Redundant comments
// Get all properties from database
var properties = await _context.Properties.ToListAsync();
```

### README Files
Each component should have a README with:
- Purpose and scope
- Setup instructions
- Usage examples
- API reference
- Contributing guidelines

## Code Review Guidelines

### Checklist
- [ ] Code compiles without warnings
- [ ] Tests pass and coverage maintained
- [ ] Security best practices followed
- [ ] Performance considerations addressed
- [ ] Documentation updated
- [ ] Database migrations included if needed
- [ ] Environment variables documented

### Review Process
1. **Automated Checks**: Lint, tests, security scan
2. **Peer Review**: Code style, logic, edge cases
3. **QA Review**: Functional testing
4. **Security Review**: For sensitive changes
5. **Approval**: Minimum 1 approval for merge

## Deployment Guidelines

### Environment Configuration
```yaml
# .github/workflows/deploy.yml
environment: production
env:
  ASPNETCORE_ENVIRONMENT: Production
  ConnectionStrings__DefaultConnection: ${{ secrets.DATABASE_CONNECTION }}
  JWT__Key: ${{ secrets.JWT_SECRET }}
```

### Rollback Strategy
- **Blue-Green Deployment**: Switch traffic between versions
- **Feature Flags**: Gradual rollout with feature toggles
- **Database Rollbacks**: Versioned migrations with down scripts

### Monitoring Post-Deployment
- Response times within acceptable ranges
- Error rates below threshold
- Resource utilization normal
- User-reported issues monitored

## Maintenance Standards

### Dependency Management
- Regular security updates
- Dependency vulnerability scanning
- Lock files committed
- Major version updates tested thoroughly

### Database Maintenance
- Regular index maintenance
- Query performance monitoring
- Data archiving for old records
- Backup verification

### Infrastructure Maintenance
- Regular security patching
- Cost optimization reviews
- Performance monitoring
- Capacity planning

This standards and guidelines document establishes the foundation for consistent, maintainable, and scalable development practices across the Real Estate Marketplace project.