
using Contracts.Account;
using Contracts.AccountManager;
using Contracts.Common;
using Contracts.User;
using Logic.Common.Interfaces.Auth;

namespace ConsoleApp.Tests.Authorization
{
    public class AuthorizationTest : IClassFixture<ControllerFixture>, IDisposable
    {
        private readonly AutoMock _autoMock;
        private readonly ControllerFixture _fixture;
        private readonly NucleiDbContext _dbContext;
        private readonly IAuthProvider _authProvider;

        public AuthorizationTest(
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

            RegisterValidUser();     //register a valid user per test
        }

        public void Dispose()
        {
            _autoMock.Dispose();
            _dbContext.Dispose();
            GC.SuppressFinalize(this);
        }

        private Task RegisterValidUser()
        {
            RegisterRequest request = new(
                User: new InsertUserRequest(
                    FirstName: "Test",
                    LastName: "Test",
                    Birthday: DateTime.UtcNow
                ),
                Email: "newAuthorize@email.com",
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

        //User attempts to access a protected functionality within authorization scope.
        //Access is granted to perform functionality.
        [Fact]
        public async void AuthorizationTest_ReturnsSuccessCase_1()
        {
            //assemble
            _authProvider.ClearAttempts();
            _autoMock.Mock<IAuthProvider>()     //mock the security provider
                .Setup(x => x.ValidateOTP(It.IsAny<string>()))
                .Returns(true);

            LoginRequest request = new(
                Email: "newAuthorize@email.com",
                Password: "Test123@"
            );

            LogoutRequest logout = new(
                Email: "newAuthorize@email.com"
            );

            //act
            var authProvider = _autoMock.Create<IAuthProvider>();
            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, authProvider);

            //attempt to logout--which requires authorization after logging in
            await accountManager.Login(request);

            var response = await accountManager.Logout(logout) as CommandResponse;

            //assert
            Assert.NotNull(response);
            Assert.IsType<CommandResponse>(response);
            Assert.True(response.Result);
        }

        //User attempts to access protected data within authorization scope.
        //Access is granted to perform read operations.
        [Fact]
        public async void AuthorizationTest_ReturnsSuccessCase_2()
        {
            //assemble
            _authProvider.ClearAttempts();
            _autoMock.Mock<IAuthProvider>()     //mock the security provider
                .Setup(x => x.ValidateOTP(It.IsAny<string>()))
                .Returns(true);

            LoginRequest request = new(
                Email: "newAuthorize@email.com",
                Password: "Test123@"
            );

            //act
            var authProvider = _autoMock.Create<IAuthProvider>();
            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, authProvider);
            var accountController = new AccountController(_fixture.Mediator, _fixture.Mapper);
            
            //fetches user data
            var account = await accountManager.Login(request) as AccountManagerResponse;
            var response = await accountController.Get(account!.AccountId) as AccountResponse;
            
            _authProvider.ClearPrincipal();

            //assert
            Assert.NotNull(response);
            Assert.IsType<AccountResponse>(response);
            Assert.Equal(account.AccountId, response.AccountId);
            Assert.Equal(request.Email, response.Email);
        }

        //User attempts to modify protected data within authorization scope.
        //Access is granted to perform write operations.
        [Fact]
        public async void AuthorizationTest_ReturnsSuccessCase_3()
        {
            //assemble
            _authProvider.ClearAttempts();
            _autoMock.Mock<IAuthProvider>()     //mock the security provider
                .Setup(x => x.ValidateOTP(It.IsAny<string>()))
                .Returns(true);

            LoginRequest request = new(
                Email: "newAuthorize@email.com",
                Password: "Test123@"
            );

            //act
            var authProvider = _autoMock.Create<IAuthProvider>();
            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, authProvider);
            var accountController = new AccountController(_fixture.Mediator, _fixture.Mapper);
            var userController = new UserController(_fixture.Mediator, _fixture.Mapper);

            //fetches user data
            var account = (await accountManager.Login(request) as AccountManagerResponse)!;
            var user = (await accountController.Get(account.AccountId) as AccountResponse)!;
            UpdateUserRequest updateUser = new(
                UserId: user.User.UserId,
                FirstName: "NewName",
                LastName: user.User.LastName,
                Birthday: user.User.Birthday
            );
            var result = await userController.Update(updateUser) as CommandResponse;        //performs update on user
            var updatedUser = await userController.Get(user.User.UserId) as UserResponse;       //now retrieve updated user data
            
            _authProvider.ClearPrincipal();

            //assert
            Assert.NotNull(result);
            Assert.NotNull(updatedUser);
            Assert.True(result.Result);
            Assert.IsType<UserResponse>(updatedUser);
            Assert.Equal(updatedUser.FirstName, updatedUser.FirstName);
        }

        //logging behavior fails on an authorization failure

        //User attempts to access a protected functionality outside of authorization scope.
        [Fact]
        public async void AuthorizationTest_ReturnsFailureCase_1()
        {
            //assemble
            _authProvider.ClearAttempts();
            _autoMock.Mock<IAuthProvider>()     //mock the security provider
                .Setup(x => x.ValidateOTP(It.IsAny<string>()))
                .Returns(true);

            LogoutRequest logout = new(
                Email: "newAuthorize@email.com"
            );

            //act
            var authProvider = _autoMock.Create<IAuthProvider>();
            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, authProvider);

            //attempt to logout before logging in, results in an error
            var result = await accountManager.Logout(logout) as List<Error>;

            //assert
            Assert.NotNull(result);
            Assert.IsType<List<Error>>(result);
            Assert.Single(result);
            Assert.Equal(DomainErrors.Authorization.Unauthorized.Code, result.First().Code);
            Assert.Equal(DomainErrors.Authorization.Unauthorized.Description, result.First().Description);
            Assert.IsType<ErrorType>(result.First().Type);
        }

        //User attempts to access protected data outside of authorization scope.
        [Fact]
        public async void AuthorizationTest_ReturnsFailureCase_2()
        {
            //assemble
            _authProvider.ClearAttempts();
            _autoMock.Mock<IAuthProvider>()     //mock the security provider
                .Setup(x => x.ValidateOTP(It.IsAny<string>()))
                .Returns(true);

            LoginRequest request = new(
                Email: "newAuthorize@email.com",
                Password: "Test123@"
            );

            //act
            var authProvider = _autoMock.Create<IAuthProvider>();
            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, authProvider);
            var userController = new UserController(_fixture.Mediator, _fixture.Mapper);

            //fetches user data
            var account = await accountManager.Login(request) as AccountManagerResponse;
            var result = await userController.GetAll() as List<Error>;       //try getting all result after logging in
            
            _authProvider.ClearPrincipal();

            //assert
            Assert.NotNull(account);
            Assert.NotNull(result);
            Assert.NotEmpty(account.Email);
            Assert.IsType<List<Error>>(result);
            Assert.Single(result);
            Assert.Equal(DomainErrors.Authorization.Unauthorized.Code, result.First().Code);
            Assert.Equal(DomainErrors.Authorization.Unauthorized.Description, result.First().Description);
            Assert.IsType<ErrorType>(result.First().Type);
        }

        //User attempts to modify protected data outside of authorization scope.
        [Fact]
        public async void AuthorizationTest_ReturnsFailureCase_3()
        {
            //assemble
            _authProvider.ClearAttempts();
            _autoMock.Mock<IAuthProvider>()     //mock the security provider
                .Setup(x => x.ValidateOTP(It.IsAny<string>()))
                .Returns(true);

            LoginRequest request = new(
                Email: "newAuthorize@email.com",
                Password: "Test123@"
            );

            //act
            var authProvider = _autoMock.Create<IAuthProvider>();
            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, authProvider);
            var accountController = new AccountController(_fixture.Mediator, _fixture.Mapper);

            //fetches user data and attempt to delete an account, which is unauthorized to normal users
            var account = (await accountManager.Login(request) as AccountManagerResponse)!;
            var result = await accountController.Delete(account.AccountId) as List<Error>;
            
            _authProvider.ClearPrincipal();

            //assert
            Assert.NotNull(account);
            Assert.NotNull(result);
            Assert.IsType<List<Error>>(result);
            Assert.Single(result);
            Assert.Equal(DomainErrors.Authorization.Unauthorized.Code, result.First().Code);
            Assert.Equal(DomainErrors.Authorization.Unauthorized.Description, result.First().Description);
            Assert.IsType<ErrorType>(result.First().Type);
        }
    }
}
