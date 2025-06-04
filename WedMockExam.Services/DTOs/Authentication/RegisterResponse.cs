namespace WedMockExam.Services.DTOs.Authentication
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
