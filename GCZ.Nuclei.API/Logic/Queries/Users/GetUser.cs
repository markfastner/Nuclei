
using Logic.AccountManager.Common;
using Logic.Common.Interfaces.Auth;
using Logic.Common.Interfaces.Persistence;
using Logic.Common.Results;
using System.Security.Principal;

namespace Logic.Queries.Users;

public record GetUserQuery(
    int UserId    
) : IRequest<ErrorOr<UserResult>>;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, ErrorOr<UserResult>>
{
    private readonly IMapper _mapper;
    private readonly IAuthProvider _authProvider;
    private readonly IUserRepository _userRepository;

    public GetUserQueryHandler(
        IMapper mapper,
        IAuthProvider authProvider,
        IUserRepository userRepository
    )
    {
        _mapper = mapper;
        _authProvider = authProvider;
        _userRepository = userRepository;
    }

    public async Task<ErrorOr<UserResult>> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        //only authorize Users that have declared to a principal to this method
        if (_authProvider.GetPrincipal() is not GenericPrincipal authPrincipal) return DomainErrors.Authorization.Unauthorized;
        if (!authPrincipal.IsInRole(AuthorizationRoles.User.ToString())) return DomainErrors.Authorization.Unauthorized;
        if (await _userRepository.GetAsync(query.UserId) is not User inquiry) return DomainErrors.User.NotFound;

        return _mapper.Map<UserResult>(inquiry);
    }
}