
using Logic.AccountManager.Common;
using Logic.Common.Interfaces.Auth;
using Logic.Common.Interfaces.Persistence;
using Logic.Common.Results;
using MapsterMapper;
using System.Security.Principal;

namespace Logic.Queries.Accounts;

public record GetAccountQuery(
    int AccountId    
) : IRequest<ErrorOr<AccountResult>>;

public class GetAccountQueryHandler : IRequestHandler<GetAccountQuery, ErrorOr<AccountResult>>
{
    private readonly IMapper _mapper;
    private readonly IAccountRepository _accountRepository;
    private readonly IAuthProvider _authProvider;

    public GetAccountQueryHandler(
        IMapper mapper,
        IAccountRepository accountRepository,
        IAuthProvider authProvider
    )
    {
        _mapper = mapper;
        _accountRepository = accountRepository;
        _authProvider = authProvider;
    }

    public async Task<ErrorOr<AccountResult>> Handle(GetAccountQuery query, CancellationToken cancellationToken)
    {
        //only authorize Users that have declared to a principal to this method
        if (_authProvider.GetPrincipal() is not GenericPrincipal authPrincipal) return DomainErrors.Authorization.Unauthorized;
        if (!authPrincipal.IsInRole(AuthorizationRoles.User.ToString())) return DomainErrors.Authorization.Unauthorized;
        if (await _accountRepository.GetAsync(query.AccountId, true) is not Account inquiry) return DomainErrors.Account.NotFound;

        return _mapper.Map<AccountResult>(inquiry);
    }
}