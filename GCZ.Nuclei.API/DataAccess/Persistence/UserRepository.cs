using Logic.Common.Interfaces.Persistence;

namespace DataAccess.Persistence
{
    public class UserRepository : IUserRepository
    {
        private readonly NucleiDbContext _dbContext;

        public UserRepository(NucleiDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> InsertAsync(User user)
        {
            _dbContext.User.Add(user);

            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<User?> GetAsync(int id)
        {
            return await _dbContext.User.FirstOrDefaultAsync(x => x.UserId == id);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _dbContext.User.ToListAsync();
        }

        public async Task<bool> UpdateAsync(User user)
        {
            _dbContext.User.Update(user);

            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(User user)
        {
            _dbContext.User.Remove(user);

            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
