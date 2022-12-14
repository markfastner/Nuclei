
namespace Logic.Common.Results
{
    public record UserResult(
        int UserId,
        string FirstName,
        string LastName,
        DateTime Birthday
    );
}
