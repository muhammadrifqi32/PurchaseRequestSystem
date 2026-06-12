using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Data;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;

namespace PurchaseRequestSystem.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public IQueryable<T> Query()
        => _dbSet.AsQueryable();

    public IQueryable<T> QueryActive()
        => _dbSet.AsQueryable().WhereNotDeleted();

    public Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => _dbSet.AsNoTracking().ToListAsync(cancellationToken);

    public Task<List<T>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        => QueryActive().AsNoTracking().ToListAsync(cancellationToken);

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync(new object[] { id }, cancellationToken);

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => _dbSet.AnyAsync(predicate, cancellationToken);

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => _dbSet.AddAsync(entity, cancellationToken).AsTask();

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Delete(T entity)
    {
        if (EntityQueryHelper.SupportsSoftDelete<T>())
        {
            EntityQueryHelper.SetSoftDeleted(entity);
            AuditHelper.SetUpdatedAudit(entity);
            _dbSet.Update(entity);
            return;
        }

        _dbSet.Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
