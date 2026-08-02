using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
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
    public class LecturerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEncryptionService _encryptionService;

        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".xlsx", ".png", ".jpg", ".jpeg" };
        private const long MaxFileSize = 5 * 1024 * 1024;

        public LecturerController(AppDbContext context, IEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        [HttpGet]
        public IActionResult Home()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Lecturer")
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Index", "Login");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var claimCount = _context.Claims.Count(c => c.LecturerId == userId);

            ViewData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["TotalClaims"] = claimCount;

            return View();
        }

        [HttpGet]
        public IActionResult SubmitClaim()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Lecturer")
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Index", "Login");
            }

            return View(new LecturerClaimView());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(LecturerClaimView model, List<IFormFile> SupportingDocuments)
        {
            try
            {
                if (HttpContext.Session.GetString("UserRole") != "Lecturer")
                {
                    TempData["Error"] = "Unauthorized access.";
                    return RedirectToAction("Index", "Login");
                }

                const double FixedHourlyRate = 75.0;
                const double FixedOvertimeRate = 150.0;

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fix the validation errors below.";
                    return View(model);
                }

                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    TempData["Error"] = "User session expired. Please login again.";
                    return RedirectToAction("Index", "Login");
                }

                var lecturer = await _context.Users.FindAsync(userId);
                if (lecturer == null)
                {
                    TempData["Error"] = "Lecturer not found.";
                    return RedirectToAction("Index", "Login");
                }

                // Create claim in database
                var claim = new Claim
                {
                    LecturerId = userId.Value,
                    Description = model.Description ?? "No description provided",
                    HoursWorked = model.HoursWorked,
                    HourlyRate = FixedHourlyRate,
                    OvertimeHours = model.OvertimeHours,
                    OvertimeRate = FixedOvertimeRate,
                    WorkDate = DateTime.Now,
                    ClaimDate = DateTime.Now,
                    LastUpdated = DateTime.Now,
                    Status = ClaimStatus.PendingManagerApproval
                };

                Console.WriteLine($"=== DEBUG: Creating claim with Status: {claim.Status}");

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                Console.WriteLine($"=== DEBUG: Claim saved with ID: {claim.Id}, Status: {claim.Status}");

                // Process documents
                var secureFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SecureUploads");
                Directory.CreateDirectory(secureFolderPath);

                foreach (var file in SupportingDocuments ?? new List<IFormFile>())
                {
                    if (file == null || file.Length == 0)
                        continue;

                    var extension = Path.GetExtension(file.FileName)?.ToLower();

                    if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
                        continue;

                    if (file.Length > MaxFileSize)
                        continue;

                    try
                    {
                        var secureFileName = _encryptionService.GenerateSecureFileName(file.FileName);
                        var filePath = Path.Combine(secureFolderPath, secureFileName);

                        using (var inputStream = file.OpenReadStream())
                        using (var outputStream = new FileStream(filePath, FileMode.Create))
                        {
                            _encryptionService.EncryptStream(inputStream, outputStream);
                        }

                        
                        var document = new SupportingDocument
                        {
                            ClaimId = claim.Id,
                            FileName = file.FileName,
                            StoredFileName = secureFileName,
                            FileSize = file.Length,
                            UploadDate = DateTime.Now
                        };

                        _context.SupportingDocuments.Add(document);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing file: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                // Create audit trail
                var audit = new ClaimAudit
                {
                    ClaimId = claim.Id,
                    ProcessorId = userId.Value,
                    OldStatus = ClaimStatus.PendingManagerApproval,
                    NewStatus = ClaimStatus.PendingManagerApproval,
                    ActionType = "Created",
                    Comments = "Claim submitted by lecturer",
                    ActionDate = DateTime.Now
                };

                _context.ClaimAudits.Add(audit);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Claim #{claim.Id} submitted successfully!";
                return RedirectToAction("PastClaims");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SubmitClaim: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                TempData["Error"] = $"An error occurred while submitting the claim: {ex.Message}";
                return View(model);
            }
        }


        [HttpGet]
        public IActionResult DownloadDocument(string fileName)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var currentUsername = HttpContext.Session.GetString("Username");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(fileName) || !fileName.EndsWith(".encrypted") || string.IsNullOrEmpty(role))
            {
                TempData["Error"] = "Invalid file request.";
                return RedirectToAction("Index", "Login");
            }

            var secureFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SecureUploads");
            var filePath = Path.Combine(secureFolderPath, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            // Authorization check using database
            var document = _context.SupportingDocuments
                .Include(d => d.Claim)
                .ThenInclude(c => c.Lecturer)
                .FirstOrDefault(d => d.StoredFileName == fileName);

            if (document == null)
                return Forbid("File not linked to any valid claim.");

            bool isAuthorized = (role == "Lecturer" && document.Claim.LecturerId == userId) ||
                               (role == "Manager" && document.Claim.Status == ClaimStatus.PendingManagerApproval) ||
                               (role == "Coordinator" && (document.Claim.Status == ClaimStatus.PendingCoordinatorApproval ||
                                                         document.Claim.Status == ClaimStatus.CoordinatorApproved));

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

        [HttpGet]
        public async Task<IActionResult> PastClaims()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Lecturer")
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Index", "Login");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var claims = await _context.Claims
                .Where(c => c.LecturerId == userId)
                .OrderByDescending(c => c.ClaimDate)
                .ToListAsync();

            // Convert to view model
            var claimViews = claims.Select(c => new LecturerClaimView
            {
                ClaimId = c.Id,
                LecturerName = HttpContext.Session.GetString("Username"),
                HoursWorked = c.HoursWorked,
                OvertimeHours = c.OvertimeHours,
                HourlyRate = c.HourlyRate,
                OvertimeRate = c.OvertimeRate,
                ClaimDate = c.ClaimDate,
                LastUpdated = c.LastUpdated,
                Status = c.Status,
                SupportingDocumentNames = _context.SupportingDocuments
                    .Where(d => d.ClaimId == c.Id)
                    .Select(d => d.StoredFileName)
                    .ToList()
            }).ToList();

            return View(claimViews);
        }

        private async Task MigrateStaticDataToDatabase()
        {
            
        }
    }
}
