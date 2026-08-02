namespace Prog_POE.Models
{
    public class CoordinatorView
    {
        public int ClaimId { get; set; }
        public string? LecturerName { get; set; }
        public double HoursWorked { get; set; }
        public double? OvertimeHours { get; set; }
        public ClaimStatus Status { get; set; }
        public double TotalClaimAmount { get; set; }
        public bool IsManagerApproved { get; set; }
        public bool IsCoordinatorApproved { get; set; }

        public List<string> SupportingDocumentNames { get; set; } = new List<string>();
        public string FormattedAmount => TotalClaimAmount.ToString("C2").Replace("$", "R ");
    }
}
