using System.Data.SqlTypes;
using WedMockExam.Services.Interfaces.Authentication;
using WedMockExam.Services.DTOs.Authentication;
using WedMockExam.Repository.Interfaces.User;
using WedMockExam.Models;
using WedMockExam.Services.Helpers;

namespace WedMockExam.Services.Implementations.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;

        public AuthenticationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Username and password are required"
                };
            }

            var hashedPassword = SecurityHelper.HashPassword(request.Password);
            var filter = new UserFilter
            {
                Username = new SqlString(request.Username)
            };

            var users = _userRepository.RetrieveCollectionAsync(filter);
            User? user = null;
            await foreach (var u in users)
            {
                user = u;
                break;
            }

            if (user == null || user.PasswordHash != hashedPassword)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            return new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                UserInfo = new UserInfo
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email
                }
            };
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var filter = new UserFilter
            {
                Username = new SqlString(request.Username)
            };

            var existingUsers = _userRepository.RetrieveCollectionAsync(filter);
            var exists = false;
            await foreach (var _ in existingUsers)
            {
                exists = true;
                break;
            }

            if (exists)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Username already exists"
                };
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = SecurityHelper.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userRepository.CreateAsync(user);
            return new RegisterResponse
            {
                Success = result > 0,
                Message = result > 0 ? "Registration successful" : "Registration failed",
                UserInfo = new UserInfo
                {
                    UserId = result,
                    Username = user.Username,
                    Email = user.Email
                }
            };
        }
    }
}
