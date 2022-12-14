
using Logic.AccountManager.Common;
using Logic.Common.Interfaces.Auth;
using Logic.Common.Interfaces.Persistence;
using Logic.Common.Results;
using MapsterMapper;
using System.Security.Principal;

namespace Logic.Queries.Users;

public record GetAllUsersQuery() : IRequest<ErrorOr<IEnumerable<UserResult>>>;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, ErrorOr<IEnumerable<UserResult>>>
{
    private readonly IMapper _mapper;
    private readonly IAuthProvider _authProvider;
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(
        IMapper mapper,
        IAuthProvider authProvider,
        IUserRepository userRepository
    )
    {
        _mapper = mapper;
        _authProvider = authProvider;
        _userRepository = userRepository;
    }

    public async Task<ErrorOr<IEnumerable<UserResult>>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        //only authorize admins to get all data
        if (_authProvider.GetPrincipal() is not GenericPrincipal authPrincipal) return DomainErrors.Authorization.Unauthorized;
        if (!authPrincipal.IsInRole(AuthorizationRoles.Admin.ToString())) return DomainErrors.Authorization.Unauthorized;
        var inquiry = await _userRepository.GetAllAsync();

        return _mapper.Map<List<UserResult>>(inquiry);
    }
}