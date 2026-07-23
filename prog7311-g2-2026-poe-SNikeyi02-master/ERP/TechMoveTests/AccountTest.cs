//using ERP.Controllers;
//using ERP.Data;
//using ERP.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using NUnit.Framework;
//using System.Collections.Generic;
//using System.Security.Claims;
//using System.Threading.Tasks;

//namespace ERP.Tests
//{
//    [TestFixture]
//    public class AccountControllerTests
//    {
//        private Mock<UserManager<ApplicationUser>> _mockUserManager;
//        private Mock<SignInManager<ApplicationUser>> _mockSignInManager;
//        private AccountController _controller;

//        [SetUp]
//        public void Setup()
//        {
//            // 1. Mock UserManager
//            var userStore = new Mock<IUserStore<ApplicationUser>>();
//            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
//                userStore.Object, null, null, null, null, null, null, null, null);

//            // 2. Mock SignInManager
//            var contextAccessor = new Mock<IHttpContextAccessor>();
//            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
//            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
//                _mockUserManager.Object, contextAccessor.Object, claimsFactory.Object, null, null, null, null);

//            // 3. Initialize Controller
//            _controller = new AccountController(_mockUserManager.Object, _mockSignInManager.Object);
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            _controller?.Dispose();
//        }

//        [Test]
//        public async Task QuickLogin_ValidEmail_SignsInAndRedirects()
//        {
//            // Arrange
//            var email = "admin@test.com";
//            var user = new ApplicationUser { Email = email, UserName = email };

//            _mockUserManager.Setup(u => u.FindByEmailAsync(email))
//                .ReturnsAsync(user);

//            // Act
//            var result = await _controller.QuickLogin(email);

//            // Assert
//            _mockSignInManager.Verify(s => s.SignInAsync(user, false, null), Times.Once);

//            Assert.IsInstanceOf<RedirectToActionResult>(result);
//            var redirect = (RedirectToActionResult)result;
//            Assert.AreEqual("SelectRole", redirect.ActionName);
//            Assert.AreEqual("Home", redirect.ControllerName);
//        }

//        [Test]
//        public void QuickLogin_InvalidEmail_ThrowsKeyNotFoundException()
//        {
//            // Arrange
//            var email = "nonexistent@test.com";
//            _mockUserManager.Setup(u => u.FindByEmailAsync(email))
//                .ReturnsAsync((ApplicationUser)null);

//            // Act & Assert
//            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
//                await _controller.QuickLogin(email));

//            Assert.That(ex.Message, Does.Contain($"User with email '{email}' not found"));
//        }

//        [Test]
//        public async Task Logout_RedirectsToHome()
//        {
//            // Act
//            var result = await _controller.Logout();

//            // Assert
//            _mockSignInManager.Verify(s => s.SignOutAsync(), Times.Once);
//            Assert.IsInstanceOf<RedirectToActionResult>(result);
//            var redirect = (RedirectToActionResult)result;
//            Assert.AreEqual("Index", redirect.ActionName);
//            Assert.AreEqual("Home", redirect.ControllerName);
//        }
//    }
//}