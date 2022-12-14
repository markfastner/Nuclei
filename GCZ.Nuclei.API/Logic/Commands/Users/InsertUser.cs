
using Logic.Common.Interfaces.Persistence;

namespace Logic.Commands.Users;

public record InsertUserCommand(
    string FirstName,
    string LastName,
    DateTime Birthday
) : IRequest<ErrorOr<bool>>;

public class InsertUserCommandHandler : IRequestHandler<InsertUserCommand, ErrorOr<bool>>
{
    private readonly IUserRepository _userRepository;

    public InsertUserCommandHandler(
        IUserRepository userRepository
    )
    {
        _userRepository = userRepository;
    }

    public async Task<ErrorOr<bool>> Handle(InsertUserCommand command, CancellationToken cancellationToken)
    {
        var user = new User()
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Birthday = command.Birthday
        };

        if (!await _userRepository.InsertAsync(user)) return DomainErrors.User.Failure;

        return true;
    }
}