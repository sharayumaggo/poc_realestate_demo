using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace RealEstateApi.Models
{
    public class Property
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,8)")]
        public decimal Latitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(11,8)")]
        public decimal Longitude { get; set; }

        [Required]
        public PropertyType PropertyType { get; set; }

        [Required]
        public int Bedrooms { get; set; }

        [Required]
        public int Bathrooms { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? LandSize { get; set; }

        [Required]
        public PropertyStatus Status { get; set; } = PropertyStatus.PendingApproval;

        [Required]
        public Guid SellerId { get; set; }

        public Guid? AgentId { get; set; }

        [Required]
        public string ImagesJson { get; set; } = "[]"; // Store as JSON string

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("SellerId")]
        public User Seller { get; set; } = null!;

        [ForeignKey("AgentId")]
        public User? Agent { get; set; }

        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

        // Helper property for Images
        [NotMapped]
        public List<string> Images
        {
            get => JsonSerializer.Deserialize<List<string>>(ImagesJson) ?? new List<string>();
            set => ImagesJson = JsonSerializer.Serialize(value);
        }
    }
}