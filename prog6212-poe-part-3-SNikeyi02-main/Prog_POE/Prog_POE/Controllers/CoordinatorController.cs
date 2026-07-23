using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prog_POE.Data;
using Prog_POE.Models;
using Prog_POE.Services;

// Microsoft Learn 2023. Eager Loading of Related Data - EF Core. 
// Online 
// Available at:
// <https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager>
// [Accessed 20 November 2025].



namespace Prog_POE.Controllers
{
    public class CoordinatorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEncryptionService _encryptionService;

        public CoordinatorController(AppDbContext context, IEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        [HttpGet]
        public async Task<IActionResult> Home()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Coordinator")
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Index", "Login");
            }

            // Get pending claims from database WITH SUPPORTING DOCUMENTS
            var pendingClaims = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.SupportingDocuments)
                .Where(c => c.Status == ClaimStatus.PendingCoordinatorApproval)
                .Select(c => new CoordinatorView
                {
                    ClaimId = c.Id,
                    LecturerName = c.Lecturer.FirstName + " " + c.Lecturer.LastName,
                    HoursWorked = c.HoursWorked,
                    OvertimeHours = c.OvertimeHours,
                    Status = c.Status,
                    TotalClaimAmount = (double)c.TotalClaimAmount,
                    IsManagerApproved = true,
                    IsCoordinatorApproved = false,
                    SupportingDocumentNames = c.SupportingDocuments.Select(d => d.StoredFileName).ToList() // ADD THIS
                })
                .ToListAsync();

            Console.WriteLine($"=== COORDINATOR DEBUG: Pending claims found: {pendingClaims.Count}");
            foreach (var claim in pendingClaims)
            {
                Console.WriteLine($"Claim #{claim.ClaimId} has {claim.SupportingDocumentNames?.Count ?? 0} documents");
            }

            return View(pendingClaims);
        }

        [HttpGet]
        public IActionResult DownloadDocument(string fileName)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Coordinator" || string.IsNullOrEmpty(fileName) || !fileName.EndsWith(".encrypted"))
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Index", "Login");
            }

            var secureFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SecureUploads");
            var filePath = Path.Combine(secureFolderPath, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            // Authorization check using database
            var document = _context.SupportingDocuments
                .Include(d => d.Claim)
                .FirstOrDefault(d => d.StoredFileName == fileName);

            if (document == null)
                return Forbid("File not linked to any valid claim.");

            bool isAuthorized = document.Claim.Status == ClaimStatus.PendingCoordinatorApproval ||
                               document.Claim.Status == ClaimStatus.CoordinatorApproved;

            if (!isAuthorized)
                return Forbid("Not authorized to access this document.");

            try
            {
                var decryptedStream = new MemoryStream();
                using (var encryptedFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    _encryptionService.DecryptStream(encryptedFileStream, decryptedStream);
                }
                decryptedStream.Position = 0;

                var originalFileName = document.FileName;
                return File(decryptedStream, "application/octet-stream", $"ClaimDoc_{originalFileName}");
            }
            catch (CryptographicException)
            {
                return Unauthorized("File decryption failed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Download error: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Coordinator")
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Index", "Login");
            }

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null || claim.Status != ClaimStatus.PendingCoordinatorApproval)
                return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");

            // Update claim status
            claim.Status = ClaimStatus.CoordinatorApproved;
            claim.LastUpdated = DateTime.Now;

            // Create audit trail
            var audit = new ClaimAudit
            {
                ClaimId = claim.Id,
                ProcessorId = userId.Value,
                OldStatus = ClaimStatus.PendingCoordinatorApproval,
                NewStatus = ClaimStatus.CoordinatorApproved,
                ActionType = "Approved",
                Comments = "Claim approved by coordinator",
                ActionDate = DateTime.Now
            };

            _context.ClaimAudits.Add(audit);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Claim #{id} approved successfully.";
            return RedirectToAction("Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Coordinator")
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Index", "Login");
            }

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null || claim.Status != ClaimStatus.PendingCoordinatorApproval)
                return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");

            // Update claim status
            claim.Status = ClaimStatus.Rejected;
            claim.LastUpdated = DateTime.Now;

            // Create audit trail
            var audit = new ClaimAudit
            {
                ClaimId = claim.Id,
                ProcessorId = userId.Value,
                OldStatus = ClaimStatus.PendingCoordinatorApproval,
                NewStatus = ClaimStatus.Rejected,
                ActionType = "Rejected",
                Comments = "Claim rejected by coordinator",
                ActionDate = DateTime.Now
            };

            _context.ClaimAudits.Add(audit);
            await _context.SaveChangesAsync();

            TempData["Error"] = $"Claim #{id} has been rejected.";
            return RedirectToAction("Home");
        }
        private async Task MigrateStaticDataToDatabase()
        {
            
        }
    }
}