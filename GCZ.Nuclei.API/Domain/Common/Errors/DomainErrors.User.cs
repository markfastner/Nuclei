using ErrorOr;

namespace Domain.Common.Errors
{
    public static partial class DomainErrors
    {
        public static class User
        {
            private const string entityName = "User";

            public static Error NotFound => Error.NotFound(
                code: entityName + ".NotFound",
                description: entityName + " was not found in the system"
            );

            public static Error Failure => Error.Failure(
                code: entityName + ".Failure",
                description: "a failure occurred in the system"
            );
        }
    }
}
