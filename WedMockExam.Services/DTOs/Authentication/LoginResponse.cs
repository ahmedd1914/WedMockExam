namespace WedMockExam.Services.DTOs.Authentication
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserInfo UserInfo { get; set; }
}
}