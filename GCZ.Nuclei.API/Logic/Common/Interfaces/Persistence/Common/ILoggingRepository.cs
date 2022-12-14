using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Logic.Common.Interfaces.Persistence.Common
{
    public interface IEFCoreLoggingRepository
    {
        Task<bool> InsertAsync(Log log);
    }
}
