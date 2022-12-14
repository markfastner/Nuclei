using Contracts.User;
using Contracts.AccountManager;
using Microsoft.EntityFrameworkCore;
using Logic.Common.Interfaces.Auth;
using ErrorOr;

namespace ConsoleApp.Tests.Authentication
{
    public class AuthenticationTest : IClassFixture<ControllerFixture>, IDisposable
    {
        private readonly AutoMock _autoMock;
        private readonly ControllerFixture _fixture;
        private readonly NucleiDbContext _dbContext;
        private readonly IAuthProvider _authProvider;

        public AuthenticationTest(
            ControllerFixture fixture,
            NucleiDbContext dbContext,
            IAuthProvider authProvider
            )
        {
            _autoMock = AutoMock.GetLoose();
            _fixture = fixture;
            _dbContext = dbContext;
            _authProvider = authProvider;

            _dbContext.Database.EnsureCreatedAsync();

            RegisterValidUser();        //register a valid user per test
        }

        private Task RegisterValidUser()
        {
            RegisterRequest request = new(
                User: new InsertUserRequest(
                    FirstName: "Test",
                    LastName: "Test",
                    Birthday: DateTime.UtcNow
                ),
                Email: "newAuthenticate@email.com",
                Password: "Test123@"
            );

            var stringBuilder = new StringBuilder()       //handles console input
                .AppendLine("n")
                .AppendLine("y");
            StringReader stringReader = new(stringBuilder.ToString());

            Console.SetIn(stringReader);

            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, It.IsAny<IAuthProvider>());

            return accountManager.Register(request);
        }

        public void Dispose()
        {
            _autoMock.Dispose();
            _dbContext.Dispose();
            GC.SuppressFinalize(this);
        }

        //User submits valid security credentials and is authenticated.
        [Fact]
        public async void AuthenticationTest_ReturnsSuccessCase_1()
        {
            //assemble
            _authProvider.ClearAttempts();
            _autoMock.Mock<IAuthProvider>()     //mock the security provider
                .Setup(x => x.ValidateOTP(It.IsAny<string>()))
                .Returns(true);

            LoginRequest request = new(
                Email: "newAuthenticate@email.com",
                Password: "Test123@"
            );

            //act
            var authProvider = _autoMock.Create<IAuthProvider>();
            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, authProvider);
            
            //logins and returns account id and email, then is taken to a user home view on the front end side
            var response = await accountManager.Login(request) as AccountManagerResponse;

            _authProvider.ClearPrincipal();

            //assert
            Assert.NotNull(response);
            Assert.IsType<AccountManagerResponse>(response);
            Assert.Equal(request.Email, response.Email);
        }

        //User submits invalid username.
        [Fact]
        public async void AuthenticationTest_ReturnsFailureCase_1()
        {
            //assemble
            _authProvider.ClearAttempts();
            _autoMock.Mock<IAuthProvider>()     //mock the security provider
                .Setup(x => x.ValidateOTP(It.IsAny<string>()))
                .Returns(true);

            LoginRequest request = new(
                Email: "newAuthenticateWrong@email.com",
                Password: "Test123@"
            );

            //act
            var authProvider = _autoMock.Create<IAuthProvider>();
            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, authProvider);
            var result = await accountManager.Login(request) as List<Error>;

            //assert
            Assert.NotNull(result);
            Assert.IsType<List<Error>>(result);
            Assert.Single(result);
            Assert.Equal(DomainErrors.Authentication.InvalidCredentials.Code, result.First().Code);
            Assert.Equal(DomainErrors.Authentication.InvalidCredentials.Description, result.First().Description);
            Assert.IsType<ErrorType>(result.First().Type);
        }

        //User submits invalid OTP.
        [Fact]
        public async void AuthenticationTest_ReturnsFailureCase_2()
        {
            //assemble
            _authProvider.ClearAttempts();
            _autoMock.Mock<IAuthProvider>()     //mock the security provider
                .Setup(x => x.ValidateOTP(It.IsAny<string>()))
                .Returns(false);        //validating OTP fails

            LoginRequest request = new(
                Email: "newAuthenticate@email.com",
                Password: "Test123@"
            );

            //act
            var authProvider = _autoMock.Create<IAuthProvider>();
            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, authProvider);

            //logins but the OTP is incorrect
            var result = await accountManager.Login(request) as List<Error>;

            _authProvider.ClearPrincipal();

            //assert
            Assert.NotNull(result);
            Assert.IsType<List<Error>>(result);
            Assert.Single(result);
            Assert.Equal(DomainErrors.Authentication.InvalidCredentials.Code, result.First().Code);
            Assert.Equal(DomainErrors.Authentication.InvalidCredentials.Description, result.First().Description);
            Assert.IsType<ErrorType>(result.First().Type);
        }

        //User submits invalid password.
        [Fact]
        public async void AuthenticationTest_ReturnsFailureCase_3()
        {
            //assemble
            _authProvider.ClearAttempts();
            _autoMock.Mock<IAuthProvider>()     //mock the security provider
                .Setup(x => x.ValidateOTP(It.IsAny<string>()))
                .Returns(true);

            LoginRequest request = new(
                Email: "newAuthenticate@email.com",
                Password: "Test123zzzzWrong"
            );

            //act
            var authProvider = _autoMock.Create<IAuthProvider>();
            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, authProvider);

            //logins and returns account id and email, then is taken to a user home view on the front end side
            var result = await accountManager.Login(request) as List<Error>;

            //assert
            Assert.NotNull(result);
            Assert.IsType<List<Error>>(result);
            Assert.Single(result);
            Assert.Equal(DomainErrors.Authentication.InvalidCredentials.Code, result.First().Code);
            Assert.Equal(DomainErrors.Authentication.InvalidCredentials.Description, result.First().Description);
            Assert.IsType<ErrorType>(result.First().Type);
        }

        //User submits valid security credentials for a disabled account.
        [Fact]
        public async void AuthenticationTest_ReturnsFailureCase_4()
        {
            //assemble
            _authProvider.ClearAttempts();
            _autoMock.Mock<IAuthProvider>()     //mock the security provider
                .Setup(x => x.ValidateOTP(It.IsAny<string>()))
                .Returns(true);

            //register an account to be deleted
            RegisterRequest request = new(
                User: new InsertUserRequest(
                    FirstName: "Test",
                    LastName: "Test",
                    Birthday: DateTime.UtcNow
                ),
                Email: "accountToBeDeleted@email.com",
                Password: "Test123@"
            );

            var stringBuilder = new StringBuilder()       //handles console input
                .AppendLine("n")
                .AppendLine("y");
            StringReader stringReader = new(stringBuilder.ToString());

            Console.SetIn(stringReader);

            var authProvider = _autoMock.Create<IAuthProvider>();
            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, authProvider);
            var accountResponse = await accountManager.Register(request) as AccountManagerResponse;

            //remove user from database
            var account = await _dbContext.Account.FirstOrDefaultAsync(x => x.AccountId == accountResponse!.AccountId);
            
            _dbContext.Account.Remove(account!);
            await _dbContext.SaveChangesAsync();

            LoginRequest loginRequest = new(
                Email: "accountToBeDeleted@email.com",
                Password: "Test123@"
            );

            //act
            //logins with deleted account credentials
            var result = await accountManager.Login(loginRequest) as List<Error>;

            //assert
            Assert.NotNull(result);
            Assert.IsType<List<Error>>(result);
            Assert.Single(result);
            Assert.Equal(DomainErrors.Authentication.InvalidCredentials.Code, result.First().Code);
            Assert.Equal(DomainErrors.Authentication.InvalidCredentials.Description, result.First().Description);
            Assert.IsType<ErrorType>(result.First().Type);
        }
    }
}