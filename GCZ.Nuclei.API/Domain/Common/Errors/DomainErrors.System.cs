
namespace Domain.Common.Errors
{
    public static partial class DomainErrors
    {
        public static class System
        {
            private const string entityName = "System";

            public static Error Timeout => Error.Custom(
                type: CustomErrorTypes.Timeout,
                code: entityName + ".Timeout",
                description: "the request has timed out"
            );
        }
    }
}
