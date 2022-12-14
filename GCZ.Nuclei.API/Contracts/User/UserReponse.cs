
namespace Contracts.User
{
    public record UserResponse(
        int UserId,
        string FirstName,
        string LastName,
        DateTime Birthday
    );
}
