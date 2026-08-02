using Prog_POE.Models;

namespace Prog_POE.Data
{
    //Fake data to populate the database for testing purposes
    public static class SeedData
    {
        public static async Task Initialize(AppDbContext context)
        {
            if (!context.Users.Any())
            {
                var users = new[]
                {
                    new User
                    {
                        Username = "hr@university.com",
                        Password = "hr123",
                        Role = UserRole.HR,
                        FirstName = "HR",
                        LastName = "Administrator",
                        Email = "hr@university.com",
                        Department = "Human Resources",
                        PhoneNumber = "+1234567890",
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        EmployeeId = "HR001"
                    },
                    new User
                    {
                        Username = "lecturer1",
                        Password = "lecturer123",
                        Role = UserRole.Lecturer,
                        FirstName = "John",
                        LastName = "Smith",
                        Email = "john.smith@university.com",
                        Department = "Computer Science",
                        PhoneNumber = "+1234567891",
                        HourlyRate = 85.00,
                        EmployeeId = "LEC001",
                        HireDate = new DateTime(2020, 1, 15)
                    },
                    new User
                    {
                        Username = "lecturer2",
                        Password = "lecturer123",
                        Role = UserRole.Lecturer,
                        FirstName = "Sarah",
                        LastName = "Johnson",
                        Email = "sarah.johnson@university.com",
                        Department = "Mathematics",
                        PhoneNumber = "+1234567892",
                        HourlyRate = 78.50,
                        EmployeeId = "LEC002",
                        HireDate = new DateTime(2019, 8, 22)
                    },
                    new User
                    {
                        Username = "manager1",
                        Password = "manager123",
                        Role = UserRole.Manager,
                        FirstName = "Michael",
                        LastName = "Brown",
                        Email = "michael.brown@university.com",
                        Department = "Administration",
                        PhoneNumber = "+1234567893",
                        EmployeeId = "MGR001"
                    },
                    new User
                    {
                        Username = "coordinator1",
                        Password = "coordinator123",
                        Role = UserRole.Coordinator,
                        FirstName = "Emily",
                        LastName = "Davis",
                        Email = "emily.davis@university.com",
                        Department = "Administration",
                        PhoneNumber = "+1234567894", 
                        EmployeeId = "COORD001"
                    }
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();

                // Seed sample claims
                var claims = new[]
                {
                    new Claim
                    {
                        LecturerId = users[1].Id, // John Smith
                        Description = "Lecture hours for CS101",
                        HoursWorked = 40,
                        HourlyRate = 85.00,
                        WorkDate = DateTime.Now.AddDays(-10),
                        ClaimDate = DateTime.Now.AddDays(-5),
                        Status = ClaimStatus.PendingManagerApproval
                    },
                    new Claim
                    {
                        LecturerId = users[2].Id, // Sarah Johnson
                        Description = "Research supervision",
                        HoursWorked = 25,
                        HourlyRate = 78.50,
                        OvertimeHours = 5,
                        OvertimeRate = 150.00,
                        WorkDate = DateTime.Now.AddDays(-15),
                        ClaimDate = DateTime.Now.AddDays(-8),
                        Status = ClaimStatus.ManagerApproved
                    }
                };

                await context.Claims.AddRangeAsync(claims);
                await context.SaveChangesAsync();
            }
        }
    }
}