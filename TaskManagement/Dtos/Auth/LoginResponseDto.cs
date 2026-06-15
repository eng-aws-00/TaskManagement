namespace TaskManagement.Dtos.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}
