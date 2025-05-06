using HotelManagementAPI.Models;

namespace HotelManagementAPI.Services
{
    public interface IJwtService
    {
        string GenerateJwtToken(User user, string roleName);
    }
}
