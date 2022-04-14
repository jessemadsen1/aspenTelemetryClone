using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aspen.Core.Models;

namespace Aspen.Core.Repositories
{
    public interface IThemeRepository
    {
        Task<Result<Theme>> Create(Theme theme, ConnectionString connectionString);
        // Task Delete(Guid charityId, ConnectionString connectionString);
        // Task<IEnumerable<Theme>> GetAll(ConnectionString connectionString);
        Task<Result<Theme>> GetByCharity(Charity charity);
        Task<Result<bool>> Update(Theme theme, ConnectionString connectionString);
    }
}