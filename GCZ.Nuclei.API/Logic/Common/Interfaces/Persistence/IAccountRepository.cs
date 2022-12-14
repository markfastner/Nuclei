
namespace Logic.Common.Interfaces.Persistence
{
    public interface IAccountRepository
    {
        Task<Account?> GetAsync(int id, bool includeUser = false);
        Task<Account?> GetByEmailAsync(string email, bool includeUser = false);
        Task<bool> InsertAsync(Account account);
        Task<bool> DeleteAsync(Account account);
    }
}
