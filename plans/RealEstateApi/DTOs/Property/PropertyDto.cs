using RealEstateApi.Models;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.DTOs.Property
{
    public class PropertyDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public PropertyType PropertyType { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public decimal? LandSize { get; set; }
        public PropertyStatus Status { get; set; }
        public Guid SellerId { get; set; }
        public Guid? AgentId { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}