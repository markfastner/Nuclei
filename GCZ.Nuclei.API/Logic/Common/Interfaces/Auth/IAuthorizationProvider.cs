using System.Security.Principal;

namespace Logic.Common.Interfaces.Auth
{
    public interface IAuthProvider
    {
        public int AuthenticationAttempts();
        public void IncrementAttempt();
        public void ClearAttempts();
        public string? OTP();
        public GenericPrincipal? GetPrincipal();
        public bool GeneratePrincipal(Account account);
        public bool ClearPrincipal();
        public string GenerateOTP();
        public bool ValidateOTP(string OTP);
    }
}
