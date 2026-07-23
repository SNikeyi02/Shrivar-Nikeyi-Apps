//using ERP.Controllers;
//using ERP.Data;
//using ERP.Enums;
//using ERP.Models;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.ViewFeatures;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using NUnit.Framework;

//namespace ERP.Tests
//{
//    [TestFixture]
//    public class AdminControllerTests
//    {
//        private GLMSDbContext _context;
//        private Mock<IWebHostEnvironment> _mockEnv;
//        private Mock<UserManager<ApplicationUser>> _mockUserManager;
//        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
//        private AdminController _controller;

//        [SetUp]
//        public void Setup()
//        {
//            // 1. In-Memory DB
//            var options = new DbContextOptionsBuilder<GLMSDbContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;
//            _context = new GLMSDbContext(options);

//            // 2. Mock Identity Managers
//            var userStore = new Mock<IUserStore<ApplicationUser>>();
//            _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

//            var roleStore = new Mock<IRoleStore<IdentityRole>>();
//            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);

//            // 3. Mock Environment
//            _mockEnv = new Mock<IWebHostEnvironment>();
//            _mockEnv.Setup(m => m.WebRootPath).Returns(Path.GetTempPath());

//            // 4. Initialize Controller
//            _controller = new AdminController(_context, _mockEnv.Object, _mockUserManager.Object, _mockRoleManager.Object);

//            // Setup TempData for RedirectActions
//            var tempDataProvider = new Mock<ITempDataProvider>();
//            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), tempDataProvider.Object);
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            _context?.Dispose();
//            _controller?.Dispose();
//        }

//        [Test]
//        public async Task AdminDashboard_ReturnsViewWithContracts()
//        {
//            // Arrange
//            _context.Contracts.Add(new Contract { ContractId = Guid.NewGuid(), StartDate = DateTime.Now });
//            await _context.SaveChangesAsync();

//            // Act
//            var result = await _controller.AdminDashboard();

//            // Assert
//            Assert.IsInstanceOf<ViewResult>(result);
//            var viewResult = (ViewResult)result;
//            var model = (List<Contract>)viewResult.Model;
//            Assert.AreEqual(1, model.Count);
//        }

//        [Test]
//        public async Task CreateClient_ValidData_CreatesUserAndProfile()
//        {
//            // Arrange
//            _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
//                .ReturnsAsync(IdentityResult.Success);
//            _mockRoleManager.Setup(r => r.RoleExistsAsync("Client")).ReturnsAsync(true);
//            _mockUserManager.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Client"))
//                .ReturnsAsync(IdentityResult.Success);

//            // Act
//            var result = await _controller.CreateClient("test@erp.com", "John Doe", "North", "Password123!");

//            // Assert
//            Assert.IsInstanceOf<RedirectToActionResult>(result);
//            var clientInDb = await _context.Client.FirstOrDefaultAsync(c => c.Name == "test@erp.com");
//            Assert.IsNotNull(clientInDb);
//            Assert.AreEqual("John Doe", clientInDb.Description);
//        }

//        [Test]
//        public void CreateClient_IdentityFails_ThrowsInvalidOperationException()
//        {
//            // Arrange
//            _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
//                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email taken" }));

//            // Act & Assert
//            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
//                await _controller.CreateClient("fail@erp.com", "Name", "Reg", "Pass"));

//            Assert.That(ex.Message, Does.Contain("Email taken"));
//        }

//        [Test]
//        public async Task ChangeStatus_ValidContract_UpdatesAndNotifies()
//        {
//            // Arrange
//            var contractId = Guid.NewGuid();
//            var contract = new Contract { ContractId = contractId, Status = ContractStatus.Active };
//            _context.Contracts.Add(contract);
//            await _context.SaveChangesAsync();

//            // Act
//            var result = await _controller.ChangeStatus(contractId, ContractStatus.Expired);

//            // Assert
//            var updated = await _context.Contracts.FindAsync(contractId);
//            Assert.AreEqual(ContractStatus.Expired, updated.Status);
//            Assert.IsInstanceOf<RedirectToActionResult>(result);
//        }

//        [Test]
//        public void ViewContract_InvalidId_ThrowsKeyNotFoundException()
//        {
//            // Act & Assert
//            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
//                await _controller.ViewContract(Guid.NewGuid()));
//        }

//        [Test]
//        public void ViewPDF_FileMissingOnDisk_ThrowsFileNotFoundException()
//        {
//            // Arrange
//            var contractId = Guid.NewGuid();
//            var contract = new Contract { ContractId = contractId, AgreementFilePath = "missing.pdf" };
//            _context.Contracts.Add(contract);
//            _context.SaveChanges();

//            // Act & Assert
//            Assert.ThrowsAsync<FileNotFoundException>(async () =>
//                await _controller.ViewPDF(contractId));
//        }
//    }
//}