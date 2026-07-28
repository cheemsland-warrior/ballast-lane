using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BallastLaneApi.Controllers;
using BallastLaneApi.Data;
using BallastLaneApi.Models;
using BallastLaneApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TestingProject
{
    [TestClass]
    public class PotholesControllerTest
    {
        private PotholesController _controller;
        private IPotholeService _potholeService;
        private IUserService _userService;
        private ApplicationDbContext _dbContext;
        private Mock<IMapper> _mockMapper;
        private User _testUser;
        private Pothole _testPothole;

        [TestInitialize]
        public async Task TestInitialize()
        {
            // Setup InMemory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _potholeService = new PotholeService(_dbContext);
            _userService = new UserService(_dbContext);

            // Setup AutoMapper mock
            _mockMapper = new Mock<IMapper>();

            // Create test user
            _testUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = "testuser@example.com",
                DisplayName = "Test User",
                CreatedDate = DateTime.Now
            };

            await _userService.CreateAsync(_testUser, "Password123!");

            // Create test pothole
            _testPothole = new Pothole
            {
                Id = Guid.NewGuid().ToString(),
                Description = "Large pothole on Main St",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Status = "Reported",
                UserId = _testUser.Id,
                CreatedDate = DateTime.Now
            };

            await _potholeService.CreateAsync(_testPothole);

            // Create controller
            _controller = new PotholesController(_dbContext, _potholeService, _mockMapper.Object);

            // Setup mock HttpContext with authenticated user
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _testUser.Id)
            };
            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);
            httpContext.User = principal;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        #region GetAll Tests

        [TestMethod]
        public async Task GetAll_ReturnsOkResult()
        {
            // Arrange
            var potholes = new List<Pothole> { _testPothole };
            var dtos = new List<PotholeDto> 
            { 
                new PotholeDto 
                { 
                    Id = _testPothole.Id, 
                    Description = _testPothole.Description,
                    Latitude = _testPothole.Latitude,
                    Longitude = _testPothole.Longitude,
                    Status = _testPothole.Status,
                    CreatedDate = _testPothole.CreatedDate,
                    UserId = _testPothole.UserId
                } 
            };
            _mockMapper.Setup(m => m.Map<List<PotholeDto>>(potholes))
                .Returns(dtos);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var returnedDtos = okResult.Value as List<PotholeDto>;
            Assert.IsNotNull(returnedDtos);
            Assert.AreEqual(1, returnedDtos.Count);
        }

        [TestMethod]
        public async Task GetAll_ReturnsEmptyList_WhenNoPotholesExist()
        {
            // Arrange
            var emptyDb = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var emptyContext = new ApplicationDbContext(emptyDb);
            var emptyService = new PotholeService(emptyContext);
            var emptyController = new PotholesController(emptyContext, emptyService, _mockMapper.Object);

            var emptyDtos = new List<PotholeDto>();
            _mockMapper.Setup(m => m.Map<List<PotholeDto>>(It.IsAny<List<Pothole>>()))
                .Returns(emptyDtos);

            // Act
            var result = await emptyController.GetAll();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedDtos = okResult.Value as List<PotholeDto>;
            Assert.IsNotNull(returnedDtos);
            Assert.AreEqual(0, returnedDtos.Count);
        }

        [TestMethod]
        public async Task GetAll_ReturnsMappedDtos()
        {
            // Arrange
            var potholes = new List<Pothole> { _testPothole };
            var dtos = new List<PotholeDto>
            {
                new PotholeDto
                {
                    Id = _testPothole.Id,
                    Description = _testPothole.Description,
                    Latitude = _testPothole.Latitude,
                    Longitude = _testPothole.Longitude,
                    Status = _testPothole.Status,
                    CreatedDate = _testPothole.CreatedDate,
                    UserId = _testPothole.UserId
                }
            };
            _mockMapper.Setup(m => m.Map<List<PotholeDto>>(potholes))
                .Returns(dtos);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result as OkObjectResult;
            var returnedDtos = okResult.Value as List<PotholeDto>;
            Assert.AreEqual(_testPothole.Id, returnedDtos[0].Id);
            Assert.AreEqual(_testPothole.Description, returnedDtos[0].Description);
            _mockMapper.Verify(m => m.Map<List<PotholeDto>>(It.IsAny<List<Pothole>>()), Times.Once);
        }

        #endregion

        #region GetById Tests

        [TestMethod]
        public async Task GetById_ReturnsOkResult_WhenPotholeExists()
        {
            // Arrange
            var dto = new PotholeDto
            {
                Id = _testPothole.Id,
                Description = _testPothole.Description,
                Latitude = _testPothole.Latitude,
                Longitude = _testPothole.Longitude,
                Status = _testPothole.Status,
                CreatedDate = _testPothole.CreatedDate,
                UserId = _testPothole.UserId
            };
            _mockMapper.Setup(m => m.Map<PotholeDto>(_testPothole))
                .Returns(dto);

            // Act
            var result = await _controller.GetById(_testPothole.Id);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var returnedDto = okResult.Value as PotholeDto;
            Assert.IsNotNull(returnedDto);
            Assert.AreEqual(_testPothole.Id, returnedDto.Id);
        }

        [TestMethod]
        public async Task GetById_ReturnsNotFound_WhenPotholeDoesNotExist()
        {
            // Arrange
            var nonexistentId = Guid.NewGuid().ToString();

            // Act
            var result = await _controller.GetById(nonexistentId);

            // Assert
            Assert.IsNotNull(result);
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task GetById_ReturnsCorrectPothole_WhenIdMatches()
        {
            // Arrange
            var dto = new PotholeDto
            {
                Id = _testPothole.Id,
                Description = _testPothole.Description,
                Latitude = _testPothole.Latitude,
                Longitude = _testPothole.Longitude,
                Status = _testPothole.Status,
                CreatedDate = _testPothole.CreatedDate,
                UserId = _testPothole.UserId
            };
            _mockMapper.Setup(m => m.Map<PotholeDto>(_testPothole))
                .Returns(dto);

            // Act
            var result = await _controller.GetById(_testPothole.Id);

            // Assert
            var okResult = result as OkObjectResult;
            var returnedDto = okResult.Value as PotholeDto;
            Assert.AreEqual(_testPothole.Description, returnedDto.Description);
            Assert.AreEqual(_testPothole.Latitude, returnedDto.Latitude);
            Assert.AreEqual(_testPothole.Longitude, returnedDto.Longitude);
        }

        #endregion

        #region Create Tests

        [TestMethod]
        public async Task Create_ReturnsCreatedAtActionResult_WithValidInput()
        {
            // Arrange
            var createDto = new PotholeToCreateDto
            {
                Description = "New pothole",
                Latitude = 40.7580,
                Longitude = -73.9855,
                Status = "Reported",
                UserId = _testUser.Id
            };

            var mappedPothole = new Pothole
            {
                Id = Guid.NewGuid().ToString(),
                Description = createDto.Description,
                Latitude = createDto.Latitude,
                Longitude = createDto.Longitude,
                Status = createDto.Status,
                UserId = createDto.UserId,
                CreatedDate = DateTime.Now
            };

            var returnedDto = new PotholeDto
            {
                Id = mappedPothole.Id,
                Description = mappedPothole.Description,
                Latitude = mappedPothole.Latitude,
                Longitude = mappedPothole.Longitude,
                Status = mappedPothole.Status,
                CreatedDate = mappedPothole.CreatedDate,
                UserId = mappedPothole.UserId
            };

            _mockMapper.Setup(m => m.Map<Pothole>(createDto))
                .Returns(mappedPothole);
            _mockMapper.Setup(m => m.Map<PotholeDto>(It.IsAny<Pothole>()))
                .Returns(returnedDto);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            Assert.IsNotNull(result);
            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(nameof(_controller.GetById), createdResult.ActionName);
            Assert.AreEqual(201, createdResult.StatusCode);
        }

        [TestMethod]
        public async Task Create_ReturnsForbid_WhenUserNotAuthenticated()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // No identity
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var createDto = new PotholeToCreateDto
            {
                Description = "New pothole",
                Latitude = 40.7580,
                Longitude = -73.9855,
                Status = "Reported"
            };

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            Assert.IsNotNull(result);
            var forbidResult = result as ForbidResult;
            Assert.IsNotNull(forbidResult);
        }

        [TestMethod]
        public async Task Create_ReturnsBadRequest_WhenLatitudeInvalid()
        {
            // Arrange
            var createDto = new PotholeToCreateDto
            {
                Description = "New pothole",
                Latitude = 91.0, // Invalid latitude (> 90)
                Longitude = -73.9855,
                Status = "Reported",
                UserId = _testUser.Id
            };

            var mappedPothole = new Pothole
            {
                Id = Guid.NewGuid().ToString(),
                Description = createDto.Description,
                Latitude = createDto.Latitude,
                Longitude = createDto.Longitude,
                Status = createDto.Status,
                UserId = createDto.UserId,
                CreatedDate = DateTime.Now
            };

            _mockMapper.Setup(m => m.Map<Pothole>(createDto))
                .Returns(mappedPothole);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            Assert.IsNotNull(result);
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [TestMethod]
        public async Task Create_ReturnsBadRequest_WhenLongitudeInvalid()
        {
            // Arrange
            var createDto = new PotholeToCreateDto
            {
                Description = "New pothole",
                Latitude = 40.7128,
                Longitude = 181.0, // Invalid longitude (> 180)
                Status = "Reported",
                UserId = _testUser.Id
            };

            var mappedPothole = new Pothole
            {
                Id = Guid.NewGuid().ToString(),
                Description = createDto.Description,
                Latitude = createDto.Latitude,
                Longitude = createDto.Longitude,
                Status = createDto.Status,
                UserId = createDto.UserId,
                CreatedDate = DateTime.Now
            };

            _mockMapper.Setup(m => m.Map<Pothole>(createDto))
                .Returns(mappedPothole);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            Assert.IsNotNull(result);
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [TestMethod]
        public async Task Create_ReturnsConflict_WhenUserNotFound()
        {
            // Arrange
            var createDto = new PotholeToCreateDto
            {
                Description = "New pothole",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Status = "Reported",
                UserId = Guid.NewGuid().ToString() // Nonexistent user
            };

            var mappedPothole = new Pothole
            {
                Id = Guid.NewGuid().ToString(),
                Description = createDto.Description,
                Latitude = createDto.Latitude,
                Longitude = createDto.Longitude,
                Status = createDto.Status,
                UserId = createDto.UserId,
                CreatedDate = DateTime.Now
            };

            _mockMapper.Setup(m => m.Map<Pothole>(createDto))
                .Returns(mappedPothole);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            Assert.IsNotNull(result);
            var conflictResult = result as ConflictObjectResult;
            Assert.IsNotNull(conflictResult);
            Assert.AreEqual(409, conflictResult.StatusCode);
        }

        [TestMethod]
        public async Task Create_SetsUserIdFromClaimsPrincipal()
        {
            // Arrange
            var createDto = new PotholeToCreateDto
            {
                Description = "New pothole",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Status = "Reported"
                // Note: UserId not set; should be populated from authenticated user
            };

            var mappedPothole = new Pothole
            {
                Id = Guid.NewGuid().ToString(),
                Description = createDto.Description,
                Latitude = createDto.Latitude,
                Longitude = createDto.Longitude,
                Status = createDto.Status,
                UserId = _testUser.Id,
                CreatedDate = DateTime.Now
            };

            var returnedDto = new PotholeDto
            {
                Id = mappedPothole.Id,
                Description = mappedPothole.Description,
                Latitude = mappedPothole.Latitude,
                Longitude = mappedPothole.Longitude,
                Status = mappedPothole.Status,
                CreatedDate = mappedPothole.CreatedDate,
                UserId = mappedPothole.UserId
            };

            _mockMapper.Setup(m => m.Map<Pothole>(createDto))
                .Returns(mappedPothole);
            _mockMapper.Setup(m => m.Map<PotholeDto>(It.IsAny<Pothole>()))
                .Returns(returnedDto);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
        }

        #endregion

        #region Delete Tests

        [TestMethod]
        public async Task Delete_ReturnsOkResult_WhenPotholeExists()
        {
            // Act
            var result = await _controller.Delete(_testPothole.Id);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        [TestMethod]
        public async Task Delete_ReturnsForbid_WhenUserNotAuthenticated()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // No identity
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.Delete(_testPothole.Id);

            // Assert
            Assert.IsNotNull(result);
            var forbidResult = result as ForbidResult;
            Assert.IsNotNull(forbidResult);
        }

        [TestMethod]
        public async Task Delete_ReturnsBadRequest_WhenIdIsEmpty()
        {
            // Act
            var result = await _controller.Delete("");

            // Assert
            Assert.IsNotNull(result);
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [TestMethod]
        public async Task Delete_ReturnsBadRequest_WhenIdIsWhitespace()
        {
            // Act
            var result = await _controller.Delete("   ");

            // Assert
            Assert.IsNotNull(result);
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [TestMethod]
        public async Task Delete_ReturnsConflict_WhenPotholeNotFound()
        {
            // Arrange
            var nonexistentId = Guid.NewGuid().ToString();

            // Act
            var result = await _controller.Delete(nonexistentId);

            // Assert
            Assert.IsNotNull(result);
            var conflictResult = result as ConflictObjectResult;
            Assert.IsNotNull(conflictResult);
            Assert.AreEqual(409, conflictResult.StatusCode);
        }

        [TestMethod]
        public async Task Delete_ActuallyDeletesPothole()
        {
            // Arrange
            var potholeToDelete = await _potholeService.GetByIdAsync(_testPothole.Id);
            Assert.IsNotNull(potholeToDelete);

            // Act
            await _controller.Delete(_testPothole.Id);

            // Assert
            var deletedPothole = await _potholeService.GetByIdAsync(_testPothole.Id);
            Assert.IsNull(deletedPothole);
        }

        #endregion
    }
}
