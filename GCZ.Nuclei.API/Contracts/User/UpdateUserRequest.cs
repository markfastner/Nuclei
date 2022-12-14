namespace Contracts.User
{
    public record UpdateUserRequest(
        int UserId,
        string FirstName,
        string LastName,
        DateTime Birthday
    );
}
