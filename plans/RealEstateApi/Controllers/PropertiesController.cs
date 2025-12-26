using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Property;
using RealEstateApi.Models;
using System.Security.Claims;

namespace RealEstateApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropertiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PropertiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/properties
        [HttpGet]
        public async Task<IActionResult> GetProperties(
            [FromQuery] string? search = null,
            [FromQuery] PropertyType? propertyType = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] int? minBedrooms = null,
            [FromQuery] int? maxBedrooms = null,
            [FromQuery] PropertyStatus? status = null,
            [FromQuery] string? location = null,
            [FromQuery] string? sortBy = "createdAt",
            [FromQuery] string? sortOrder = "desc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Properties
                .Include(p => p.Seller)
                .Include(p => p.Agent)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Title.Contains(search) ||
                                        p.Description.Contains(search) ||
                                        p.Location.Contains(search));
            }

            if (propertyType.HasValue)
            {
                query = query.Where(p => p.PropertyType == propertyType.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (minBedrooms.HasValue)
            {
                query = query.Where(p => p.Bedrooms >= minBedrooms.Value);
            }

            if (maxBedrooms.HasValue)
            {
                query = query.Where(p => p.Bedrooms <= maxBedrooms.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(p => p.Location.Contains(location));
            }

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "price" => sortOrder?.ToLower() == "asc"
                    ? query.OrderBy(p => p.Price)
                    : query.OrderByDescending(p => p.Price),
                "bedrooms" => sortOrder?.ToLower() == "asc"
                    ? query.OrderBy(p => p.Bedrooms)
                    : query.OrderByDescending(p => p.Bedrooms),
                "createdat" or _ => sortOrder?.ToLower() == "asc"
                    ? query.OrderBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.CreatedAt)
            };

            // Apply pagination
            var totalCount = await query.CountAsync();
            var properties = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var propertyDtos = properties.Select(p => new PropertyDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                Location = p.Location,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                PropertyType = p.PropertyType,
                Bedrooms = p.Bedrooms,
                Bathrooms = p.Bathrooms,
                LandSize = p.LandSize,
                Status = p.Status,
                SellerId = p.SellerId,
                AgentId = p.AgentId,
                Images = p.Images,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();

            var result = new
            {
                Data = propertyDtos,
                Pagination = new
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            };

            return Ok(result);
        }

        // GET: api/properties/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProperty(Guid id)
        {
            var property = await _context.Properties
                .Include(p => p.Seller)
                .Include(p => p.Agent)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
                return NotFound();

            var propertyDto = new PropertyDto
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                Location = property.Location,
                Latitude = property.Latitude,
                Longitude = property.Longitude,
                PropertyType = property.PropertyType,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                LandSize = property.LandSize,
                Status = property.Status,
                SellerId = property.SellerId,
                AgentId = property.AgentId,
                Images = property.Images,
                CreatedAt = property.CreatedAt,
                UpdatedAt = property.UpdatedAt
            };

            return Ok(propertyDto);
        }

        // POST: api/properties
        [HttpPost]
        [Authorize(Roles = "Seller,Agent,Admin")]
        public async Task<IActionResult> CreateProperty([FromBody] CreatePropertyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();

            var property = new Property
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Location = request.Location,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                PropertyType = request.PropertyType,
                Bedrooms = request.Bedrooms,
                Bathrooms = request.Bathrooms,
                LandSize = request.LandSize,
                Status = PropertyStatus.PendingApproval,
                SellerId = userId,
                ImagesJson = System.Text.Json.JsonSerializer.Serialize(request.Images),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            var propertyDto = new PropertyDto
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                Location = property.Location,
                Latitude = property.Latitude,
                Longitude = property.Longitude,
                PropertyType = property.PropertyType,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                LandSize = property.LandSize,
                Status = property.Status,
                SellerId = property.SellerId,
                AgentId = property.AgentId,
                Images = property.Images,
                CreatedAt = property.CreatedAt,
                UpdatedAt = property.UpdatedAt
            };

            return CreatedAtAction(nameof(GetProperty), new { id = property.Id }, propertyDto);
        }

        // PUT: api/properties/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Seller,Agent,Admin")]
        public async Task<IActionResult> UpdateProperty(Guid id, [FromBody] UpdatePropertyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userRoleEnum = Enum.Parse<UserRole>(userRole);

            var property = await _context.Properties.FindAsync(id);
            if (property == null)
                return NotFound();

            // Check if user is the seller, agent, or admin
            if (property.SellerId != userId && property.AgentId != userId && userRoleEnum != UserRole.Admin)
                return Forbid();

            property.Title = request.Title;
            property.Description = request.Description;
            property.Price = request.Price;
            property.Location = request.Location;
            property.Latitude = request.Latitude;
            property.Longitude = request.Longitude;
            property.PropertyType = request.PropertyType;
            property.Bedrooms = request.Bedrooms;
            property.Bathrooms = request.Bathrooms;
            property.LandSize = request.LandSize;
            property.AgentId = request.AgentId;
            property.ImagesJson = System.Text.Json.JsonSerializer.Serialize(request.Images);
            property.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var propertyDto = new PropertyDto
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                Location = property.Location,
                Latitude = property.Latitude,
                Longitude = property.Longitude,
                PropertyType = property.PropertyType,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                LandSize = property.LandSize,
                Status = property.Status,
                SellerId = property.SellerId,
                AgentId = property.AgentId,
                Images = property.Images,
                CreatedAt = property.CreatedAt,
                UpdatedAt = property.UpdatedAt
            };

            return Ok(propertyDto);
        }

        // DELETE: api/properties/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Seller,Agent,Admin")]
        public async Task<IActionResult> DeleteProperty(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userRoleEnum = Enum.Parse<UserRole>(userRole);

            var property = await _context.Properties.FindAsync(id);
            if (property == null)
                return NotFound();

            // Check if user is the seller, agent, or admin
            if (property.SellerId != userId && property.AgentId != userId && userRoleEnum != UserRole.Admin)
                return Forbid();

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}