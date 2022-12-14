
namespace Logic.Tests.Logging
{   
    //https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database#inmemory-provider
    public class LoggingTest : IDisposable
    {
        private readonly AutoMock _autoMock;

        public LoggingTest()
        {
            _autoMock = AutoMock.GetLoose();
        }

        public void Dispose()
        {
            _autoMock.Dispose();
            GC.SuppressFinalize(this);
        }

        /*
        private readonly AutoMock _autoMock;
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<NucleiDbContext> _dbContextOptions;

        public LoggingTest()
        {
            _autoMock = AutoMock.GetLoose();
            _connection = new SqliteConnection("Filename=:memory:");   
            _connection.Open();

            _dbContextOptions = new DbContextOptionsBuilder<NucleiDbContext>()
                .UseSqlite(_connection)
                .Options;
        }

        public void Dispose()
        {
            _autoMock.Dispose();
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        private NucleiDbContext CreateDbContext()
        {
            var dbContext = new NucleiDbContext(_dbContextOptions);

            dbContext.Database.EnsureCreated();

            return dbContext;
        }

        private NucleiDbContext CreateDbContext(List<Log> seed)
        {
            var dbContext = new NucleiDbContext(_dbContextOptions);

            if (dbContext.Database.EnsureCreated())
            {
                dbContext.AddRange(seed);
                dbContext.SaveChanges();
            }

            return dbContext;
        }

        [Fact]
        public async void LoggingTest_SuccessCase_1()
        {
            //assemble
            Log log = new() { LogLevel = 0, Operation = "System" };
            using var dbContext = CreateDbContext(new List<Log> { log });

            //action
            Log? result = await dbContext.Log
                .FirstOrDefaultAsync(x => x.LogId == log.LogId);

            //assert
            Assert.NotNull(result);
            Assert.Equal((byte)0, result?.LogLevel);      //assert log has level of 2 indicating a success
            Assert.Contains("system", result?.Operation?.ToLower());
        }

        [Fact]
        public async void LoggingTest_SuccessCase_2()
        {
            //assemble
            Log log = new() { LogLevel = 4, Operation = "System" };
            using var dbContext = CreateDbContext(new List<Log> { log });

            //action
            Log? result = await dbContext.Log
                .FirstOrDefaultAsync(x => x.LogId == log.LogId);

            //assert
            Assert.NotNull(result);
            Assert.Equal((byte)4, result?.LogLevel);      //assert log has level of 4 indicating an error
            Assert.Contains("system", result?.Operation?.ToLower());
        }

        [Fact]
        public async void LoggingTest_SuccessCase_3()
        {
            //assemble
            Log log = new() { LogLevel = 2, Operation = "TRequest", Message = "ErrorOr" };
            using var dbContext = CreateDbContext(new List<Log> { log });

            //action
            Log? result = await dbContext.Log
                .FirstOrDefaultAsync(x => x.LogId == log.LogId);

            //assert
            Assert.NotNull(result);
            Assert.Equal((byte)2, result?.LogLevel);      //assert log has level of 2 indicating an error
            Assert.Contains("request", result?.Operation?.ToLower());
            Assert.Contains("erroror", result?.Message?.ToLower());
        }

        [Fact]
        public async void LoggingTest_SuccessCase_4()
        {
            //assemble
            Log log = new() { LogLevel = 4, Operation = "TRequest", Message = "ErrorOr" };
            using var dbContext = CreateDbContext(new List<Log> { log });

            //action
            Log? result = await dbContext.Log
                .FirstOrDefaultAsync(x => x.LogId == log.LogId);

            //assert
            Assert.NotNull(result);
            Assert.Equal((byte)4, result?.LogLevel);      //assert log has level of 4 indicating a success
            Assert.Contains("request", result?.Operation?.ToLower());
            Assert.Contains("erroror", result?.Message?.ToLower());
        }

        [Fact]
        public async void LoggingTest_FailureCase_1()
        {
            //assemble
            using var dbContext = CreateDbContext();
            CancellationTokenSource cancellationTokenSource = new();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            //action
            dbContext.Log.Add(new Log());
            cancellationTokenSource.Cancel();

            //assert
            Assert.True(cancellationToken.IsCancellationRequested);
            await Assert.ThrowsAsync<TaskCanceledException>(() => dbContext.SaveChangesAsync(cancellationToken));
        }

        [Fact]
        public async void LoggingTest_FailureCase_2()
        {
            //assemble
            Log log = new() { LogLevel = 2, Operation = "System" };
            using var dbContext = CreateDbContext(new List<Log>() { log });

            //action
            Log? result = await dbContext.Log
                .FirstOrDefaultAsync(x => x.LogId == log.LogId);

            //block user from interacting with system
            bool blocked = true;

            //assert
            Assert.NotNull(result);
            Assert.True(blocked);
        }

        [Fact]
        public async void LoggingTest_FailureCase_3()
        {
            //assert
            var repositoryMock = _autoMock.Mock<ILoggingRepository>();
            var controller = repositoryMock.Object;
            Stopwatch stopwatch = new();

            repositoryMock
                .Setup(x => x.InsertAsync(It.IsAny<Log>()))
                .ReturnsAsync(false);

            //action
            stopwatch.Start();

            bool result = await controller.InsertAsync(new Log());

            stopwatch.Stop();

            //assert
            Assert.False(result);       //asserts that inserting into db was a failure
            Assert.True(stopwatch.ElapsedMilliseconds < 5000);
        }

        [Fact]
        public async void LoggingTest_FailureCase_4()
        {
            //assemble
            Log log = new();       //log entry with completely null fields
            using var dbContext = CreateDbContext(new List<Log> { log });

            Stopwatch stopwatch = new();

            //action
            stopwatch.Start();

            Log? result = await dbContext.Log
                .FirstOrDefaultAsync(x => x.LogId == log.LogId);

            stopwatch.Stop();

            //assert
            Assert.NotNull(result);
            Assert.NotEqual((byte)0, result?.LogLevel);
            Assert.NotEqual("SomeValidData", result?.Message);
            Assert.NotEqual("SomeValidData", result?.Operation);
            Assert.NotEqual("SomeValidData", result?.Category);
            Assert.True(stopwatch.ElapsedMilliseconds < 5000);
        }

        [Fact]
        public async void LoggingTest_FailureCase_5()
        {
            //assemble
            Log log = new() { LogLevel = 2, Operation = "System" };
            using var dbContext = CreateDbContext(new List<Log>() { log });

            //action
            Log? result1 = await dbContext.Log
                .FirstOrDefaultAsync(x => x.LogId == log.LogId);

            Log result1Copy = new()
            {
                LogLevel = result1!.LogLevel,
                Category = result1!.Category,
                Operation = result1!.Operation,
                Message = result1!.Message
            };

            log.Operation = "Mutated";

            dbContext.Log.Update(log);
            dbContext.SaveChanges();

            Log? result2 = await dbContext.Log
                .FirstOrDefaultAsync(x => x.LogId == log.LogId);

            //assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotEqual(result1Copy?.Operation, result2?.Operation);      //assert that the log has been mutated
        }
    */
    }
}