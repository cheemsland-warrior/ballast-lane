using System.Collections.Generic;
using System.Threading.Tasks;
using BallastLaneApi.Models;

namespace BallastLaneApi.Services
{
    public interface IPotholeService
    {
        Task<List<Pothole>> GetAllAsync();
        Task<Pothole?> GetByIdAsync(string id);
        Task<Pothole> CreateAsync(Pothole pothole);
        Task UpdateAsync(Pothole pothole);
        Task DeleteAsync(string id);
    }
}
