using Domain.Entities;

namespace Logic.Common.Interfaces.Persistence
{
    public interface IUserRepository
    {
        public Task<bool> InsertAsync(User user);
        public Task<User?> GetAsync(int id);
        public Task<IEnumerable<User>> GetAllAsync();
        public Task<bool> UpdateAsync(User user);
        public Task<bool> DeleteAsync(User user);
    }
}
