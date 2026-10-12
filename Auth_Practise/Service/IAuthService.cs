using Auth_Practise.Entities;
using Auth_Practise.Models;

namespace Auth_Practise.Service
{
    public interface IAuthService
    {
        Task<User> RegisterAync(UserDTO request);
        Task<string> LoginAsync(UserDTO request);

        Task<List<AllUsersDto>> GetAllUsersAsync();
    }
}
