using Logic.Common.Interfaces.Utilities;
using System.Security.Cryptography;
using System.Text;

namespace Services.Utilities
{
    //https://dotnetcodr.com/2017/10/26/how-to-hash-passwords-with-a-salt-in-net-2/
    //https://stackoverflow.com/questions/2138429/hash-and-salt-passwords-in-c-sharp
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltBits = 64;
        private const int Iterations = 100000;
        private const int OutputLength = 16;

        //generate salt of length 64
        private static byte[] GenerateSalt()
        {
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[SaltBits];
            rng.GetBytes(salt);

            return salt;
        }

        public string Hash(string password)
        {
            byte[] salt = GenerateSalt();
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(     //perform hashing
                password: password,
                salt: salt,
                hashAlgorithm: HashAlgorithmName.SHA256,
                iterations: Iterations,      //iterations make alg slower
                outputLength: OutputLength
            );

            return Convert.ToBase64String(hash) + Convert.ToBase64String(salt);        //converts hash + salt into a string
        }

        private static string Hash(string password, string salt)
        {
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(     //perform hashing
                password: password,
                salt: Convert.FromBase64String(salt),
                hashAlgorithm: HashAlgorithmName.SHA256,
                iterations: Iterations,      //iterations make alg slower
                outputLength: OutputLength
            );

            return Convert.ToBase64String(hash);        //converts bytes into string
        }

        public bool Verify(string password, string hashedPassword)
        {
            var salt = hashedPassword[(hashedPassword.IndexOf("=") + 2)..];     //retrieves salt by substringing stored hash
            var hash = Hash(password, salt);

            return hashedPassword.SequenceEqual(hash + salt);
        }
    }
}
