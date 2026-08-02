//using ERP.Controllers;
//using ERP.Data;
//using ERP.Enums;
//using ERP.Models;
//using ERP.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using NUnit.Framework;

//namespace ERP.Tests
//{
//    [TestFixture]
//    public class ServiceRequestControllerTests
//    {
//        private GLMSDbContext _context;
//        private Mock<ICurrencyService> _mockCurrencyService;
//        private ServiceRequestController _controller;

//        [SetUp]
//        public void Setup()
//        {
//            // 1. Setup In-Memory Database
//            var options = new DbContextOptionsBuilder<GLMSDbContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;

//            _context = new GLMSDbContext(options);

//            // 2. Mock Currency Service
//            _mockCurrencyService = new Mock<ICurrencyService>();

//            // 3. Initialize Controller
//            _controller = new ServiceRequestController(_context, _mockCurrencyService.Object);
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            // Dispose controller first (it may use the context), then dispose the context.
//            _controller?.Dispose();
//            _context?.Dispose();
//        }

//        [Test]
//        public async Task Create_Post_ValidRequest_RedirectsToIndex()
//        {
//            // Arrange
//            var contractId = Guid.NewGuid();
//            var contract = new Contract { ContractId = contractId, Status = ContractStatus.Active };
//            _context.Contracts.Add(contract);
//            await _context.SaveChangesAsync();

//            var model = new ServiceRequest { ContractID = contractId, CostUSD = 100 };
//            _mockCurrencyService.Setup(s => s.ConvertUsdToZarAsync(100)).ReturnsAsync(1900m);

//            // Act
//            var result = await _controller.Create(model, "Air");

//            // Assert
//            Assert.IsInstanceOf<RedirectToActionResult>(result);
//            var redirect = (RedirectToActionResult)result;
//            Assert.AreEqual("Index", redirect.ActionName);
//            Assert.AreEqual("Client", redirect.ControllerName);
//        }

//        [Test]
//        public void Create_Post_InactiveContract_ThrowsInvalidOperationException()
//        {
//            // Arrange
//            var contractId = Guid.NewGuid();
//            var contract = new Contract { ContractId = contractId, Status = ContractStatus.Expired }; // Inactive
//            _context.Contracts.Add(contract);
//            _context.SaveChanges();

//            var model = new ServiceRequest { ContractID = contractId };

//            // Act & Assert
//            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
//                await _controller.Create(model, "Road"));

//            Assert.AreEqual("Service Requests can only be created for ACTIVE contracts.", ex.Message);
//        }

//        [Test]
//        public void Create_Post_NonExistentContract_ThrowsKeyNotFoundException()
//        {
//            // Arrange
//            var model = new ServiceRequest { ContractID = Guid.NewGuid() };

//            // Act & Assert
//            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
//                await _controller.Create(model, "Sea"));
//        }

//        [Test]
//        public async Task Create_Post_CurrencyConversion_Success_CalculatesZar()
//        {
//            // Arrange
//            var contractId = Guid.NewGuid();
//            var contract = new Contract { ContractId = contractId, Status = ContractStatus.Active };
//            _context.Contracts.Add(contract);
//            await _context.SaveChangesAsync();

//            var model = new ServiceRequest
//            {
//                ContractID = contractId,
//                CostUSD = 50 // $50 USD
//            };

//            // Mocking the exchange rate at 1 USD = 19 ZAR
//            decimal expectedZar = 50 * 19m;
//            _mockCurrencyService.Setup(s => s.ConvertUsdToZarAsync(50))
//                .ReturnsAsync(expectedZar);

//            // Act
//            await _controller.Create(model, "Sea");

//            // Assert
//            var savedRequest = await _context.ServiceRequests.FirstOrDefaultAsync();
//            Assert.IsNotNull(savedRequest);
//            Assert.AreEqual(expectedZar, savedRequest.CostZAR, "The ZAR cost should be correctly calculated and saved.");
//        }

//        [Test]
//        public async Task Create_Post_CurrencyServiceFails_ThrowsExceptionAsync()
//        {
//            // Arrange
//            var contractId = Guid.NewGuid();
//            var contract = new Contract { ContractId = contractId, Status = ContractStatus.Active };
//            _context.Contracts.Add(contract);
//            await _context.SaveChangesAsync();

//            var model = new ServiceRequest { ContractID = contractId, CostUSD = 100 };

//            // Simulate an API failure or network error in the Currency Service
//            _mockCurrencyService.Setup(s => s.ConvertUsdToZarAsync(It.IsAny<decimal>()))
//                .ThrowsAsync(new HttpRequestException("Currency API is unreachable."));

//            // Act & Assert
//            // This verifies that the controller doesn't "swallow" the error, 
//            // allowing your Middleware to catch it and show the popup.
//            var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
//                await _controller.Create(model, "Air"));

//            Assert.AreEqual("Currency API is unreachable.", ex.Message);
//        }
//    }
//}