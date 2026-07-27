using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BallastLaneApi.Data;
using BallastLaneApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BallastLaneApi.Services
{
    public class PotholeService : IPotholeService
    {
        private readonly ApplicationDbContext _db;

        public PotholeService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Pothole>> GetAllAsync()
        {
            return await _db.Potholes.Include(p => p.User).ToListAsync();
        }

        public async Task<Pothole?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id is required", nameof(id));
            return await _db.Potholes.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Pothole> CreateAsync(Pothole pothole)
        {
            if (pothole == null) throw new ArgumentNullException(nameof(pothole));
            if (pothole.Latitude < -90 || pothole.Latitude > 90) throw new ArgumentException("Latitude must be between -90 and 90.");
            if (pothole.Longitude < -180 || pothole.Longitude > 180) throw new ArgumentException("Longitude must be between -180 and 180.");
            if (string.IsNullOrWhiteSpace(pothole.UserId)) throw new ArgumentException("UserId is required.");

            var user = await _db.Users.FindAsync(pothole.UserId);
            if (user == null) throw new InvalidOperationException("User not found for UserId.");

            pothole.Id = string.IsNullOrWhiteSpace(pothole.Id) ? Guid.NewGuid().ToString() : pothole.Id;
            pothole.CreatedDate = DateTime.UtcNow;

            _db.Potholes.Add(pothole);
            await _db.SaveChangesAsync();
            return pothole;
        }

        public async Task UpdateAsync(Pothole pothole)
        {
            if (pothole == null) throw new ArgumentNullException(nameof(pothole));
            if (string.IsNullOrWhiteSpace(pothole.Id)) throw new ArgumentException("Id is required.");

            var existing = await _db.Potholes.FindAsync(pothole.Id);
            if (existing == null) throw new InvalidOperationException("Pothole not found.");

            if (pothole.Latitude < -90 || pothole.Latitude > 90) throw new ArgumentException("Latitude must be between -90 and 90.");
            if (pothole.Longitude < -180 || pothole.Longitude > 180) throw new ArgumentException("Longitude must be between -180 and 180.");

            existing.Description = pothole.Description;
            existing.Latitude = pothole.Latitude;
            existing.Longitude = pothole.Longitude;
            existing.Status = pothole.Status;

            _db.Potholes.Update(existing);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id is required", nameof(id));
            var existing = await _db.Potholes.FindAsync(id);
            if (existing == null) throw new InvalidOperationException("Pothole not found.");
            _db.Potholes.Remove(existing);
            await _db.SaveChangesAsync();
        }
    }
}
