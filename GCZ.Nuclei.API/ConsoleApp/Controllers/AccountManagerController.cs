using Logic.AccountManager.Commands;
using Logic.AccountManager.Common;
using Logic.AccountManager.Queries;
using Contracts.AccountManager;
using Contracts.Common;
using Logic.Common.Interfaces.Auth;
using Domain.Common.Errors;

namespace ConsoleApp.Controllers
{
    public class AccountManagerController : ConsoleController
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;
        private readonly IAuthProvider _authProvider;

        public AccountManagerController(
            ISender mediator, 
            IMapper mapper,
            IAuthProvider authProvider
        )
        {
            _mediator = mediator;
            _mapper = mapper;
            _authProvider = authProvider;
        }
        
        public async Task<object> Login(LoginRequest request)
        {
            var query = _mapper.Map<LoginQuery>(request);
            var result = await _mediator.Send(query);

            Console.WriteLine("Enter OTP: ");
            
            //reassign result as an error if OTP was invalid; consequently returns error response
            if (!_authProvider.ValidateOTP(Console.ReadLine()!)) result = DomainErrors.Authentication.InvalidCredentials;

            return result.Match<object>(
                value => _mapper.Map<AccountManagerResponse>(value),
                errors => errors
            );
        }

        public async Task<object> Logout(LogoutRequest request)
        {
            var query = _mapper.Map<LogoutQuery>(request);
            var result = await _mediator.Send(query);

            return result.Match<object>(
                value => new CommandResponse(Result: result.Value),
                error => error
            );
        }

        public async Task<object?> Register(RegisterRequest request)
        {
            //user EULA
            Console.WriteLine("Are you a robot? (Y/N)");

            if (Console.ReadLine()?.ToLower() == "y") return null;

            Console.WriteLine("Do you agree with our terms and policies? (Y/N)");

            if (Console.ReadLine()?.ToLower() == "n") return null;

            var command = _mapper.Map<RegisterCommand>(request);
            ErrorOr<AccountManagerResult> result = await _mediator.Send(command, Cts.Token);

            return result.Match<object>(
                value => _mapper.Map<AccountManagerResponse>(value),
                errors => errors
            );
        }
    }
}