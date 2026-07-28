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
    public class UserServiceTest
    {
        private ApplicationDbContext _dbContext;
        private UserService _userService;

        [TestInitialize]
        public void Setup()
        {
            // Use EF Core InMemory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _userService = new UserService(_dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext?.Dispose();
        }

        #region GetAllAsync Tests

        [TestMethod]
        public async Task GetAllAsync_ReturnsAllUsers()
        {
            // Arrange
            var user1 = new User { Id = "1", Email = "user1@example.com", DisplayName = "User One", PasswordHash = new byte[32], PasswordSalt = new byte[16], CreatedDate = DateTime.UtcNow };
            var user2 = new User { Id = "2", Email = "user2@example.com", DisplayName = "User Two", PasswordHash = new byte[32], PasswordSalt = new byte[16], CreatedDate = DateTime.UtcNow };

            _dbContext.Users.Add(user1);
            _dbContext.Users.Add(user2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userService.GetAllAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("user1@example.com", result[0].Email);
            Assert.AreEqual("user2@example.com", result[1].Email);
        }

        [TestMethod]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoUsersExist()
        {
            // Act
            var result = await _userService.GetAllAsync();

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
                await _userService.GetByIdAsync(null);
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
                await _userService.GetByIdAsync("");
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
                await _userService.GetByIdAsync("   ");
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = "1", Email = "user@example.com", DisplayName = "Test User", PasswordHash = new byte[32], PasswordSalt = new byte[16], CreatedDate = DateTime.UtcNow };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userService.GetByIdAsync("1");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("user@example.com", result.Email);
        }

        [TestMethod]
        public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Act
            var result = await _userService.GetByIdAsync("999");

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region CreateAsync Tests

        [TestMethod]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenUserIsNull()
        {
            // Act & Assert
            try
            {
                await _userService.CreateAsync(null, "password123");
                Assert.Fail("Expected ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task CreateAsync_ThrowsArgumentException_WhenEmailIsNull()
        {
            // Arrange
            var user = new User { Email = null };

            // Act & Assert
            try
            {
                await _userService.CreateAsync(user, "password123");
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task CreateAsync_ThrowsArgumentException_WhenEmailIsEmpty()
        {
            // Arrange
            var user = new User { Email = "" };

            // Act & Assert
            try
            {
                await _userService.CreateAsync(user, "password123");
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task CreateAsync_ThrowsArgumentException_WhenPasswordIsNull()
        {
            // Arrange
            var user = new User { Email = "user@example.com" };

            // Act & Assert
            try
            {
                await _userService.CreateAsync(user, null);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task CreateAsync_ThrowsArgumentException_WhenPasswordTooShort()
        {
            // Arrange
            var user = new User { Email = "user@example.com" };

            // Act & Assert
            try
            {
                await _userService.CreateAsync(user, "short");
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task CreateAsync_ThrowsArgumentException_WhenEmailInvalid()
        {
            // Arrange
            var user = new User { Email = "invalid-email" };

            // Act & Assert
            try
            {
                await _userService.CreateAsync(user, "password123");
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task CreateAsync_ThrowsInvalidOperationException_WhenEmailAlreadyExists()
        {
            // Arrange
            var existingUser = new User { Id = "existing", Email = "existing@example.com", PasswordHash = new byte[32], PasswordSalt = new byte[16], CreatedDate = DateTime.UtcNow };
            _dbContext.Users.Add(existingUser);
            await _dbContext.SaveChangesAsync();

            var newUser = new User { Email = "existing@example.com" };

            // Act & Assert
            try
            {
                await _userService.CreateAsync(newUser, "password123");
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task CreateAsync_CreatesUser_WithValidInput()
        {
            // Arrange
            var user = new User { Email = "newuser@example.com", DisplayName = "New User" };

            // Act
            var result = await _userService.CreateAsync(user, "password123");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Id);
            Assert.AreEqual("newuser@example.com", result.Email);
            Assert.IsNotNull(result.PasswordHash);
            Assert.IsNotNull(result.PasswordSalt);
            Assert.AreEqual(32, result.PasswordHash.Length);
            Assert.AreEqual(16, result.PasswordSalt.Length);
            Assert.IsNotNull(result.CreatedDate);

            // Verify it was actually saved
            var savedUser = await _dbContext.Users.FindAsync(result.Id);
            Assert.IsNotNull(savedUser);
            Assert.AreEqual("newuser@example.com", savedUser.Email);
        }

        [TestMethod]
        public async Task CreateAsync_GeneratesIdIfNotProvided()
        {
            // Arrange
            var user = new User { Email = "newuser@example.com", Id = null };

            // Act
            var result = await _userService.CreateAsync(user, "password123");

            // Assert
            Assert.IsNotNull(result.Id);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Id));
        }

        [TestMethod]
        public async Task CreateAsync_UsesProvidedIdIfNotEmpty()
        {
            // Arrange
            var providedId = "custom-id-123";
            var user = new User { Id = providedId, Email = "newuser@example.com" };

            // Act
            var result = await _userService.CreateAsync(user, "password123");

            // Assert
            Assert.AreEqual(providedId, result.Id);
        }

        #endregion

        #region UpdateAsync Tests

        [TestMethod]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenUserIsNull()
        {
            // Act & Assert
            try
            {
                await _userService.UpdateAsync(null);
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
            var user = new User { Id = null, Email = "user@example.com" };

            // Act & Assert
            try
            {
                await _userService.UpdateAsync(user);
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
            var user = new User { Id = "", Email = "user@example.com" };

            // Act & Assert
            try
            {
                await _userService.UpdateAsync(user);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenUserNotFound()
        {
            // Arrange
            var user = new User { Id = "999", Email = "user@example.com" };

            // Act & Assert
            try
            {
                await _userService.UpdateAsync(user);
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task UpdateAsync_UpdatesUserFields_WhenUserExists()
        {
            // Arrange
            var existingUser = new User { Id = "1", Email = "old@example.com", DisplayName = "Old Name", PasswordHash = new byte[32], PasswordSalt = new byte[16], CreatedDate = DateTime.UtcNow };
            _dbContext.Users.Add(existingUser);
            await _dbContext.SaveChangesAsync();

            var updateUser = new User { Id = "1", Email = "new@example.com", DisplayName = "New Name" };

            // Act
            await _userService.UpdateAsync(updateUser);

            // Assert
            var updated = await _dbContext.Users.FindAsync("1");
            Assert.AreEqual("new@example.com", updated.Email);
            Assert.AreEqual("New Name", updated.DisplayName);
        }

        #endregion

        #region DeleteAsync Tests

        [TestMethod]
        public async Task DeleteAsync_ThrowsArgumentException_WhenIdIsNull()
        {
            // Act & Assert
            try
            {
                await _userService.DeleteAsync(null);
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
                await _userService.DeleteAsync("");
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
                await _userService.DeleteAsync("   ");
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task DeleteAsync_ThrowsInvalidOperationException_WhenUserNotFound()
        {
            // Act & Assert
            try
            {
                await _userService.DeleteAsync("999");
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task DeleteAsync_DeletesUser_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = "1", Email = "user@example.com", PasswordHash = new byte[32], PasswordSalt = new byte[16], CreatedDate = DateTime.UtcNow };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            await _userService.DeleteAsync("1");

            // Assert
            var deleted = await _dbContext.Users.FindAsync("1");
            Assert.IsNull(deleted);
        }

        #endregion

        #region AuthenticateAsync Tests

        [TestMethod]
        public async Task AuthenticateAsync_ReturnsNull_WhenEmailIsNull()
        {
            // Act
            var result = await _userService.AuthenticateAsync(null, "password123");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task AuthenticateAsync_ReturnsNull_WhenPasswordIsNull()
        {
            // Act
            var result = await _userService.AuthenticateAsync("user@example.com", null);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task AuthenticateAsync_ReturnsNull_WhenEmailIsEmpty()
        {
            // Act
            var result = await _userService.AuthenticateAsync("", "password123");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task AuthenticateAsync_ReturnsNull_WhenPasswordIsEmpty()
        {
            // Act
            var result = await _userService.AuthenticateAsync("user@example.com", "");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task AuthenticateAsync_ReturnsNull_WhenUserNotFound()
        {
            // Act
            var result = await _userService.AuthenticateAsync("nonexistent@example.com", "password123");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task AuthenticateAsync_ReturnsNull_WhenPasswordIsIncorrect()
        {
            // Arrange
            var salt = new byte[16];
            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            var correctHash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
                "correctpassword", salt, 100_000, System.Security.Cryptography.HashAlgorithmName.SHA256, 32);

            var user = new User
            {
                Id = "1",
                Email = "user@example.com",
                PasswordHash = correctHash,
                PasswordSalt = salt,
                CreatedDate = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userService.AuthenticateAsync("user@example.com", "wrongpassword");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task AuthenticateAsync_ReturnsUser_WhenCredentialsAreCorrect()
        {
            // Arrange
            var salt = new byte[16];
            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            var correctPassword = "correctpassword";
            var correctHash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
                correctPassword, salt, 100_000, System.Security.Cryptography.HashAlgorithmName.SHA256, 32);

            var user = new User
            {
                Id = "1",
                Email = "user@example.com",
                PasswordHash = correctHash,
                PasswordSalt = salt,
                DisplayName = "Test User",
                CreatedDate = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userService.AuthenticateAsync("user@example.com", correctPassword);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("user@example.com", result.Email);
            Assert.AreEqual("Test User", result.DisplayName);
        }

        #endregion
    }
}
