
using Logic.Common.Interfaces.Auth;

namespace ConsoleApp.Tests.Common.Fixtures
{
    //fixture to set up a shared injection of ISender and IMapper for all tests to use
    public class ControllerFixture : IDisposable
    {
        public ControllerFixture(
            ISender mediator, 
            IMapper mapper, 
            IAuthProvider authProvider
        )
        {
            Mediator = mediator;
            Mapper = mapper;
            AuthProvider = authProvider;
        }

        public ISender Mediator { get; private set; }
        public IMapper Mapper { get; private set; }
        public IAuthProvider AuthProvider { get; private set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
