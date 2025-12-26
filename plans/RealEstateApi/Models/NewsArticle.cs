using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateApi.Models
{
    public class NewsArticle
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public Guid? AuthorId { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public DateTime PublishDate { get; set; }

        [Required]
        public NewsStatus Status { get; set; } = NewsStatus.Draft;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("AuthorId")]
        public User? Author { get; set; }
    }
}