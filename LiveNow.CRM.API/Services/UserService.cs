using System.Security.Cryptography;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.API.Services;

public sealed class UserService : IUserService
{
    private const int Iterations = 210_000;
    private readonly LiveNowDbContext _context;

    public UserService(LiveNowDbContext context) => _context = context;

    public async Task<User?> ValidateCredentialsAsync(string userOrEmail, string password, CancellationToken cancellationToken = default)
    {
        User? user = await _context.Users.SingleOrDefaultAsync(
            item => item.IsActive && (item.Email == userOrEmail || item.Username == userOrEmail), cancellationToken);

        return user is not null && VerifyPassword(password, user.PasswordHash) ? user : null;
    }

    public async Task<User> EnsureBootstrapAdminAsync(string name, string username, string email, string password, CancellationToken cancellationToken = default)
    {
        User? existing = await _context.Users.SingleOrDefaultAsync(item => item.Email == email, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        User user = new()
        {
            Name = name,
            Username = username,
            Email = email,
            PasswordHash = HashPassword(password),
            Role = "Admin"
        };
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return $"PBKDF2-SHA256:{Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string encoded)
    {
        string[] parts = encoded.Split(':');
        if (parts.Length != 4 || parts[0] != "PBKDF2-SHA256" || !int.TryParse(parts[1], out int iterations))
        {
            return false;
        }
        try
        {
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expected = Convert.FromBase64String(parts[3]);
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
