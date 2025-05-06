namespace HotelManagementAPI.DTOs
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
