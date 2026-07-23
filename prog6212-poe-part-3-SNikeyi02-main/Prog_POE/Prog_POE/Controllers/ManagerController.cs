using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Prog_POE.Data;
using Prog_POE.Models;
using Prog_POE.Services;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;

// Microsoft Learn 2023. Eager Loading of Related Data - EF Core. 
// Online 
// Available at:
// <https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager>
// [Accessed 20 November 2025].

namespace Prog_POE.Controllers
{
    public class ManagerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEncryptionService _encryptionService;

        public ManagerController(AppDbContext context, IEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        [HttpGet]
        public async Task<IActionResult> Home()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Manager")
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Index", "Login");
            }

            ViewData["Username"] = HttpContext.Session.GetString("Username");

            // DEBUG: Check what claims exist in database
            var allClaims = await _context.Claims.ToListAsync();
            Console.WriteLine($"=== DEBUG: Total claims in DB: {allClaims.Count}");
            foreach (var claim in allClaims)
            {
                Console.WriteLine($"Claim #{claim.Id}: Status={claim.Status}, LecturerId={claim.LecturerId}");
            }

            // Get pending claims from database WITH SUPPORTING DOCUMENTS
            var pendingClaims = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.SupportingDocuments) // ADD THIS LINE - CRITICAL!
                .Where(c => c.Status == ClaimStatus.PendingManagerApproval)
                .Select(c => new ManagerView
                {
                    ClaimId = c.Id,
                    LecturerName = c.Lecturer.FirstName + " " + c.Lecturer.LastName,
                    HoursWorked = c.HoursWorked,
                    OvertimeHours = c.OvertimeHours,
                    Status = c.Status,
                    TotalClaimAmount = (double)c.TotalClaimAmount,
                    IsManagerApproved = false,
                    SupportingDocumentNames = c.SupportingDocuments.Select(d => d.StoredFileName).ToList() // ADD THIS
                })
                .ToListAsync();

            Console.WriteLine($"=== DEBUG: Pending claims found: {pendingClaims.Count}");
            foreach (var claim in pendingClaims)
            {
                Console.WriteLine($"Claim #{claim.ClaimId} has {claim.SupportingDocumentNames?.Count ?? 0} documents");
                if (claim.SupportingDocumentNames != null && claim.SupportingDocumentNames.Any())
                {
                    foreach (var doc in claim.SupportingDocumentNames)
                    {
                        Console.WriteLine($"  - Document: {doc}");
                    }
                }
            }

            ViewData["TotalClaims"] = pendingClaims.Count;
            return View(pendingClaims);
        }

        [HttpGet]
        public IActionResult DownloadDocument(string fileName)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Manager" || string.IsNullOrEmpty(fileName) || !fileName.EndsWith(".encrypted"))
            {
                TempData["Error"] = "Unauthorized or invalid access to file.";
                return RedirectToAction("Index", "Login");
            }

            var secureFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SecureUploads");
            var filePath = Path.Combine(secureFolderPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found on the server.");
            }

            // Authorization check using database
            var document = _context.SupportingDocuments
                .Include(d => d.Claim)
                .FirstOrDefault(d => d.StoredFileName == fileName);

            if (document == null || document.Claim.Status != ClaimStatus.PendingManagerApproval)
            {
                return Forbid("Document is not related to a claim pending your approval.");
            }

            var originalFileName = document.FileName;

            try
            {
                var decryptedStream = new MemoryStream();

                using (var encryptedFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    _encryptionService.DecryptStream(encryptedFileStream, decryptedStream);
                }

                decryptedStream.Position = 0;
                return File(decryptedStream, "application/octet-stream", $"ClaimDoc_{originalFileName}");
            }
            catch (CryptographicException)
            {
                return Unauthorized("File decryption failed.");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred during file download: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Manager")
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Index", "Login");
            }

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
                return NotFound();

            if (claim.Status != ClaimStatus.PendingManagerApproval)
            {
                TempData["Error"] = $"Claim #{id} is not pending manager approval.";
                return RedirectToAction("Home");
            }

            var userId = HttpContext.Session.GetInt32("UserId");

            // Update claim status
            claim.Status = ClaimStatus.PendingCoordinatorApproval;
            claim.LastUpdated = DateTime.Now;

            // Create audit trail
            var audit = new ClaimAudit
            {
                ClaimId = claim.Id,
                ProcessorId = userId.Value,
                OldStatus = ClaimStatus.PendingManagerApproval,
                NewStatus = ClaimStatus.PendingCoordinatorApproval,
                ActionType = "Approved",
                Comments = "Claim approved by manager",
                ActionDate = DateTime.Now
            };

            _context.ClaimAudits.Add(audit);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Claim #{id} approved by Manager and sent to Coordinator.";
            return RedirectToAction("Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Manager")
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Index", "Login");
            }

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
                return NotFound();

            if (claim.Status != ClaimStatus.PendingManagerApproval)
            {
                TempData["Error"] = $"Claim #{id} is not pending manager approval.";
                return RedirectToAction("Home");
            }

            var userId = HttpContext.Session.GetInt32("UserId");

            // Update claim status
            claim.Status = ClaimStatus.Rejected;
            claim.LastUpdated = DateTime.Now;

            // Create audit trail
            var audit = new ClaimAudit
            {
                ClaimId = claim.Id,
                ProcessorId = userId.Value,
                OldStatus = ClaimStatus.PendingManagerApproval,
                NewStatus = ClaimStatus.Rejected,
                ActionType = "Rejected",
                Comments = "Claim rejected by manager",
                ActionDate = DateTime.Now
            };

            _context.ClaimAudits.Add(audit);
            await _context.SaveChangesAsync();

            TempData["Error"] = $"Claim #{id} rejected by Manager. Lecturer notified.";
            return RedirectToAction("Home");
        }

        private async Task MigrateStaticDataToDatabase()
        {
            
        }

    }
}


//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Http;
//using Prog_POE.Models;
//using Prog_POE.Services;
//using System.Collections.Generic;
//using System.Linq;
//using System.IO;
//using System.Security.Cryptography;

//namespace Prog_POE.Controllers
//{
//    public class ManagerController : Controller
//    {
//        private const double FixedHourlyRate = 75.0;
//        private const double FixedOvertimeRate = 150.0;
//        private readonly IEncryptionService _encryptionService;

//        public ManagerController(IEncryptionService encryptionService)
//        {
//            _encryptionService = encryptionService;
//        }

//        [HttpGet]
//        public IActionResult Home()
//        {
//            var role = HttpContext.Session.GetString("UserRole");
//            if (role != "Manager")
//            {
//                TempData["Error"] = "Unauthorized access.";
//                return RedirectToAction("Index", "Login");
//            }

//            ViewData["Username"] = HttpContext.Session.GetString("Username");

//            // Filter claims that are PendingManagerApproval from the shared store
//            var pendingClaims = CoordinatorController.ClaimsStore
//                .Where(c => c.Status == ClaimStatus.PendingManagerApproval)
//                .Select(c => new ManagerView
//                {
//                    ClaimId = c.ClaimId,
//                    LecturerName = c.LecturerName,
//                    HoursWorked = c.HoursWorked,
//                    OvertimeHours = c.OvertimeHours,
//                    Notes = c.Notes,
//                    SupportingDocumentNames = c.SupportingDocumentNames,
//                    Status = c.Status,
//                    TotalClaimAmount = (decimal)c.TotalClaimAmount,
//                    IsManagerApproved = c.IsManagerApproved
//                })
//                .ToList();

//            ViewData["TotalClaims"] = pendingClaims.Count;
//            return View(pendingClaims);
//        }

//        [HttpGet]
//        public IActionResult DownloadDocument(string fileName)
//        {
//            var role = HttpContext.Session.GetString("UserRole");

//            // 1. Basic Security/Role Check
//            if (role != "Manager" || string.IsNullOrEmpty(fileName) || !fileName.EndsWith(".encrypted"))
//            {
//                TempData["Error"] = "Unauthorized or invalid access to file.";
//                return RedirectToAction("Index", "Login");
//            }

//            // Define the secure path (must match where LecturerController saved it)
//            var secureFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SecureUploads");
//            var filePath = Path.Combine(secureFolderPath, fileName);

//            if (!System.IO.File.Exists(filePath))
//            {
//                return NotFound("File not found on the server.");
//            }

//            // 2. Authorization Check (Ensure the Manager is viewing a claim pending their approval)
//            var owningClaim = CoordinatorController.ClaimsStore
//                .FirstOrDefault(c => c.SupportingDocumentNames.Contains(fileName));

//            if (owningClaim == null || owningClaim.Status != ClaimStatus.PendingManagerApproval)
//            {
//                return Forbid("Document is not related to a claim pending your approval.");
//            }

//            // 3. Decryption and Streaming
//            var originalFileName = fileName.Replace(".encrypted", "");
//            var mimeType = "application/octet-stream";

//            try
//            {
//                var decryptedStream = new MemoryStream();

//                using (var encryptedFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
//                {
//                    // Use the injected service instance instead of static method
//                    _encryptionService.DecryptStream(encryptedFileStream, decryptedStream);
//                }

//                decryptedStream.Position = 0;
//                return File(decryptedStream, mimeType, $"ClaimDoc_{originalFileName}");
//            }
//            catch (CryptographicException)
//            {
//                return Unauthorized("File decryption failed.");
//            }
//            catch (System.Exception ex)
//            {
//                return StatusCode(500, $"An error occurred during file download: {ex.Message}");
//            }
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult ApproveClaim(int id)
//        {
//            var role = HttpContext.Session.GetString("UserRole");
//            if (role != "Manager")
//            {
//                TempData["Error"] = "Unauthorized access.";
//                return RedirectToAction("Index", "Login");
//            }

//            // Search the shared store
//            var claim = CoordinatorController.ClaimsStore.FirstOrDefault(c => c.ClaimId == id);

//            if (claim == null)
//                return NotFound();

//            // Check if claim is actually pending for Manager approval
//            if (claim.Status != ClaimStatus.PendingManagerApproval)
//            {
//                TempData["Error"] = $"Claim #{id} is not pending manager approval.";
//                return RedirectToAction("Home");
//            }

//            claim.IsManagerApproved = true;
//            claim.Status = ClaimStatus.PendingCoordinatorApproval;
//            claim.LastUpdated = DateTime.Now;

//            TempData["Success"] = $"Claim #{id} approved by Manager and sent to Coordinator.";
//            return RedirectToAction("Home");
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult RejectClaim(int id)
//        {
//            var role = HttpContext.Session.GetString("UserRole");
//            if (role != "Manager")
//            {
//                TempData["Error"] = "Unauthorized access.";
//                return RedirectToAction("Index", "Login");
//            }

//            // Search the shared store
//            var claim = CoordinatorController.ClaimsStore.FirstOrDefault(c => c.ClaimId == id);

//            if (claim == null)
//                return NotFound();

//            if (claim.Status != ClaimStatus.PendingManagerApproval)
//            {
//                TempData["Error"] = $"Claim #{id} is not pending manager approval.";
//                return RedirectToAction("Home");
//            }

//            // Set the status to Rejected
//            claim.Status = ClaimStatus.Rejected;
//            claim.IsManagerApproved = false;
//            claim.LastUpdated = DateTime.Now;

//            TempData["Error"] = $"Claim #{id} rejected by Manager. Lecturer notified.";
//            return RedirectToAction("Home");
//        }
//    }
//}