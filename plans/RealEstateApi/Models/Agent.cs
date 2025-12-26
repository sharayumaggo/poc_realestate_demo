using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateApi.Models
{
    public class Agent
    {
        [Key]
        [ForeignKey("User")]
        public Guid Id { get; set; }

        [Required]
        public string AgencyName { get; set; } = string.Empty;

        [Required]
        public string LicenseNumber { get; set; } = string.Empty;

        public string Specialties { get; set; } = string.Empty;

        public int YearsOfExperience { get; set; }

        public string Bio { get; set; } = string.Empty;

        public string? ProfileImage { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public User User { get; set; } = null!;
    }
}