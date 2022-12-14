
namespace ConsoleApp.Controllers
{
    public class ConsoleController
    {
        protected CancellationTokenSource Cts;

        public ConsoleController()
        {
            //cancellation token is created with every mediator request which spans for 5 seconds
            Cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        }
    }
}
