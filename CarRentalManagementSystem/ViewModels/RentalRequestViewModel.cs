using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.ViewModels
{
    // I use this view model so the rental request form stays simple and clean.
    // Validation here keeps bad input from even reaching the controller.
    public class RentalRequestViewModel
    {
        [Required]
        public int CarId { get; set; }

        // This is just for display on the form (make/model/year + price).
        public string CarDisplay { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

        // Custom validation so EndDate cannot be before StartDate.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate.Date < StartDate.Date)
            {
                yield return new ValidationResult(
                    "End date cannot be before start date.",
                    new[] { nameof(EndDate) }
                );
            }
        }
    }
}
