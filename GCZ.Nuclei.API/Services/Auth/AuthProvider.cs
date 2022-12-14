using Domain.Entities;
using System.Security.Claims;
using System.Security.Principal;
using Logic.Common.Interfaces.Auth;

namespace Services.Auth
{
    //https://learn.microsoft.com/en-us/dotnet/standard/security/how-to-create-genericprincipal-and-genericidentity-objects
    public class AuthProvider : IAuthProvider
    {
        private int _authenticationAttempts;
        private GenericPrincipal? _principal;
        private string? _OTP;

        public AuthProvider()
        {
            _authenticationAttempts = 0;
            _principal = null;
            _OTP = null;
        }

        public int AuthenticationAttempts() => _authenticationAttempts;

        public void IncrementAttempt() => _authenticationAttempts++;

        public void ClearAttempts() => _authenticationAttempts = 0;     //testing purposes: clears attempts before each test since AuthPrvider is a shared singleton service

        public string? OTP() => _OTP;

        public GenericPrincipal? GetPrincipal() => _principal;

        public bool GeneratePrincipal(Account account)
        {
            if (_authenticationAttempts >= 3) return false;     //fail if attempted login succeeds 3 tries
            if (account.User is null) return false;

            //create a claim for identity
            var claims = new[]
            {
                new Claim(ClaimTypes.GivenName, account.User.FirstName),
                new Claim(ClaimTypes.Surname, account.User.LastName),
                new Claim(ClaimTypes.DateOfBirth, account.User.Birthday.ToString())
            };
            var userIdentity = new GenericIdentity(account.Email);

            userIdentity.AddClaims(claims);

            //create a new principal for indentity and assign "User" as default role
            _principal = new GenericPrincipal(userIdentity, new[] { AuthorizationRoles.User.ToString() });
            _authenticationAttempts = 0;        //reset attempts

            return true;
        }

        public bool ClearPrincipal()
        {
            if (!_principal!.Identity.IsAuthenticated) return false;

            _principal = null;

            return true;
        }

        public string GenerateOTP()
        {
            _OTP = new Random().Next(100000, 1000000).ToString("D6");

            //run a delay of 2 mins, and after reset OTP to null
            var t = Task.Run(() =>
            {
                Task.Delay(120000);

                _OTP = null;
            });

            return _OTP;
        }

        public bool ValidateOTP(string OTP)
        {
            if (_OTP is null || _OTP != OTP) return false;

            return true;
        }
    }
}
