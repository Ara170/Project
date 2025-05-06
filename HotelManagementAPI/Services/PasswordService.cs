using System.Security.Cryptography;
using System.Text;

namespace HotelManagementAPI.Services
{
    public class PasswordService : IPasswordService
    {
        public (byte[] passwordHash, string salt) HashPassword(string password)
        {
            // Generate a random salt
            var saltBytes = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            
            string salt = Convert.ToBase64String(saltBytes);
            
            // Hash the password with the salt
            using (var sha256 = SHA256.Create())
            {
                var passwordWithSalt = password + salt;
                var passwordBytes = Encoding.UTF8.GetBytes(passwordWithSalt);
                var hashedBytes = sha256.ComputeHash(passwordBytes);
                
                return (hashedBytes, salt);
            }
        }

        public bool VerifyPassword(string password, byte[] storedHash, string salt)
        {
            // Hash the input password with the stored salt
            using (var sha256 = SHA256.Create())
            {
                var passwordWithSalt = password + salt;
                var passwordBytes = Encoding.UTF8.GetBytes(passwordWithSalt);
                var hashedBytes = sha256.ComputeHash(passwordBytes);
                
                // Compare the computed hash with the stored hash
                return CompareByteArrays(hashedBytes, storedHash);
            }
        }

        private bool CompareByteArrays(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                    return false;
            }

            return true;
        }
    }
}
