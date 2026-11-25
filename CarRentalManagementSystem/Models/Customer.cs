using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.Models
{
    // This table stores extra customer info. The login itself is handled by ASP.NET Identity.
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        // I use this to link the customer record to the Identity user account.
        [Required]
        public string IdentityUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
