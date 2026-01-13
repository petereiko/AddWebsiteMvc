using AddWebsiteMvc.Business.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Linq.Expressions;

namespace AddWebsiteMvc.Business
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly VoteDbContext _context;
        private readonly DbSet<T> _dbSet;
        private IDbContextTransaction _transaction;

        public GenericRepository(VoteDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<List<T>> ExecuteRawQueryAsync<T>(string sql, Func<IDataReader, T> mapper)
        {
            var connection = 
                _context.Database.GetDbConnection();

            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            var results = new List<T>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(mapper(reader));
            }

            return results;
        }
        public async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IQueryable<T>> GetAllAsync(bool isTracking = true, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = isTracking ? _dbSet : _dbSet.AsNoTracking();
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return await Task.FromResult(query);
        }

        public async Task<IQueryable<T>> FilterAsync(Expression<Func<T, bool>> predicate, bool isTracking = true, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = isTracking ? _dbSet.Where(predicate) : _dbSet.AsNoTracking().Where(predicate);
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return await Task.FromResult(query);
        }

        public async Task<T> AddAsync(T entity, CancellationToken token = default)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync(token);
            return entity;
        }

        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken token = default)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync(token);
            return entities;
        }

        public async Task UpdateAsync(T entity, CancellationToken token = default)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync(token);
        }

        public async Task RemoveAsync(T entity, CancellationToken token = default)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(token);
        }

        public async Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken token = default)
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync(token);
        }

        public async Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate, bool isTracking = true, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = isTracking ? _dbSet.Where(predicate) : _dbSet.AsNoTracking().Where(predicate);
            if (includes != null)
            {

                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AsNoTracking().AnyAsync(predicate);
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
            }
        }


        #region Stored Procedures

        public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync(string storedProcedureName, params object[] parameters)
        {
            var sql = parameters.Count() == 0 ? storedProcedureName : BuildStoredProcedureCommand(storedProcedureName, parameters);

            return await _context.Set<T>().FromSqlRaw(sql, parameters).ToListAsync();
        }

        public async Task<int> ExecuteStoredProcedureNonQueryAsync(string storedProcedureName, params object[] parameters)
        {
            var sql = parameters.Count() == 0 ? storedProcedureName : BuildStoredProcedureCommand(storedProcedureName, parameters);
            return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        private static string BuildStoredProcedureCommand(string storedProcedureName, params object[] parameters)
        {
            // Generate SQL for the stored procedure call
            string builder = storedProcedureName;
            foreach (var parameter in (SqlParameter[])parameters)
            {
                builder += $" {parameter.ParameterName},";
            }
            if (builder.EndsWith(",")) builder = builder.Remove(builder.Length - 1);
            return builder;
        }

        public T? GetById(object id)
        {
            return _dbSet.Find(id);
        }

        public IQueryable<T> GetAll(bool isTracking = true, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = isTracking ? _dbSet : _dbSet.AsNoTracking();
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return query;
        }

        public IEnumerable<T> Filter(Expression<Func<T, bool>> predicate, bool isTracking = true, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = isTracking ? _dbSet.Where(predicate) : _dbSet.AsNoTracking().Where(predicate);
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return query.ToList();
        }

        public T? GetSingle(Expression<Func<T, bool>> predicate, bool isTracking = true, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = isTracking ? _dbSet.Where(predicate) : _dbSet.AsNoTracking().Where(predicate);
            if (includes != null)
            {

                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return query.FirstOrDefault();
        }

        public bool Any(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.AsNoTracking().Any(predicate);
        }

        public T Add(T entity)
        {
            _dbSet.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            _dbSet.AddRange(entities);
            _context.SaveChanges();
            return entities;
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
            _context.SaveChanges();
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            var entityList = entities.ToList();
            _dbSet.UpdateRange(entityList);
            await Task.CompletedTask;
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            _context.SaveChanges();
        }

        public void BeginTransaction()
        {
            _transaction = _context.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
            }
        }

        public void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
            }
        }

        public IEnumerable<T> ExecuteStoredProcedure(string storedProcedureName, params object[] parameters)
        {
            var sql = parameters.Count() == 0 ? storedProcedureName : BuildStoredProcedureCommand(storedProcedureName, parameters);

            return _context.Set<T>().FromSqlRaw(sql, parameters).ToList();
        }



        public int ExecuteStoredProcedureNonQuery(string storedProcedureName, params object[] parameters)
        {
            var sql = parameters.Count() == 0 ? storedProcedureName : BuildStoredProcedureCommand(storedProcedureName, parameters);
            return _context.Database.ExecuteSqlRaw(sql, parameters);
        }



        #endregion
    }
}
