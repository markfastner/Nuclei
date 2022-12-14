using Logic.AccountManager.Common;
using Logic.Common.Interfaces.Auth;
using Logic.Common.Interfaces.Persistence;
using Logic.Common.Interfaces.Utilities;
using System.Text.RegularExpressions;

namespace Logic.AccountManager.Commands;

public record RegisterCommand(
    User User,
    string Email,
    string Password
) : IRequest<ErrorOr<AccountManagerResult>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<AccountManagerResult>>
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccountRepository _accountRepository;
    private readonly IUserRepository _userRepository;

    public RegisterCommandHandler(
        IAccountRepository accountRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher
    )
    {
        _passwordHasher = passwordHasher;
        _accountRepository = accountRepository;
        _userRepository = userRepository;
    }

    //temporary input validation for RegisterCommand
    private bool Validate(RegisterCommand command)
    {
        //https://stackoverflow.com/questions/201323/how-can-i-validate-an-email-address-using-a-regular-expression
        var emailRegex = new Regex(
            "(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])"
        )
            .IsMatch(command.Email);
        //https://stackoverflow.com/questions/19605150/regex-for-password-must-contain-at-least-eight-characters-at-least-one-number-a
        var passwordRegex = new Regex(
            "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[.,@!-]).{8,}$"
        )
            .IsMatch(command.Password);

        var emailValidation = command.Email.Length > 0;
        var passwordValidation = 0 < command.Password.Length && command.Password.Length < 50;

        return emailRegex & emailValidation & passwordRegex & passwordValidation;
    }

    public async Task<ErrorOr<AccountManagerResult>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        if (!Validate(command)) return DomainErrors.AccountManager.Validation;

        //validate if account doesn't exist
        if (await _accountRepository.GetByEmailAsync(command.Email) is not null) return DomainErrors.AccountManager.Validation;

        //create user
        User user = new()
        {
            FirstName = command.User.FirstName,
            LastName = command.User.LastName,
            Birthday = command.User.Birthday,
        };

        //return failure if adding user failed
        if (!await _userRepository.InsertAsync(user)) return DomainErrors.AccountManager.Failure;

        //create account
        Account account = new()
        {
            Email = command.Email,
            Password = _passwordHasher.Hash(command.Password),      //hash password
            UserId = user.UserId        //associate user with account
        };

        //if inserting to data is unsuccessful
        if (!await _accountRepository.InsertAsync(account)) return DomainErrors.AccountManager.Failure;

        return new AccountManagerResult(
            account
        );
    }
}