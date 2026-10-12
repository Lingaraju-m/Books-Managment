using API_Practise.Entities;
using API_Practise.Models;

namespace API_Practise.Service
{
    public interface IAuthService
    {
        Task<User> RegisterAync(userDto request);
        Task<string> LoginAsync(userDto request);
    }
}
