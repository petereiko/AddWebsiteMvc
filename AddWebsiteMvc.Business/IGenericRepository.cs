using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> ExecuteRawQueryAsync<T>(string sql, Func<IDataReader, T> mapper);
        Task<T?> GetByIdAsync(object id);
        T? GetById(object id);
        Task<IQueryable<T>> GetAllAsync(bool isTracking = true, params Expression<Func<T, object>>[] includes);
        IQueryable<T> GetAll(bool isTracking = true, params Expression<Func<T, object>>[] includes);
        Task<IQueryable<T>> FilterAsync(Expression<Func<T, bool>> predicate, bool isTracking = true, params Expression<Func<T, object>>[] includes);

        IEnumerable<T> Filter(Expression<Func<T, bool>> predicate, bool isTracking = true, params Expression<Func<T, object>>[] includes);
        Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate, bool isTracking = true, params Expression<Func<T, object>>[] includes);
        T? GetSingle(Expression<Func<T, bool>> predicate, bool isTracking = true, params Expression<Func<T, object>>[] includes);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        bool Any(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity, CancellationToken token = default);
        T Add(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken token = default);
        IEnumerable<T> AddRange(IEnumerable<T> entities);
        Task UpdateAsync(T entity, CancellationToken token = default);
        void Update(T entity);
        Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task RemoveAsync(T entity, CancellationToken token = default);
        void Remove(T entity);
        Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken token = default);
        void RemoveRange(IEnumerable<T> entities);
        Task BeginTransactionAsync();
        void BeginTransaction();
        Task CommitTransactionAsync();
        void CommitTransaction();
        Task RollbackTransactionAsync();
        void RollbackTransaction();

        #region Stored Procedures
        Task<IEnumerable<T>> ExecuteStoredProcedureAsync(string storedProcedureName, params object[] parameters);
        IEnumerable<T> ExecuteStoredProcedure(string storedProcedureName, params object[] parameters);
        Task<int> ExecuteStoredProcedureNonQueryAsync(string storedProcedureName, params object[] parameters);
        int ExecuteStoredProcedureNonQuery(string storedProcedureName, params object[] parameters);

        #endregion
    }
}
