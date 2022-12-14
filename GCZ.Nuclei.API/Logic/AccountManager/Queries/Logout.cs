using Logic.Common.Interfaces.Persistence;
using System.Security.Principal;
using Logic.AccountManager.Common;
using System.Text.RegularExpressions;
using Logic.Common.Interfaces.Auth;

namespace Logic.AccountManager.Queries
{
    public record LogoutQuery(
        string Email    
    ) : IRequest<ErrorOr<bool>>;

    public class LogoutQueryHandler : IRequestHandler<LogoutQuery, ErrorOr<bool>>
    {
        private readonly IAuthProvider _authProvider;
        private readonly IAccountRepository _accountRepository;

        public LogoutQueryHandler(
            IAuthProvider authProvider,
            IAccountRepository accountRepository
        )
        {
            _authProvider = authProvider;
            _accountRepository = accountRepository;
        }

        //temporary input validation for LoginQuery
        private bool Validate(LogoutQuery query)
        {
            //https://stackoverflow.com/questions/201323/how-can-i-validate-an-email-address-using-a-regular-expression
            var emailRegex = new Regex(
                "(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])"
            )
                .IsMatch(query.Email);

            var emailValidation = query.Email.Length > 0;

            return emailRegex & emailValidation;
        }

        public async Task<ErrorOr<bool>> Handle(LogoutQuery query, CancellationToken cancellationToken)
        {
            if (!Validate(query)) return DomainErrors.Authentication.InvalidCredentials;

            //only authorize Users that have declared to a principal to this method
            if (_authProvider.GetPrincipal() is not GenericPrincipal authPrincipal) return DomainErrors.Authorization.Unauthorized;
            if (!authPrincipal.IsInRole(AuthorizationRoles.User.ToString())) return DomainErrors.Authorization.Unauthorized;

            //validate if account exists
            if (await _accountRepository.GetByEmailAsync(query.Email, true) is null) return DomainErrors.Authorization.Failure;

            _authProvider.ClearPrincipal();     //clear the principal

            return true;
        }
    }
}