using Contracts.User;
using System.Diagnostics;
using Logic.AccountManager.Commands;
using Logic.AccountManager.Common;
using Contracts.AccountManager;
using Logic.Common.Interfaces.Persistence;
using Logic.Common.Interfaces.Auth;

namespace ConsoleApp.Tests.Registration
{
    public class RegistrationTest : IClassFixture<ControllerFixture>, IDisposable
    {
        private readonly AutoMock _autoMock;
        private readonly ControllerFixture _fixture;
        private readonly NucleiDbContext _dbContext;
        private readonly IAuthProvider _authProvider;

        public RegistrationTest(
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
        }

        public void Dispose()
        {
            _autoMock.Dispose();
            _dbContext.Dispose();
            GC.SuppressFinalize(this);
        }

        //User registers with a valid email address and valid password.
        //The system, without timing out, assigns the email as the user’s unique username,
        //displays a success message to the interface, and associates the unique username/email to the user.
        [Fact]
        public async void RegistrationTest_ReturnsSuccessCase_1()
        {
            //assemble
            RegisterRequest request = new(
                User: new InsertUserRequest(
                    FirstName: "Test",
                    LastName: "Test",
                    Birthday: DateTime.UtcNow
                ),
                Email: "valid@email.com",
                Password: "Test123@"
            );

            var stringBuilder = new StringBuilder()       //handles console input
                .AppendLine("n")
                .AppendLine("y");
            StringReader stringReader = new(stringBuilder.ToString());

            Console.SetIn(stringReader);

            //act
            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, _authProvider);
            var response = await accountManager.Register(request) as AccountManagerResponse;

            //assert
            Assert.NotNull(response);
            Assert.IsType<AccountManagerResponse>(response);
            Assert.True(response.AccountId > 0);
            Assert.Equal(request.Email, response.Email);
        }

        //User registers with an invalid email address
        [Fact]
        public async void RegistrationTest_ReturnsFailureCase_1()
        {
            //assemble
            RegisterRequest request = new(
                User: new InsertUserRequest(
                    FirstName: "Test",
                    LastName: "Test",
                    Birthday: DateTime.UtcNow
                ),
                Email: "validemail.com",
                Password: "Test123@"
            );

            var stringBuilder = new StringBuilder()       //handles console input
                .AppendLine("n")
                .AppendLine("y");
            StringReader stringReader = new(stringBuilder.ToString());

            Console.SetIn(stringReader);

            //act
            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, _authProvider);
            var result = await accountManager.Register(request) as List<Error>;

            //assert
            Assert.NotNull(result);
            Assert.IsType<List<Error>>(result);
            Assert.Single(result);
            Assert.Equal(DomainErrors.AccountManager.Validation.Code, result.First().Code);
            Assert.Equal(DomainErrors.AccountManager.Validation.Description, result.First().Description);
            Assert.IsType<ErrorType>(result.First().Type);
        }

        //User registers with a valid email address, but it is not unique within the system.
        //This causes our system to not assign a username.
        [Fact]
        public async void RegistrationTest_ReturnsFailureCase_2()
        {
            //assemble
            RegisterRequest request = new(
                User: new InsertUserRequest(
                    FirstName: "Test",
                    LastName: "Test",
                    Birthday: DateTime.UtcNow
                ),
                Email: "used@email.com",
                Password: "Test123@"
            );

            var stringBuilder = new StringBuilder()       //handles console input
                .AppendLine("n")
                .AppendLine("y");
            StringReader stringReader = new(stringBuilder.ToString());

            Console.SetIn(stringReader);

            //seed db
            User seedUser = new()
            {
                FirstName = "Test",
                LastName = "Test",
                Birthday = DateTime.UtcNow
            };

            _dbContext.User.Add(seedUser);
            await _dbContext.SaveChangesAsync();

            _dbContext.Account.Add(new Account()
            {
                Email = "used@email.com",
                Password = "Test",
                UserId = seedUser.UserId
            });
            await _dbContext.SaveChangesAsync();

            //act
            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, _authProvider);
            var result = await accountManager.Register(request) as List<Error>;

            //assert
            Assert.NotNull(result);
            Assert.IsType<List<Error>>(result);
            Assert.Single(result);
            Assert.Equal(DomainErrors.AccountManager.Validation.Code, result.First().Code);
            Assert.Equal(DomainErrors.AccountManager.Validation.Description, result.First().Description);
            Assert.IsType<ErrorType>(result.First().Type);
        }

        //User registers with an invalid password
        [Fact]
        public async void RegistrationTest_ReturnsFailureCase_3()
        {
            //assemble
            RegisterRequest request = new(
                User: new InsertUserRequest(
                    FirstName: "Test",
                    LastName: "Test",
                    Birthday: DateTime.UtcNow
                ),
                Email: "anothervalid@email.com",
                Password: "Test1234"
            );

            var stringBuilder = new StringBuilder()       //handles console input
                .AppendLine("n")
                .AppendLine("y");
            StringReader stringReader = new(stringBuilder.ToString());

            Console.SetIn(stringReader);

            //act
            var accountManager = new AccountManagerController(_fixture.Mediator, _fixture.Mapper, _authProvider);
            var result = await accountManager.Register(request) as List<Error>;

            //assert
            Assert.NotNull(result);
            Assert.IsType<List<Error>>(result);
            Assert.Single(result);
            Assert.Equal(DomainErrors.AccountManager.Validation.Code, result.First().Code);
            Assert.Equal(DomainErrors.AccountManager.Validation.Description, result.First().Description);
            Assert.IsType<ErrorType>(result.First().Type);
        }

        //User enters correct information but the user is not able to be created inside the database.
        [Fact]
        public async void RegistrationTest_ReturnsFailureCase_4()
        {
            //assemble
            RegisterRequest request = new(
                User: new InsertUserRequest(
                    FirstName: "Test",
                    LastName: "Test",
                    Birthday: DateTime.UtcNow
                ),
                Email: "ValidDb@email.com",
                Password: "Test123@"
            );
            RegisterCommand command = _fixture.Mapper.Map<RegisterCommand>(request);

            _autoMock.Mock<IAccountRepository>()
                .Setup(x => x.GetByEmailAsync(request.Email, default))
                .ReturnsAsync(It.IsAny<Account>());
            _autoMock.Mock<IUserRepository>()       //mocks the result of a user being inserted into the database as false: a failure
                .Setup(x => x.InsertAsync(It.IsAny<User>()))
                .ReturnsAsync(false);

            var stringBuilder = new StringBuilder()       //handles console input
                .AppendLine("n")
                .AppendLine("y");
            StringReader stringReader = new(stringBuilder.ToString());

            Console.SetIn(stringReader);

            //act
            var registerHandler = _autoMock.Create<RegisterCommandHandler>();
            var result = await registerHandler.Handle(command, default);

            //assert
            Assert.True(result.IsError);        //assert that because inserting to database failed, result is an error object
            Assert.Null(result.Value);
            Assert.IsType<ErrorOr<AccountManagerResult>>(result);
            Assert.Single(result.Errors);
            Assert.Equal(DomainErrors.AccountManager.Failure.Code, result.FirstError.Code);
            Assert.Equal(DomainErrors.AccountManager.Failure.Description, result.FirstError.Description);
            Assert.IsType<ErrorType>(result.FirstError.Type);
        }

        //User registers, an account is created and the username is returned back to the user.
        //Process takes longer than 5 seconds.
        [Fact]
        public async void RegistrationTest_ReturnsFailureCase_5()
        {
            //assemble
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
            Stopwatch stopwatch = new();
            RegisterRequest request = new(
                User: new InsertUserRequest(
                    FirstName: "Test",
                    LastName: "Test",
                    Birthday: DateTime.UtcNow
                ),
                Email: "valid@email.com",
                Password: "Test123@"
            );

            var token = cts.Token;

            //act
            stopwatch.Start();
            Thread.Sleep(5000);     //simulate the service taking 5 seconds to complete

            var result = await _fixture.Mediator.Send(_fixture.Mapper.Map<RegisterCommand>(request), token);

            stopwatch.Stop();

            //assert
            Assert.IsType<ErrorOr<AccountManagerResult>>(result);
            Assert.True(stopwatch.ElapsedMilliseconds >= 5000);
            Assert.True(result.IsError);
            Assert.Equal(DomainErrors.System.Timeout.Code, result.FirstError.Code);
            Assert.Equal(DomainErrors.System.Timeout.Description, result.FirstError.Description);
            Assert.IsType<ErrorType>(result.FirstError.Type);
        }
    }
}