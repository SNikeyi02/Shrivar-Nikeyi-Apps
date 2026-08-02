using System.ComponentModel.DataAnnotations;

namespace Prog_POE.Models
{
    public enum UserRole { Lecturer, Coordinator, Manager, HR, ClaimProcessor }

    public enum ClaimStatus
    {
        [Display(Name = "Pending Manager Approval")]
        PendingManagerApproval = 1,

        [Display(Name = "Manager Approved")]
        ManagerApproved = 2,

        [Display(Name = "Pending Coordinator Approval")]
        PendingCoordinatorApproval = 3,

        [Display(Name = "Coordinator Approved (Ready for Payment)")]
        CoordinatorApproved = 4,

        [Display(Name = "Rejected")]
        Rejected = 98,

        [Display(Name = "Paid")]
        Paid = 99
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Department { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Lecturer specific fields
        public double? HourlyRate { get; set; }
        public string EmployeeId { get; set; }
        public DateTime? HireDate { get; set; }

        // Navigation properties
        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
        public virtual ICollection<ClaimAudit> ProcessedClaims { get; set; } = new List<ClaimAudit>();
    }

    public class SupportingDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClaimId { get; set; }
        public virtual Claim Claim { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string StoredFileName { get; set; } // Encrypted filename

        public DateTime UploadDate { get; set; } = DateTime.Now;
        public long FileSize { get; set; }
    }

    public class Claim
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LecturerId { get; set; }
        public virtual User Lecturer { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        [Required]
        [Range(0.5, 100, ErrorMessage = "Hours must be between 0.5 and 100.")]
        public double HoursWorked { get; set; }

        [Required]
        [Range(50, 500, ErrorMessage = "Hourly rate must be between 50 and 500.")]
        public double HourlyRate { get; set; } = 75.0;

        [Range(0, 50, ErrorMessage = "Overtime hours must be between 0 and 50.")]
        public double? OvertimeHours { get; set; }

        [Range(100, 1000, ErrorMessage = "Overtime rate must be between 100 and 1000.")]
        public double? OvertimeRate { get; set; } = 150.0;

        public DateTime WorkDate { get; set; }
        public DateTime ClaimDate { get; set; } = DateTime.Now;
        public DateTime? LastUpdated { get; set; } = DateTime.Now;

        public ClaimStatus Status { get; set; } = ClaimStatus.PendingManagerApproval;

        public double TotalClaimAmount =>
            (HoursWorked * HourlyRate) +
            ((OvertimeHours ?? 0) * (OvertimeRate ?? 0));

        // Navigation properties
        public virtual ICollection<ClaimAudit> AuditTrail { get; set; } = new List<ClaimAudit>();

        public virtual ICollection<SupportingDocument> SupportingDocuments { get; set; } = new List<SupportingDocument>();
    }

    public class ClaimAudit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClaimId { get; set; }
        public virtual Claim Claim { get; set; }

        [Required]
        public int ProcessorId { get; set; }
        public virtual User Processor { get; set; }

        public ClaimStatus OldStatus { get; set; }
        public ClaimStatus NewStatus { get; set; }

        [StringLength(500)]
        public string Comments { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string ActionType { get; set; } // "Approved", "Rejected", "Created", "Updated"
    }

    public class ReportFilter
    {
        public string LecturerName { get; set; }
        public ClaimStatus? Status { get; set; }
        public double? MinAmount { get; set; }
        public double? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportType { get; set; } = "PDF";
    }

    public class DashboardStats
    {
        public int TotalLecturers { get; set; }
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ManagerApprovedClaims { get; set; }
        public int CoordinatorApprovedClaims { get; set; }
        public double TotalClaimValue { get; set; }
    }

}