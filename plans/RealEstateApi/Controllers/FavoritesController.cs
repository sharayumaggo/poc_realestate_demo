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
    public class FavoritesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FavoritesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/favorites
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetFavorites()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Property)
                .ThenInclude(p => p.Seller)
                .ToListAsync();

            var propertyDtos = favorites.Select(f => new PropertyDto
            {
                Id = f.Property.Id,
                Title = f.Property.Title,
                Description = f.Property.Description,
                Price = f.Property.Price,
                Location = f.Property.Location,
                Latitude = f.Property.Latitude,
                Longitude = f.Property.Longitude,
                PropertyType = f.Property.PropertyType,
                Bedrooms = f.Property.Bedrooms,
                Bathrooms = f.Property.Bathrooms,
                LandSize = f.Property.LandSize,
                Status = f.Property.Status,
                SellerId = f.Property.SellerId,
                AgentId = f.Property.AgentId,
                Images = f.Property.Images,
                CreatedAt = f.Property.CreatedAt,
                UpdatedAt = f.Property.UpdatedAt
            }).ToList();

            return Ok(propertyDtos);
        }

        // POST: api/favorites/{propertyId}
        [HttpPost("{propertyId}")]
        [Authorize]
        public async Task<IActionResult> AddFavorite(Guid propertyId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);

            var property = await _context.Properties.FindAsync(propertyId);
            if (property == null)
                return NotFound("Property not found.");

            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PropertyId == propertyId);

            if (existingFavorite != null)
                return BadRequest("Property is already in favorites.");

            var favorite = new Favorite
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PropertyId = propertyId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok("Property added to favorites.");
        }

        // DELETE: api/favorites/{propertyId}
        [HttpDelete("{propertyId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFavorite(Guid propertyId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PropertyId == propertyId);

            if (favorite == null)
                return NotFound("Favorite not found.");

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}