
namespace Domain.Common.Errors
{
    public static partial class DomainErrors
    {
        public static class AccountManager
        {
            private readonly static string entityName = "AccountManager";

            public static Error Validation => Error.Validation(
                code: entityName + ".Validation",
                description: "one or more inputs are invalid"
            );

            public static Error Failure => Error.Failure(
                code: entityName + ".GeneralFailure",
                description: "a failure has occurred"
            );
        }
    }
}