using System;
using System.Collections.Generic;
using System.Text;

namespace TradingDashboard.Core.Interfaces
{
    interface IRepository<T> where T : class
    {
        Task<T?> Get(Guid Id);
        Task<IEnumerable<T>> GetAll();
        Task AddEntity(T entity);
        Task Update(T entity);
        Task Delete(Guid Id);
        Task<bool> IsExisting(Guid Id);

    }
}
