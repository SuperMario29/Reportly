using System;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
namespace Reportly;

public class AuthService
{
    private readonly string _connectionString;
    private readonly string _jwtSecretKey;

    public AuthService(string connectionString, string jwtSecretKey)
    {
        _connectionString = connectionString;
        _jwtSecretKey = jwtSecretKey;
    }

    public AuthService(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Register a new user
    public bool Register(User user, string password)
    {
        using var connection = new SqlConnection(_connectionString);

        // Check if user exists
        var existingUser = connection.QueryFirstOrDefault<User>(
            "SELECT * FROM Users WHERE Username = @Username OR Email = @Email",
            new { user.Username, user.Email });

        if (existingUser != null) return false;

        // Hash password
        var salt = GenerateSalt();
        var hashedPassword = HashPassword(password, salt);

        // Save user
        connection.Execute(
            @"INSERT INTO Users (Username, PasswordHash, Salt, Email, Role) 
              VALUES (@Username, @PasswordHash, @Salt, @Email, @Role)",
            new
            {
                user.Username,
                PasswordHash = hashedPassword,
                Salt = salt,
                user.Email,
                user.Role
            });

        return true;
    }

    // Login and generate JWT token
    public string Login(string username, string password)
    {
        using var connection = new SqlConnection(_connectionString);
        var user = connection.QueryFirstOrDefault<User>(
            "SELECT * FROM Users WHERE Username = @Username",
            new { Username = username });

        if (user == null) return null;

        // Verify password
        var hashedPassword = HashPassword(password, user.Salt);
        if (hashedPassword != user.PasswordHash) return null;

        // Generate JWT token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // Helper: Generate password hash
    private string HashPassword(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var saltedPassword = password + salt;
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return Convert.ToBase64String(bytes);
    }

    // Helper: Generate random salt
    private string GenerateSalt()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    internal User GetUserByUsername(string username)
    {
         using var connection = new SqlConnection(_connectionString);
        return connection.QueryFirstOrDefault<User>(
            "SELECT * FROM Users WHERE Username = @Username",
            new { Username = username });
    }

    internal bool UpdateUserInfo(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Execute(
                    "UPDATE USERS SET Username = @Username , Email = @Email FROM Users WHERE Id = @Id",
                    new { Username = user.Username , Email = user.Email, Id = user.Id });
        return true; 
    }
    
    internal bool UpdateUserPassword(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Execute(
                    "UPDATE USERS SET PasswordHash = @PasswordHash , Salt = @Salt FROM Users WHERE Id = @Id",
                    new { PasswordHash = user.PasswordHash , Salt = user.Salt, Id = user.Id });
        return true; 
    }

    internal bool UpdateUserRole(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Execute(
                    "UPDATE USERS SET Role = @Role FROM Users WHERE Id = @Id",
                    new { Role = user.Role, Id = user.Id });
        return true; 
    }

    internal bool UpdateUserTwoFactor(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Execute(
                    "UPDATE USERS SET TwoFactorEnabled = @TwoFactorEnabled, TwoFactorSecret = @TwoFactorSecret , PhoneNumber = @Phone FROM Users WHERE Id = @Id",
                    new { TwoFactorEnabled = user.TwoFactorEnabled, TwoFactorSecret = user.TwoFactorSecret, PhoneNumber = user.PhoneNumber, Id = user.Id });
        return true;
    }
    
 
}