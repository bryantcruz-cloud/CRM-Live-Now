using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Core.Interfaces.Services;

public interface IUserService
{
    Task<User?> ValidateCredentialsAsync(string userOrEmail, string password, CancellationToken cancellationToken = default);
    Task<User> EnsureBootstrapAdminAsync(string name, string username, string email, string password, CancellationToken cancellationToken = default);
}
