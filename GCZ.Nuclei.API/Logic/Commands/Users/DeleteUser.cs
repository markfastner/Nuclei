
using Logic.Common.Interfaces.Persistence;

namespace Logic.Commands.Users;

public record DeleteUserCommand(
    int UserId
) : IRequest<ErrorOr<bool>>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ErrorOr<bool>>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(
        IUserRepository userRepository
    )
    {
        _userRepository = userRepository;
    }

    public async Task<ErrorOr<bool>> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        if (await _userRepository.GetAsync(command.UserId) is not User inquiry) return DomainErrors.User.NotFound;
        if (!await _userRepository.DeleteAsync(inquiry)) return DomainErrors.User.Failure;

        return true;
    }
}