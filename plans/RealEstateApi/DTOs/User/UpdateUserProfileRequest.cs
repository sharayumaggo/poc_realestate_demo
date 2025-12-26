using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.DTOs.User
{
    public class UpdateUserProfileRequest
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? Phone { get; set; }
    }
}