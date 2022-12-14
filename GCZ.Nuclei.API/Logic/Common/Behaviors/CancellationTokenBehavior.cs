
namespace Logic.Common.Behaviors;

public class CancellationTokenBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    public CancellationTokenBehavior()
    {

    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        try
        {
            TResponse response = await next();

            cancellationToken.ThrowIfCancellationRequested();

            return response;
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
            return (dynamic)DomainErrors.System.Timeout;
        }
    }
}