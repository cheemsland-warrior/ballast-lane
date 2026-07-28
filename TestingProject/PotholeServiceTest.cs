using System;
using System.Threading.Tasks;
using BallastLaneApi.Data;
using BallastLaneApi.Models;
using BallastLaneApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestingProject
{
    [TestClass]
    public class PotholeServiceTest
    {
        private ApplicationDbContext _dbContext;
        private PotholeService _potholeService;
        private UserService _userService;

        [TestInitialize]
        public void Setup()
        {
            // Use EF Core InMemory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _potholeService = new PotholeService(_dbContext);
            _userService = new UserService(_dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext?.Dispose();
        }

        #region Helper Methods

        private async Task<User> CreateTestUserAsync(string email = "testuser@example.com")
        {
            var user = new User { Email = email, DisplayName = "Test User" };
            return await _userService.CreateAsync(user, "password123");
        }

        #endregion

        #region GetAllAsync Tests

        [TestMethod]
        public async Task GetAllAsync_ReturnsAllPotholes()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var pothole1 = new Pothole { Id = "1", Description = "Deep hole", Latitude = 40.7128, Longitude = -74.0060, UserId = user.Id, Status = "Reported", CreatedDate = DateTime.UtcNow };
            var pothole2 = new Pothole { Id = "2", Description = "Large pothole", Latitude = 34.0522, Longitude = -118.2437, UserId = user.Id, Status = "In Progress", CreatedDate = DateTime.UtcNow };

            _dbContext.Potholes.Add(pothole1);
            _dbContext.Potholes.Add(pothole2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _potholeService.GetAllAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoPotholesExist()
        {
            // Act
            var result = await _potholeService.GetAllAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region GetByIdAsync Tests

        [TestMethod]
        public async Task GetByIdAsync_ThrowsArgumentException_WhenIdIsNull()
        {
            // Act & Assert
            try
            {
                await _potholeService.GetByIdAsync(null);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_ThrowsArgumentException_WhenIdIsEmpty()
        {
            // Act & Assert
            try
            {
                await _potholeService.GetByIdAsync("");
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_ThrowsArgumentException_WhenIdIsWhitespace()
        {
            // Act & Assert
            try
            {
                await _potholeService.GetByIdAsync("   ");
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_ReturnsPothole_WhenPotholeExists()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var pothole = new Pothole { Id = "1", Description = "Test pothole", Latitude = 40.7128, Longitude = -74.0060, UserId = user.Id, CreatedDate = DateTime.UtcNow };
            _dbContext.Potholes.Add(pothole);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _potholeService.GetByIdAsync("1");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("Test pothole", result.Description);
        }

        [TestMethod]
        public async Task GetByIdAsync_ReturnsNull_WhenPotholeDoesNotExist()
        {
            // Act
            var result = await _potholeService.GetByIdAsync("999");

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region CreateAsync Tests

        [TestMethod]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenPotholeIsNull()
        {
            // Act & Assert
            try
            {
                await _potholeService.CreateAsync(null);
                Assert.Fail("Expected ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task CreateAsync_ThrowsArgumentException_WhenLatitudeTooLow()
        {
            // Arrange
            var pothole = new Pothole { Latitude = -91, Longitude = 0, UserId = "user1" };

            // Act & Assert
            try
            {
                await _potholeService.CreateAsync(pothole);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Latitude"));
            }
        }

        [TestMethod]
        public async Task CreateAsync_ThrowsArgumentException_WhenLatitudeTooHigh()
        {
            // Arrange
            var pothole = new Pothole { Latitude = 91, Longitude = 0, UserId = "user1" };

            // Act & Assert
            try
            {
                await _potholeService.CreateAsync(pothole);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Latitude"));
            }
        }

        [TestMethod]
        public async Task CreateAsync_ThrowsArgumentException_WhenLongitudeTooLow()
        {
            // Arrange
            var pothole = new Pothole { Latitude = 0, Longitude = -181, UserId = "user1" };

            // Act & Assert
            try
            {
                await _potholeService.CreateAsync(pothole);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Longitude"));
            }
        }

        [TestMethod]
        public async Task CreateAsync_ThrowsArgumentException_WhenLongitudeTooHigh()
        {
            // Arrange
            var pothole = new Pothole { Latitude = 0, Longitude = 181, UserId = "user1" };

            // Act & Assert
            try
            {
                await _potholeService.CreateAsync(pothole);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Longitude"));
            }
        }

        [TestMethod]
        public async Task CreateAsync_ThrowsArgumentException_WhenUserIdIsNull()
        {
            // Arrange
            var pothole = new Pothole { Latitude = 40.7128, Longitude = -74.0060, UserId = null };

            // Act & Assert
            try
            {
                await _potholeService.CreateAsync(pothole);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task CreateAsync_ThrowsArgumentException_WhenUserIdIsEmpty()
        {
            // Arrange
            var pothole = new Pothole { Latitude = 40.7128, Longitude = -74.0060, UserId = "" };

            // Act & Assert
            try
            {
                await _potholeService.CreateAsync(pothole);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task CreateAsync_ThrowsInvalidOperationException_WhenUserNotFound()
        {
            // Arrange
            var pothole = new Pothole { Latitude = 40.7128, Longitude = -74.0060, UserId = "nonexistent-user" };

            // Act & Assert
            try
            {
                await _potholeService.CreateAsync(pothole);
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("User not found"));
            }
        }

        [TestMethod]
        public async Task CreateAsync_CreatesPothole_WithValidInput()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var pothole = new Pothole { Description = "Deep pothole", Latitude = 40.7128, Longitude = -74.0060, UserId = user.Id };

            // Act
            var result = await _potholeService.CreateAsync(pothole);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Id);
            Assert.AreEqual("Deep pothole", result.Description);
            Assert.AreEqual(40.7128, result.Latitude);
            Assert.AreEqual(-74.0060, result.Longitude);
            Assert.IsNotNull(result.CreatedDate);

            // Verify it was saved
            var saved = await _dbContext.Potholes.FindAsync(result.Id);
            Assert.IsNotNull(saved);
        }

        [TestMethod]
        public async Task CreateAsync_SetsDefaultStatus()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var pothole = new Pothole { Description = "Test", Latitude = 40.7128, Longitude = -74.0060, UserId = user.Id };

            // Act
            var result = await _potholeService.CreateAsync(pothole);

            // Assert
            Assert.AreEqual("Reported", result.Status);
        }

        [TestMethod]
        public async Task CreateAsync_GeneratesUniqueIds()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var pothole1 = new Pothole { Description = "Hole 1", Latitude = 40.7128, Longitude = -74.0060, UserId = user.Id };
            var pothole2 = new Pothole { Description = "Hole 2", Latitude = 34.0522, Longitude = -118.2437, UserId = user.Id };

            // Act
            var result1 = await _potholeService.CreateAsync(pothole1);
            var result2 = await _potholeService.CreateAsync(pothole2);

            // Assert
            Assert.AreNotEqual(result1.Id, result2.Id);
        }

        #endregion

        #region UpdateAsync Tests

        [TestMethod]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenPotholeIsNull()
        {
            // Act & Assert
            try
            {
                await _potholeService.UpdateAsync(null);
                Assert.Fail("Expected ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task UpdateAsync_ThrowsArgumentException_WhenIdIsNull()
        {
            // Arrange
            var pothole = new Pothole { Id = null, Latitude = 40.7128, Longitude = -74.0060 };

            // Act & Assert
            try
            {
                await _potholeService.UpdateAsync(pothole);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task UpdateAsync_ThrowsArgumentException_WhenIdIsEmpty()
        {
            // Arrange
            var pothole = new Pothole { Id = "", Latitude = 40.7128, Longitude = -74.0060 };

            // Act & Assert
            try
            {
                await _potholeService.UpdateAsync(pothole);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenPotholeNotFound()
        {
            // Arrange
            var pothole = new Pothole { Id = "999", Latitude = 40.7128, Longitude = -74.0060 };

            // Act & Assert
            try
            {
                await _potholeService.UpdateAsync(pothole);
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("not found"));
            }
        }

        [TestMethod]
        public async Task UpdateAsync_ThrowsArgumentException_WhenLatitudeInvalid()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var pothole = new Pothole { Id = "1", Description = "Test", Latitude = 40.7128, Longitude = -74.0060, UserId = user.Id, CreatedDate = DateTime.UtcNow };
            _dbContext.Potholes.Add(pothole);
            await _dbContext.SaveChangesAsync();

            var updatePothole = new Pothole { Id = "1", Latitude = 91, Longitude = -74.0060 };

            // Act & Assert
            try
            {
                await _potholeService.UpdateAsync(updatePothole);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Latitude"));
            }
        }

        [TestMethod]
        public async Task UpdateAsync_ThrowsArgumentException_WhenLongitudeInvalid()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var pothole = new Pothole { Id = "1", Description = "Test", Latitude = 40.7128, Longitude = -74.0060, UserId = user.Id, CreatedDate = DateTime.UtcNow };
            _dbContext.Potholes.Add(pothole);
            await _dbContext.SaveChangesAsync();

            var updatePothole = new Pothole { Id = "1", Latitude = 40.7128, Longitude = 181 };

            // Act & Assert
            try
            {
                await _potholeService.UpdateAsync(updatePothole);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Longitude"));
            }
        }

        [TestMethod]
        public async Task UpdateAsync_UpdatesPotholeFields_WhenPotholeExists()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var pothole = new Pothole { Id = "1", Description = "Old description", Latitude = 40.7128, Longitude = -74.0060, UserId = user.Id, Status = "Reported", CreatedDate = DateTime.UtcNow };
            _dbContext.Potholes.Add(pothole);
            await _dbContext.SaveChangesAsync();

            var updatePothole = new Pothole { Id = "1", Description = "New description", Latitude = 40.7500, Longitude = -74.0100, Status = "In Progress" };

            // Act
            await _potholeService.UpdateAsync(updatePothole);

            // Assert
            var updated = await _dbContext.Potholes.FindAsync("1");
            Assert.AreEqual("New description", updated.Description);
            Assert.AreEqual(40.7500, updated.Latitude);
            Assert.AreEqual(-74.0100, updated.Longitude);
            Assert.AreEqual("In Progress", updated.Status);
        }

        #endregion

        #region DeleteAsync Tests

        [TestMethod]
        public async Task DeleteAsync_ThrowsArgumentException_WhenIdIsNull()
        {
            // Act & Assert
            try
            {
                await _potholeService.DeleteAsync(null);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task DeleteAsync_ThrowsArgumentException_WhenIdIsEmpty()
        {
            // Act & Assert
            try
            {
                await _potholeService.DeleteAsync("");
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task DeleteAsync_ThrowsArgumentException_WhenIdIsWhitespace()
        {
            // Act & Assert
            try
            {
                await _potholeService.DeleteAsync("   ");
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task DeleteAsync_ThrowsInvalidOperationException_WhenPotholeNotFound()
        {
            // Act & Assert
            try
            {
                await _potholeService.DeleteAsync("999");
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("not found"));
            }
        }

        [TestMethod]
        public async Task DeleteAsync_DeletesPothole_WhenPotholeExists()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var pothole = new Pothole { Id = "1", Description = "Test", Latitude = 40.7128, Longitude = -74.0060, UserId = user.Id, CreatedDate = DateTime.UtcNow };
            _dbContext.Potholes.Add(pothole);
            await _dbContext.SaveChangesAsync();

            // Act
            await _potholeService.DeleteAsync("1");

            // Assert
            var deleted = await _dbContext.Potholes.FindAsync("1");
            Assert.IsNull(deleted);
        }

        #endregion
    }
}
