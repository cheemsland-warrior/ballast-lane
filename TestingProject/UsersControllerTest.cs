using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BallastLaneApi.Controllers;
using BallastLaneApi.Data;
using BallastLaneApi.Models;
using BallastLaneApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TestingProject
{
    [TestClass]
    public class UsersControllerTest
    {
        private ApplicationDbContext _dbContext;
        private IUserService _userService;
        private UsersController _controller;
        private IMapper _mapper;
        private Mock<IAuthService> _mockAuthService;
        private Mock<IConfiguration> _mockConfig;

        [TestInitialize]
        public void Setup()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _userService = new UserService(_dbContext);

            // Setup AutoMapper with mock
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<List<UserDto>>(It.IsAny<List<User>>()))
                .Returns((List<User> users) =>
                {
                    var dtos = new List<UserDto>();
                    foreach (var user in users)
                    {
                        dtos.Add(new UserDto
                        {
                            Id = user.Id,
                            Email = user.Email,
                            DisplayName = user.DisplayName,
                            CreatedDate = user.CreatedDate
                        });
                    }
                    return dtos;
                });

            mockMapper.Setup(m => m.Map<UserDto>(It.IsAny<User>()))
                .Returns((User user) => new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    CreatedDate = user.CreatedDate
                });

            _mapper = mockMapper.Object;

            // Setup mock auth service
            _mockAuthService = new Mock<IAuthService>();

            // Setup mock configuration
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["Jwt:Key"]).Returns("your-secret-key-must-be-at-least-32-characters-long");

            // Create controller
            _controller = new UsersController(_userService, _mapper, _mockAuthService.Object, _mockConfig.Object);

            // Setup mock HttpContext with cookie authentication services
            var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();
            httpContext.RequestServices = services.BuildServiceProvider();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext?.Dispose();
        }

        #region Get Tests

        [TestMethod]
        public async Task Get_ReturnsAllUsers()
        {
            // Arrange
            var user1 = new User { Email = "user1@example.com", DisplayName = "User One" };
            var user2 = new User { Email = "user2@example.com", DisplayName = "User Two" };
            var created1 = await _userService.CreateAsync(user1, "password123");
            var created2 = await _userService.CreateAsync(user2, "password123");

            // Act
            var result = await _controller.Get();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var users = okResult.Value as List<UserDto>;
            Assert.IsNotNull(users);
            Assert.AreEqual(2, users.Count);
        }

        [TestMethod]
        public async Task Get_ReturnsEmptyList_WhenNoUsersExist()
        {
            // Act
            var result = await _controller.Get();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var users = okResult.Value as List<UserDto>;
            Assert.IsNotNull(users);
            Assert.AreEqual(0, users.Count);
        }

        #endregion

        #region Register Tests

        [TestMethod]
        public async Task Register_ReturnsCreatedResult_WithValidInput()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "newuser@example.com",
                DisplayName = "New User",
                Password = "password123"
            };

            var authResponse = new AuthResponse { Token = "jwt-token-here", Expires = DateTime.UtcNow.AddHours(8) };
            _mockAuthService.Setup(a => a.AuthenticateAsync("newuser@example.com", "password123"))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            Assert.IsNotNull(result);
            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(nameof(_controller.Get), createdResult.ActionName);

            // Verify user was created in database
            var savedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
            Assert.IsNotNull(savedUser);
            Assert.AreEqual("New User", savedUser.DisplayName);
        }

        [TestMethod]
        public async Task Register_ReturnsBadRequest_WhenInvalidEmail()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "invalid-email",
                DisplayName = "Test User",
                Password = "password123"
            };

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            Assert.IsNotNull(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task Register_ReturnsBadRequest_WhenPasswordTooShort()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "user@example.com",
                DisplayName = "Test User",
                Password = "short"
            };

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            Assert.IsNotNull(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task Register_ReturnsConflict_WhenEmailAlreadyExists()
        {
            // Arrange
            // Create first user
            var existingUser = new User { Email = "user@example.com", DisplayName = "Existing User" };
            await _userService.CreateAsync(existingUser, "password123");

            // Try to register with same email
            var registerRequest = new RegisterRequest
            {
                Email = "user@example.com",
                DisplayName = "Different User",
                Password = "password456"
            };

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            Assert.IsNotNull(result);
            var conflictResult = result as ConflictObjectResult;
            Assert.IsNotNull(conflictResult);
            Assert.AreEqual(409, conflictResult.StatusCode);
        }

        #endregion

        #region Login Tests

        [TestMethod]
        public async Task Login_ReturnsOkWithToken_WhenCredentialsValid()
        {
            // Arrange
            var user = new User { Email = "user@example.com", DisplayName = "Test User" };
            var createdUser = await _userService.CreateAsync(user, "password123");

            var loginRequest = new LoginRequest
            {
                Email = "user@example.com",
                Password = "password123"
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            // Response is an anonymous object with Id and token properties
            var responseType = okResult.Value?.GetType();
            Assert.IsNotNull(responseType);

            var idProperty = responseType.GetProperty("Id");
            var tokenProperty = responseType.GetProperty("token");

            Assert.IsNotNull(idProperty);
            Assert.IsNotNull(tokenProperty);

            var idValue = idProperty.GetValue(okResult.Value);
            var tokenValue = tokenProperty.GetValue(okResult.Value);

            Assert.AreEqual(createdUser.Id, idValue.ToString());
            Assert.IsNotNull(tokenValue);
        }

        [TestMethod]
        public async Task Login_ReturnsBadRequest_WhenEmailEmpty()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "",
                Password = "password123"
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsNotNull(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task Login_ReturnsBadRequest_WhenPasswordEmpty()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "user@example.com",
                Password = ""
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsNotNull(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task Login_ReturnsBadRequest_WhenBothEmpty()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "",
                Password = ""
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsNotNull(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "nonexistent@example.com",
                Password = "password123"
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsNotNull(result);
            var unauthorizedResult = result as UnauthorizedResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task Login_ReturnsUnauthorized_WhenPasswordIncorrect()
        {
            // Arrange
            var user = new User { Email = "user@example.com", DisplayName = "Test User" };
            await _userService.CreateAsync(user, "correctpassword");

            var loginRequest = new LoginRequest
            {
                Email = "user@example.com",
                Password = "wrongpassword"
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsNotNull(result);
            var unauthorizedResult = result as UnauthorizedResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task Login_TokenIsJWT_WhenCredentialsValid()
        {
            // Arrange
            var user = new User { Email = "user@example.com", DisplayName = "Test User" };
            var createdUser = await _userService.CreateAsync(user, "password123");

            var loginRequest = new LoginRequest
            {
                Email = "user@example.com",
                Password = "password123"
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = result as OkObjectResult;
            var responseType = okResult.Value?.GetType();
            var tokenProperty = responseType.GetProperty("token");
            var tokenValue = tokenProperty.GetValue(okResult.Value) as string;

            // JWT tokens have 3 parts separated by dots
            var parts = tokenValue.Split('.');
            Assert.AreEqual(3, parts.Length);
        }

        #endregion

        #region Logout Tests

        [TestMethod]
        public async Task Logout_ReturnsNoContent()
        {
            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.IsNotNull(result);
            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult);
            Assert.AreEqual(204, noContentResult.StatusCode);
        }

        #endregion
    }
}
