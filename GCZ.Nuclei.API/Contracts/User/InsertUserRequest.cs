namespace Contracts.User
{
    public record InsertUserRequest(
        string FirstName,
        string LastName,
        DateTime Birthday
    );
}
