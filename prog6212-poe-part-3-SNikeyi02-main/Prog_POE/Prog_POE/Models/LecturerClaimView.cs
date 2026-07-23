using System.ComponentModel.DataAnnotations;

namespace Prog_POE.Models
{
    public class LecturerClaimView
    {
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "Lecturer name is required.")]
        public string? LecturerName { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
        public string? Description { get; set; }

        [Display(Name = "Date Submitted")]
        public DateTime ClaimDate { get; set; } = DateTime.Now;

        [Display(Name = "Last Updated")]
        public DateTime? LastUpdated { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Hours worked is required.")]
        [Range(0.5, 100, ErrorMessage = "Hours must be between 0.5 and 100.")]
        public double HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required.")]
        [Range(50, 500, ErrorMessage = "Hourly rate must be between 50 and 500.")]
        public double HourlyRate { get; set; } = 75.0;

        [Range(0, 50, ErrorMessage = "Overtime hours must be between 0 and 50.")]
        public double? OvertimeHours { get; set; }

        [Range(100, 1000, ErrorMessage = "Overtime rate must be between 100 and 1000.")]
        public double? OvertimeRate { get; set; } = 150.0;

        public List<string> SupportingDocumentNames { get; set; } = new();

        [Display(Name = "Supporting Documents")]
        public List<IFormFile> SupportingDocuments { get; set; } = new();

        public bool IsManagerApproved { get; set; }
        public bool IsCoordinatorApproved { get; set; }

        public ClaimStatus Status { get; set; } = ClaimStatus.PendingManagerApproval;

        [Display(Name = "Total Claim Amount (R)")]
        public double TotalClaimAmount =>
            (HoursWorked * HourlyRate) +
            ((OvertimeHours ?? 0) * (OvertimeRate ?? 0));
    }
}
