
using Logic.AccountManager.Common;
using Logic.Common.Interfaces.Auth;
using Logic.Common.Interfaces.Persistence;
using System.Security.Principal;

namespace Logic.Commands.Accounts;

public record DeleteAccountCommand(
    int AccountId
) : IRequest<ErrorOr<bool>>;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, ErrorOr<bool>>
{
    private readonly IAuthProvider _authProvider;
    private readonly IAccountRepository _accountRepository;

    public DeleteAccountCommandHandler(
        IAuthProvider authProvider,
        IAccountRepository accountRepository
    )
    {
        _authProvider = authProvider;
        _accountRepository = accountRepository;
    }

    public async Task<ErrorOr<bool>> Handle(DeleteAccountCommand command, CancellationToken cancellationToken)
    {
        //only authorize Admins that have declared to a principal to this method
        if (_authProvider.GetPrincipal() is not GenericPrincipal authPrincipal) return DomainErrors.Authorization.Unauthorized;
        if (!authPrincipal.IsInRole(AuthorizationRoles.Admin.ToString())) return DomainErrors.Authorization.Unauthorized;

        if (await _accountRepository.GetAsync(command.AccountId) is not Account inquiry) return DomainErrors.Account.NotFound;
        if (!await _accountRepository.DeleteAsync(inquiry)) return DomainErrors.Account.Failure;

        return true;
    }
}