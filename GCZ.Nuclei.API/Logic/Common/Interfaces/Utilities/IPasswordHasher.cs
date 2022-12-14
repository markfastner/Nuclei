namespace Logic.Common.Interfaces.Utilities
{
    public interface IPasswordHasher
    {
        public string Hash(string password);
        public bool Verify(string password, string hashedPassword);
    }
}
