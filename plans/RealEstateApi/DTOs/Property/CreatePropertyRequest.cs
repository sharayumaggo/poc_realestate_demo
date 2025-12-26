using RealEstateApi.Models;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.DTOs.Property
{
    public class CreatePropertyRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public decimal Longitude { get; set; }

        [Required]
        public PropertyType PropertyType { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Bedrooms { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Bathrooms { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? LandSize { get; set; }

        public List<string> Images { get; set; } = new List<string>();
    }
}