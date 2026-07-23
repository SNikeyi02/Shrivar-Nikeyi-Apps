//using ERP.Controllers;
//using ERP.Data;
//using ERP.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using NUnit.Framework;
//using System.Security.Claims;

//namespace ERP.Tests
//{
//    [TestFixture]
//    public class ClientControllerTests
//    {
//        private GLMSDbContext _context;
//        private ClientController _controller;
//        private const string TestUser = "StandardClient";

//        [SetUp]
//        public void Setup()
//        {
//            // 1. Setup In-Memory Database with a unique name per test
//            var options = new DbContextOptionsBuilder<GLMSDbContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;
//            _context = new GLMSDbContext(options);

//            // 2. Mock the User Identity (User.Identity.Name)
//            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
//            {
//                new Claim(ClaimTypes.Name, TestUser),
//            }, "mock"));

//            _controller = new ClientController(_context);
//            _controller.ControllerContext = new ControllerContext()
//            {
//                HttpContext = new DefaultHttpContext() { User = user }
//            };
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            _context?.Dispose();
//            _controller?.Dispose();
//        }

//        [Test]
//        public async Task Index_UserExists_ReturnsViewWithClientModel()
//        {
//            // Arrange - Including required fields to avoid the DbUpdateException
//            var client = new Client
//            {
//                Name = TestUser,
//                Description = "Test Bio",
//                Region = "Durban"
//            };
//            _context.Client.Add(client);
//            await _context.SaveChangesAsync();

//            // Act
//            var result = await _controller.Index();

//            // Assert
//            Assert.IsInstanceOf<ViewResult>(result);
//            var viewResult = (ViewResult)result;
//            var model = (Client)viewResult.Model;
//            Assert.AreEqual(TestUser, model.Name);
//        }

//        //[Test]
//        //public void Index_UserMissing_ThrowsKeyNotFoundException()
//        //{
//        //    // Act & Assert 
//        //    // We didn't add any client to the DB, so it should throw
//        //    var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
//        //        await _controller.Index());

//        //    Assert.That(ex.Message, Does.Contain($"Client record for '{TestUser}' not found."));
//        //}

//        [Test]
//        public async Task ClientDetails_ValidId_ReturnsView()
//        {
//            // Arrange
//            var clientId = Guid.NewGuid();
//            _context.Client.Add(new Client
//            {
//                ClientId = clientId,
//                Name = "Other Client",
//                Description = "Info",
//                Region = "Cape Town"
//            });
//            await _context.SaveChangesAsync();

//            // Act
//            var result = await _controller.ClientDetails(clientId);

//            // Assert
//            Assert.IsInstanceOf<ViewResult>(result);
//        }

//        [Test]
//        public void ClientDetails_InvalidId_ThrowsKeyNotFoundException()
//        {
//            // Act & Assert
//            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
//                await _controller.ClientDetails(Guid.NewGuid()));
//        }

//        //[Test]
//        //public async Task Details_RequestExists_ReturnsView()
//        //{
//        //    // Arrange
//        //    var requestId = Guid.NewGuid();
//        //    var request = new ServiceRequest
//        //    {
//        //        ServiceRequestId = requestId,
//        //        Description = "Transport Cargo"
//        //    };
//        //    _context.ServiceRequests.Add(request);
//        //    await _context.SaveChangesAsync();

//        //    // Act
//        //    var result = await _controller.Details(requestId);

//        //    // Assert
//        //    Assert.IsInstanceOf<ViewResult>(result);
//        //}

//        [Test]
//        public void CreateContract_RedirectsToContractController()
//        {
//            // Act
//            var result = _controller.CreateContract();

//            // Assert
//            Assert.IsInstanceOf<RedirectToActionResult>(result);
//            var redirect = (RedirectToActionResult)result;
//            Assert.AreEqual("Create", redirect.ActionName);
//            Assert.AreEqual("Contract", redirect.ControllerName);
//        }
//    }
//}