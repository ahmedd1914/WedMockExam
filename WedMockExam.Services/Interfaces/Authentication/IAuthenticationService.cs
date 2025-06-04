using WedMockExam.Services.DTOs.Authentication;

namespace WedMockExam.Services.Interfaces.Authentication
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticates a user and returns their information
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Authentication response with user info</returns>
        Task<LoginResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="request">Registration information</param>
        /// <returns>True if registration was successful</returns>
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    }
}
