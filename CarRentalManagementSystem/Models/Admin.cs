using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.Models
{
    // This table stores admin profile info. Identity + roles handle the actual login/permissions.
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Link to Identity user.
        [Required]
        public string IdentityUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
