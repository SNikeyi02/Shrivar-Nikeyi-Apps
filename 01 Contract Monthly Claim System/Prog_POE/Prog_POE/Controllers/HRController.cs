using System;
using System.ComponentModel.DataAnnotations;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prog_POE.Data;
using Prog_POE.Models;

// Microsoft Learn 2023. Eager Loading of Related Data - EF Core. 
// Online 
// Available at:
// <https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager>
// [Accessed 20 November 2025].

namespace Prog_POE.Controllers
{
    public class HRController : Controller
    {
        private readonly AppDbContext _context;

        public HRController(AppDbContext context)
        {
            _context = context;
        }

        // HR Dashboard
        // HR Dashboard
        public IActionResult Dashboard()
        {
            if (!IsHRUser()) return RedirectToAction("Index", "Login");

            var stats = new DashboardStats
            {
                TotalLecturers = _context.Users.Count(u => u.Role == UserRole.Lecturer && u.IsActive),
                TotalClaims = _context.Claims.Count(),
                PendingClaims = _context.Claims.Count(c => c.Status == ClaimStatus.PendingManagerApproval),
                ManagerApprovedClaims = _context.Claims.Count(c => c.Status == ClaimStatus.ManagerApproved),
                CoordinatorApprovedClaims = _context.Claims.Count(c => c.Status == ClaimStatus.CoordinatorApproved),
                // Calculate using database columns instead of computed property
                TotalClaimValue = _context.Claims
                    .Where(c => c.Status == ClaimStatus.CoordinatorApproved)
                    .Sum(c => (decimal)(c.HoursWorked * c.HourlyRate + (c.OvertimeHours ?? 0) * (c.OvertimeRate ?? 0)))
            };

            return View(stats);
        }

        // User Management
        public IActionResult ManageUsers()
        {
            if (!IsHRUser()) return RedirectToAction("Index", "Login");
            var users = _context.Users.ToList();
            return View(users);
        }

        // Add User - GET
        public IActionResult AddUser()
        {
            if (!IsHRUser()) return RedirectToAction("Index", "Login");

            try
            {
                var roles = Enum.GetValues(typeof(UserRole))
                    .Cast<UserRole>()
                    .Where(r => r != UserRole.HR)
                    .Select(r => new SelectListItem
                    {
                        Value = r.ToString(),
                        Text = r.ToString()
                    })
                    .ToList();

                // Ensure we have at least one role
                if (!roles.Any())
                {
                    roles = new List<SelectListItem>
            {
                new SelectListItem { Value = UserRole.Lecturer.ToString(), Text = UserRole.Lecturer.ToString() },
                new SelectListItem { Value = UserRole.Manager.ToString(), Text = UserRole.Manager.ToString() },
                new SelectListItem { Value = UserRole.Coordinator.ToString(), Text = UserRole.Coordinator.ToString() }
            };
                }

                ViewBag.Roles = roles;
            }
            catch (Exception ex)
            {
                // Fallback roles if something goes wrong
                ViewBag.Roles = new List<SelectListItem>
        {
            new SelectListItem { Value = "Lecturer", Text = "Lecturer" },
            new SelectListItem { Value = "Manager", Text = "Manager" },
            new SelectListItem { Value = "Coordinator", Text = "Coordinator" }
        };
            }

            return View();
        }

        // Add User - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(User user)
        {
            if (!IsHRUser()) return RedirectToAction("Index", "Login");

            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    ViewBag.Roles = new SelectList(Enum.GetValues(typeof(UserRole)).Cast<UserRole>(), "Value", "Text");
                    return View(user);
                }

                user.CreatedDate = DateTime.Now;
                user.IsActive = true;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"{user.Role} {user.FirstName} {user.LastName} added successfully!";
                return RedirectToAction("ManageUsers");
            }

            ViewBag.Roles = new SelectList(Enum.GetValues(typeof(UserRole)).Cast<UserRole>(), "Value", "Text");
            return View(user);
        }

        // Edit User - GET
        public async Task<IActionResult> EditUser(int id)
        {
            if (!IsHRUser()) return RedirectToAction("Index", "Login");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("ManageUsers");
            }

            ViewBag.Roles = new SelectList(Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Where(r => r != UserRole.HR || user.Role == UserRole.HR)
                .Select(r => new { Value = r, Text = r.ToString() }),
                "Value", "Text", user.Role);

            return View(user);
        }

        // Edit User - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(User user)
        {
            if (!IsHRUser()) return RedirectToAction("Index", "Login");

            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FindAsync(user.Id);
                if (existingUser == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("ManageUsers");
                }

                // Update properties
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.Department = user.Department;
                existingUser.Role = user.Role;
                existingUser.HourlyRate = user.HourlyRate;
                existingUser.EmployeeId = user.EmployeeId;
                existingUser.HireDate = user.HireDate;

                await _context.SaveChangesAsync();
                TempData["Success"] = "User updated successfully!";
                return RedirectToAction("ManageUsers");
            }

            ViewBag.Roles = new SelectList(Enum.GetValues(typeof(UserRole)).Cast<UserRole>(), "Value", "Text", user.Role);
            return View(user);
        }

        // Update Lecturer Hourly Rate via AJAX
        [HttpPost]
        public async Task<IActionResult> UpdateHourlyRate(int lecturerId, double hourlyRate)
        {
            if (!IsHRUser()) return Json(new { success = false, message = "Unauthorized" });

            var lecturer = await _context.Users.FindAsync(lecturerId);
            if (lecturer == null || lecturer.Role != UserRole.Lecturer)
            {
                return Json(new { success = false, message = "Lecturer not found" });
            }

            lecturer.HourlyRate = hourlyRate;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Hourly rate updated successfully" });
        }

        // Claims Management
        public IActionResult ManageClaims()
        {
            if (!IsHRUser()) return RedirectToAction("Index", "Login");

            var claims = _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.AuditTrail)
                .ThenInclude(a => a.Processor)
                .OrderByDescending(c => c.ClaimDate)
                .ToList();

            return View(claims);
        }

        // Generate Reports - GET
        public IActionResult GenerateReport()
        {
            if (!IsHRUser()) return RedirectToAction("Index", "Login");

            ViewBag.Lecturers = new SelectList(_context.Users
                .Where(u => u.Role == UserRole.Lecturer && u.IsActive)
                .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName }),
                "Id", "FullName");

            ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(ClaimStatus)).Cast<ClaimStatus>());

            return View(new ReportFilter());
        }

        // Generate Reports - POST
        [HttpPost]
        public IActionResult GenerateReport(ReportFilter filter)
        {
            if (!IsHRUser()) return RedirectToAction("Index", "Login");

            var query = _context.Claims
                .Include(c => c.Lecturer)
                .AsQueryable();

            // Apply filters using your ClaimStatus enum
            if (!string.IsNullOrEmpty(filter.LecturerName))
            {
                query = query.Where(c => (c.Lecturer.FirstName + " " + c.Lecturer.LastName)
                    .Contains(filter.LecturerName));
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(c => c.Status == filter.Status.Value);
            }

            // For amount filters, we need to calculate in memory or use database columns
            var results = query.ToList(); // Bring to memory first

            // Now apply amount filters in memory
            if (filter.MinAmount.HasValue)
            {
                results = results.Where(c => c.TotalClaimAmount >= filter.MinAmount.Value).ToList();
            }

            if (filter.MaxAmount.HasValue)
            {
                results = results.Where(c => c.TotalClaimAmount <= filter.MaxAmount.Value).ToList();
            }

            if (filter.StartDate.HasValue)
            {
                results = results.Where(c => c.WorkDate >= filter.StartDate.Value).ToList();
            }

            if (filter.EndDate.HasValue)
            {
                results = results.Where(c => c.WorkDate <= filter.EndDate.Value).ToList();
            }

            if (filter.ReportType == "Excel")
            {
                return GenerateExcelReport(results);
            }
            else if (filter.ReportType == "PDF")
            {
                return GeneratePdfReport(results);
            }

            // Return to view with results for preview
            ViewBag.Lecturers = new SelectList(_context.Users
                .Where(u => u.Role == UserRole.Lecturer && u.IsActive)
                .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName }),
                "Id", "FullName");

            ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(ClaimStatus)).Cast<ClaimStatus>());

            ViewBag.ReportResults = results;
            return View(filter);
        }

        // Excel Report Generation
        private IActionResult GenerateExcelReport(List<Claim> claims)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Claims Report");

            // Headers
            var headers = new[] { "Claim ID", "Lecturer", "Description", "Work Date", "Hours", "Hourly Rate", "Overtime Hours", "Total Amount", "Status" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            }

            // Data
            for (int i = 0; i < claims.Count; i++)
            {
                var claim = claims[i];
                worksheet.Cell(i + 2, 1).Value = claim.Id;
                worksheet.Cell(i + 2, 2).Value = $"{claim.Lecturer.FirstName} {claim.Lecturer.LastName}";
                worksheet.Cell(i + 2, 3).Value = claim.Description;
                worksheet.Cell(i + 2, 4).Value = claim.WorkDate.ToString("yyyy-MM-dd");
                worksheet.Cell(i + 2, 5).Value = claim.HoursWorked;
                worksheet.Cell(i + 2, 6).Value = claim.HourlyRate;
                worksheet.Cell(i + 2, 7).Value = claim.OvertimeHours ?? 0;
                worksheet.Cell(i + 2, 8).Value = claim.TotalClaimAmount;
                worksheet.Cell(i + 2, 9).Value = GetDisplayName(claim.Status);
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Claims_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        // PDF Report Generation
        private IActionResult GeneratePdfReport(List<Claim> claims)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);

                    page.Header().Text("Claims Report")
                        .FontSize(20).Bold().AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(); // ID
                            columns.RelativeColumn(1.5f); // Lecturer
                            columns.RelativeColumn(2); // Description
                            columns.RelativeColumn(); // Date
                            columns.RelativeColumn(); // Amount
                            columns.RelativeColumn(1.5f); // Status
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("ID").Bold();
                            header.Cell().Text("Lecturer").Bold();
                            header.Cell().Text("Description").Bold();
                            header.Cell().Text("Date").Bold();
                            header.Cell().Text("Amount").Bold();
                            header.Cell().Text("Status").Bold();
                        });

                        foreach (var claim in claims)
                        {
                            table.Cell().Text(claim.Id.ToString());
                            table.Cell().Text($"{claim.Lecturer.FirstName} {claim.Lecturer.LastName}");
                            table.Cell().Text(claim.Description);
                            table.Cell().Text(claim.WorkDate.ToString("yyyy-MM-dd"));
                            table.Cell().Text($"R {claim.TotalClaimAmount:0.00}");
                            table.Cell().Text(GetDisplayName(claim.Status));
                        }
                    });
                });
            });

            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", $"Claims_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        // Helper method to get display name from ClaimStatus enum
        private string GetDisplayName(ClaimStatus status)
        {
            var field = status.GetType().GetField(status.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;
            return attribute?.Name ?? status.ToString();
        }

        // Claim Audit Log
        public IActionResult ClaimAuditLog(int claimId)
        {
            if (!IsHRUser()) return RedirectToAction("Index", "Login");

            var auditTrail = _context.ClaimAudits
                .Include(a => a.Processor)
                .Include(a => a.Claim)
                .ThenInclude(c => c.Lecturer)
                .Where(a => a.ClaimId == claimId)
                .OrderByDescending(a => a.ActionDate)
                .ToList();

            return View(auditTrail);
        }

        private bool IsHRUser()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole == UserRole.HR.ToString();
        }
    }

    public class DashboardStats
    {
        public int TotalLecturers { get; set; }
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ManagerApprovedClaims { get; set; }
        public int CoordinatorApprovedClaims { get; set; }
        public decimal TotalClaimValue { get; set; }
    }
}