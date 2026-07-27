using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BallastLaneApi.Data;
using BallastLaneApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BallastLaneApi.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;

        public UserService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _db.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id is required", nameof(id));
            return await _db.Users.FindAsync(id);
        }

        public async Task<User> CreateAsync(User user, string password)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(user.Email)) throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8) throw new ArgumentException("Password must be at least 8 characters.");

            // simple email validation
            if (!Regex.IsMatch(user.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) throw new ArgumentException("Invalid email address.");

            var existing = await _db.Users.AnyAsync(u => u.Email == user.Email);
            if (existing) throw new InvalidOperationException("A user with that email already exists.");

            // generate salt and hash
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, System.Security.Cryptography.HashAlgorithmName.SHA256, 32);

            user.Id = string.IsNullOrWhiteSpace(user.Id) ? Guid.NewGuid().ToString() : user.Id;
            user.PasswordSalt = salt;
            user.PasswordHash = hash;
            user.CreatedDate = DateTime.UtcNow;

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return user;
        }

        public async Task UpdateAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(user.Id)) throw new ArgumentException("Id is required.");

            var existing = await _db.Users.FindAsync(user.Id);
            if (existing == null) throw new InvalidOperationException("User not found.");

            // update allowed fields
            existing.DisplayName = user.DisplayName;
            existing.Email = user.Email;
            // do not overwrite password here

            _db.Users.Update(existing);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id is required", nameof(id));
            var existing = await _db.Users.FindAsync(id);
            if (existing == null) throw new InvalidOperationException("User not found.");
            _db.Users.Remove(existing);
            await _db.SaveChangesAsync();
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return null;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            var computed = Rfc2898DeriveBytes.Pbkdf2(password, user.PasswordSalt, 100_000, System.Security.Cryptography.HashAlgorithmName.SHA256, 32);
            if (!CryptographicOperations.FixedTimeEquals(computed, user.PasswordHash)) return null;
            return user;
        }
    }
}
