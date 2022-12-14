
namespace Domain.Common.Errors
{
    public static partial class DomainErrors
    {
        public static class Authentication
        {
            private readonly static string entityName = "Authentication";

            public static Error InvalidCredentials => Error.Validation(
                code: entityName + ".InvalidCredentials",
                description: "invalid credentials"
            );

            public static Error Failure => Error.Failure(
                code: entityName + ".GeneralFailure",
                description: "a failure has occurred"
            );
        }
    }
}