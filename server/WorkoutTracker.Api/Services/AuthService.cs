
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkoutTracker.Api.Data;
using WorkoutTracker.Api.Models;

namespace WorkoutTracker.Api.Services;

public class AuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public AuthService(
        ApplicationDbContext db,
        IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<AppUser> RegisterAsync(
        string username,
        string password)
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(x => x.Username == normalizedUsername))
            throw new InvalidOperationException("User already exists");

        var user = new AppUser
        {
            Username = normalizedUsername,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return user;
    }

    public async Task<string?> LoginAsync(
        string username,
        string password)
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(
            x => x.Username == normalizedUsername);

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is missing");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
