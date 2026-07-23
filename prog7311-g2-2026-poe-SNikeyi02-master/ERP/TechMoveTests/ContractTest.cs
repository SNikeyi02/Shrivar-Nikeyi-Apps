//using ERP.Controllers;
//using ERP.Data;
//using ERP.Enums;
//using ERP.Models;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using System.Security.Claims;
//using System.Text;

//namespace ERP.Tests
//{
//    [TestFixture]
//    public class ContractControllerTests
//    {
//        private GLMSDbContext _context;
//        private Mock<IWebHostEnvironment> _mockEnv;
//        private ContractController _controller;
//        private const string TestUser = "TestClient";

//        [SetUp]
//        public void Setup()
//        {
//            var options = new DbContextOptionsBuilder<GLMSDbContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;
//            _context = new GLMSDbContext(options);

//            _mockEnv = new Mock<IWebHostEnvironment>();
//            _mockEnv.Setup(m => m.WebRootPath).Returns(Path.GetTempPath());

//            _controller = new ContractController(_context, _mockEnv.Object);

//            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
//            {
//                new Claim(ClaimTypes.Name, TestUser),
//            }, "mock"));

//            _controller.ControllerContext = new ControllerContext()
//            {
//                HttpContext = new DefaultHttpContext() { User = user }
//            };
//        }

//        // --- HELPER METHOD TO MAKE TESTS WORK ---
//        private IFormFile CreateMockFile(string fileName, string contentType, string content)
//        {
//            var bytes = Encoding.UTF8.GetBytes(content);
//            var stream = new MemoryStream(bytes);
//            return new FormFile(stream, 0, bytes.Length, "AgreementFile", fileName)
//            {
//                Headers = new HeaderDictionary(),
//                ContentType = contentType
//            };
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            _controller?.Dispose();
//            _context?.Dispose();
//        }

//        [Test]
//        public async Task Create_ValidPdf_SavesToDbAndRedirects()
//        {
//            _context.Client.Add(new Client
//            {
//                Name = TestUser,
//                Description = "Test Description", 
//                Region = "Test Region"             
//            });
//            await _context.SaveChangesAsync();

//            var contract = new Contract();
//            var file = CreateMockFile("test.pdf", "application/pdf", "fake pdf content");

//            var result = await _controller.Create(contract, file);

//            Assert.IsInstanceOf<RedirectToActionResult>(result);
//            var savedContract = await _context.Contracts.FirstOrDefaultAsync();
//            Assert.IsNotNull(savedContract);
//            Assert.AreEqual(ContractStatus.Active, savedContract.Status);
//        }

//        [Test]
//        public async Task Create_InvalidExtension_ThrowsBadHttpRequestException()
//        {
//            _context.Client.Add(new Client
//            {
//                Name = TestUser,
//                Description = "Test Description", 
//                Region = "Test Region"            
//            });
//            await _context.SaveChangesAsync();

//            var contract = new Contract();
//            var file = CreateMockFile("test.jpg", "image/jpeg", "fake image content");

//            var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () =>
//                await _controller.Create(contract, file));

//            Assert.That(ex.Message, Does.Contain("Only PDF files are allowed"));
//        }

//        [Test]
//        public void Create_NoFile_ThrowsArgumentException()
//        {
//            _context.Client.Add(new Client
//            {
//                Name = TestUser,
//                Description = "Test Description", 
//                Region = "Test Region"          
//            });
//            _context.SaveChanges();

//            Assert.ThrowsAsync<ArgumentException>(async () =>
//                await _controller.Create(new Contract(), null));
//        }

//        [Test]
//        public void Create_UserMappingFails_ThrowsUnauthorizedAccessException()
//        {
//            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
//                await _controller.Create(new Contract(), null));
//        }
//    }
//}