using Contracts.User;

namespace Contracts.Account
{
    public record AccountResponse(
        int AccountId,
        string Email,
        UserResponse User
    );
}
