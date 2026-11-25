using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace CarRentalManagementSystem.Models
{
    // This model represents a car that can be added by admins and rented by customers.
    public class Car
    {
        [Key]
        public int CarId { get; set; }

        [Required]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Range(1980, 2100)]
        public int Year { get; set; }

        [Required]
        [StringLength(30)]
        public string Category { get; set; } = string.Empty; // e.g., Sedan, SUV

        [Required]
        [StringLength(20)]
        public string Transmission { get; set; } = "Automatic";

        [Range(1, 12)]
        public int Seats { get; set; } = 4;

        [Range(0, 100000)]
        public decimal DailyRate { get; set; }

        // This makes it easy to hide cars that are currently rented out.
        public bool IsAvailable { get; set; } = true;

        // I save the path to the uploaded image instead of the image itself.
        public string? ImagePath { get; set; }

        // I use this only when uploading an image from the form.
        // It is not saved in the database.
        [NotMapped]
        public IFormFile? ImageFile { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
