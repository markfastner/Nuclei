using Logic.Common.Interfaces.Persistence.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Persistence.Common
{
    public class EFCoreLoggingRepository : IEFCoreLoggingRepository
    {
        private readonly NucleiDbContext _dbContext;

        public EFCoreLoggingRepository(NucleiDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> InsertAsync(Log log)
        {
            log.LogId = 0;

            _dbContext.Log.Add(log);
            
            return await _dbContext.SaveChangesAsync() > 0;
        }

    }
}
