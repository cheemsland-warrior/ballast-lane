using System.Collections.Generic;
using System.Threading.Tasks;
using BallastLaneApi.Models;

namespace BallastLaneApi.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(string id);
        Task<User> CreateAsync(User user, string password);
        Task UpdateAsync(User user);
        Task DeleteAsync(string id);
        Task<User?> AuthenticateAsync(string email, string password);
    }
}
