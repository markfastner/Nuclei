
using Logic.AccountManager.Common;
using Logic.Common.Interfaces.Auth;
using Logic.Common.Interfaces.Persistence;
using MapsterMapper;
using System.Security.Principal;

namespace Logic.Commands.Users;

public record UpdateUserCommand(
    int UserId,
    string FirstName,
    string LastName,
    DateTime Birthday
) : IRequest<ErrorOr<bool>>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ErrorOr<bool>>
{
    private readonly IAuthProvider _authProvider;
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(
        IAuthProvider authProvider,
        IUserRepository userRepository
    )
    {
        _authProvider = authProvider;
        _userRepository = userRepository;
    }

    public async Task<ErrorOr<bool>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        //only authorize Users that have declared to a principal to this method
        if (_authProvider.GetPrincipal() is not GenericPrincipal authPrincipal) return DomainErrors.Authorization.Unauthorized;
        if (!authPrincipal.IsInRole(AuthorizationRoles.User.ToString())) return DomainErrors.Authorization.Unauthorized;

        if (await _userRepository.GetAsync(command.UserId) is not User user) return DomainErrors.User.NotFound;

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.Birthday = command.Birthday;

        if (!await _userRepository.UpdateAsync(user)) return DomainErrors.User.Failure;

        return true;
    }
} 