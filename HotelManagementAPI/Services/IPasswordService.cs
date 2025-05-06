namespace HotelManagementAPI.Services
{
    public interface IPasswordService
    {
        (byte[] passwordHash, string salt) HashPassword(string password);
        bool VerifyPassword(string password, byte[] storedHash, string salt);
    }
}
