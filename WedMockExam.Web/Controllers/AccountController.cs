using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WedMockExam.Services.Interfaces.Authentication;
using WedMockExam.Services.DTOs.Authentication;
using WedMockExam.Web.Models.ViewModels.Account;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AuthService = WedMockExam.Services.Interfaces.Authentication.IAuthenticationService;
using Microsoft.Extensions.Logging;
using WedMockExam.Services.Implementations.Authentication;

namespace WedMockExam.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly AuthService _authService;

        public AccountController(AuthService authService, ILogger<AccountController> logger) : base(logger)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            try
            {
                // If user is already logged in, redirect to home
                if (GetUserId().HasValue)
                {
                    return RedirectToAction("Index", "Home");
                }

                return View(new LoginViewModel
                {
                    ReturnUrl = returnUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading login page");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var result = await _authService.LoginAsync(new LoginRequest
                {
                    Username = model.Username,
                    Password = model.Password
                });
                if (result.Success)
                {
                    HttpContext.Session.SetInt32("UserId", result.UserInfo.UserId);
                    HttpContext.Session.SetString("UserName", result.UserInfo.Username);

                    if (!string.IsNullOrEmpty(model.ReturnUrl))
                        return Redirect(model.ReturnUrl);

                    return RedirectToAction("Index", "Home");
                }
                ViewData["ErrorMessage"] = result.Message ?? "Invalid username or password";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt");
                ViewData["ErrorMessage"] = "An error occurred during login. Please try again.";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            try
            {
                // If user is already logged in, redirect to home
                if (GetUserId().HasValue)
                {
                    return RedirectToAction("Index", "Home");
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading registration page");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var request = new RegisterRequest
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = model.DateOfBirth
                };

                var result = await _authService.RegisterAsync(request);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Registration successful! Please login.";
                    return RedirectToAction("Login");
                }

                ViewData["ErrorMessage"] = result.Message ?? "Registration failed";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration attempt");
                ViewData["ErrorMessage"] = "An error occurred during registration. Please try again.";
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToAction("Login");
            }
        }
    }
}
