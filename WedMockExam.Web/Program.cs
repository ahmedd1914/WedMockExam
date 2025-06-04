using WedMockExam.Repository;
using WedMockExam.Repository.Interfaces.User;
using WedMockExam.Repository.Implementations.User;
using WedMockExam.Repository.Interfaces.Workplace;
using WedMockExam.Repository.Implementations.Workplace;
using WedMockExam.Repository.Interfaces.Reservation;
using WedMockExam.Repository.Implementations.Reservation;
using WedMockExam.Repository.Interfaces.PreferredLocation;
using WedMockExam.Services.Interfaces.Authentication;
using WedMockExam.Services.Implementations.Authentication;
using WedMockExam.Services.Interfaces.Workplace;
using WedMockExam.Services.Implementations.Workplace;
using WedMockExam.Services.Interfaces.Reservation;
using WedMockExam.Services.Implementations.Reservation;
using WedMockExam.Services.Interfaces.PreferredLocation;
using Microsoft.AspNetCore.Authentication.Cookies;
using WedMockExam.Repository.Implementations.PreferredLocation;
using WedMockExam.Services.Implementations.PreferredLocation;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
ConnectionFactory.Initialize(connectionString);

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWorkplaceRepository, WorkplaceRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IPreferredLocationRepository, PreferredLocationRepository>();

// Register Services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IWorkplaceService, WorkplaceService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPreferredLocationService, PreferredLocationService>();

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "WedMockExam.Auth";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Add Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
