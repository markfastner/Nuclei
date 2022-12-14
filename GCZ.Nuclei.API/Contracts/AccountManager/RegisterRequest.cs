using Contracts.User;

namespace Contracts.AccountManager
{
    public record RegisterRequest(
        InsertUserRequest User,
        string Email,
        string Password
    );
}
