using Logic.Common.Interfaces.Persistence;

namespace DataAccess.Persistence
{
    public class AccountRepository : IAccountRepository
    {
        private readonly NucleiDbContext _dbContext;

        public AccountRepository(NucleiDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Account?> GetAsync(int id, bool includeUser = false)
        {
            var result = _dbContext.Account
                .Where(x => x.AccountId == id);

            //if user should be included in the entity
            result = includeUser ?
                result.Include(x => x.User) :
                result;

            return await result.FirstOrDefaultAsync();
        }

        //queries database for data with matching email and joins associated user
        public async Task<Account?> GetByEmailAsync(string email, bool includeUser = false)
        {
            var result = _dbContext.Account
                .Where(x => x.Email == email);

            //if user should be included in the entity
            result = includeUser ?
                result.Include(x => x.User) :
                result;

            return await result.FirstOrDefaultAsync();
        }

        public async Task<bool> InsertAsync(Account account)
        {
            _dbContext.Account.Add(account);

            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Account account)
        {
            _dbContext.Account.Remove(account);

            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
