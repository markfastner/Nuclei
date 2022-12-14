
namespace Domain.Common.Errors
{
    public static partial class DomainErrors
    {
        public static class Authorization
        {
            private readonly static string entityName = "Authorization";

            public static Error Unauthorized => Error.Custom(
                type: CustomErrorTypes.Unauthorized,
                code: entityName + ".Unauthorized",
                description: "unauthorized access"
            );

            public static Error Failure => Error.Failure(
                code: entityName + ".GeneralFailure",
                description: "a failure has occurred"
            );
        }
    }
}