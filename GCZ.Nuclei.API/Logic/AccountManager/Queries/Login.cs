using Logic.AccountManager.Commands;
using Logic.AccountManager.Common;
using Logic.Common.Interfaces.Auth;
using Logic.Common.Interfaces.Persistence;
using Logic.Common.Interfaces.Utilities;
using System.Text.RegularExpressions;

namespace Logic.AccountManager.Queries;

public record LoginQuery(
    string Email,
    string Password
) : IRequest<ErrorOr<AccountManagerResult>>;

public class LoginQueryHandler : IRequestHandler<LoginQuery, ErrorOr<AccountManagerResult>>
{
    private readonly IAuthProvider _authProvider;
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHasher _passwordHasher;

    public LoginQueryHandler(
        IAuthProvider authProvider,
        IAccountRepository accountRepository,
        IPasswordHasher passwordHasher
    )
    {
        _authProvider = authProvider;
        _accountRepository = accountRepository;
        _passwordHasher = passwordHasher;
    }

    //temporary input validation for LoginQuery
    private bool Validate(LoginQuery query)
    {
        //https://stackoverflow.com/questions/201323/how-can-i-validate-an-email-address-using-a-regular-expression
        var emailRegex = new Regex(
            "(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])"
        )
            .IsMatch(query.Email);
        //https://stackoverflow.com/questions/19605150/regex-for-password-must-contain-at-least-eight-characters-at-least-one-number-a
        var passwordRegex = new Regex(
            "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[.,@!-]).{8,}$"
        )
            .IsMatch(query.Password);

        var emailValidation = query.Email.Length > 0;
        var passwordValidation = 0 < query.Password.Length && query.Password.Length < 50;

        return emailRegex & emailValidation & passwordRegex & passwordValidation;
    }

    public async Task<ErrorOr<AccountManagerResult>> Handle(LoginQuery query, CancellationToken cancellationToken)
    {
        _authProvider.IncrementAttempt();       //increment an attempt to login

        if (!Validate(query)) return DomainErrors.Authentication.InvalidCredentials;        //normally would return Validation error, but an InvalidCredentials is more ambigious for antimalice

        //validate if account exists
        if (await _accountRepository.GetByEmailAsync(query.Email, true) is not Account account) return DomainErrors.Authentication.InvalidCredentials;

        //validate password        
        if (!_passwordHasher.Verify(query.Password, account.Password)) return DomainErrors.Authentication.InvalidCredentials;

        //return failure if user is not associated with account
        if (account.User is null) return DomainErrors.Authentication.Failure;

        //generate a authentication principal based off user
        if (!_authProvider.GeneratePrincipal(account)) return DomainErrors.Authentication.Failure;

        Console.WriteLine("Your OTP: " + _authProvider.GenerateOTP());

        return new AccountManagerResult(
            account
        );
    }
}