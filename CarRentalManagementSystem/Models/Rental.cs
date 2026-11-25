using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalManagementSystem.Models
{
    // This tracks rental requests and the workflow (pending -> approved/rejected -> completed).
    public class Rental
    {
        [Key]
        public int RentalId { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [Required]
        [ForeignKey("Car")]
        public int CarId { get; set; }

        // AdminId is optional because a rental starts as "Pending".
        [ForeignKey("ApprovedByAdmin")]
        public int? ApprovedByAdminId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Requested At")]
        public DateTime RequestedAt { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";
        // Pending / Approved / Rejected / Completed

        [Display(Name = "Decision Date")]
        public DateTime? DecisionAt { get; set; }

        [Range(0, 1000000)]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Total Cost")]
        public decimal TotalCost { get; set; }

        public string? Notes { get; set; }

        // Navigation properties (lets me use Include() and makes relationships clear)
        public Customer? Customer { get; set; }
        public Car? Car { get; set; }
        public Admin? ApprovedByAdmin { get; set; }

        // I stored the logged-in user's Identity ID so that I can easily filter "My Rentals".
        [Required]
        public string IdentityUserId { get; set; } = string.Empty;
    }
}
