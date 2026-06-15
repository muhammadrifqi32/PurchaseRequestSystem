using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Data;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Repositories;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly AppDbContext _context;

    public PurchaseOrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<PurchaseOrder> Query()
        => _context.PurchaseOrders.AsQueryable();

    public async Task<List<PurchaseOrder>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await WithDetails()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PurchaseOrder?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await WithDetails()
            .FirstOrDefaultAsync(x => x.PurchaseOrderId == id, cancellationToken);
    }

    public async Task<PurchaseOrder?> GetByPurchaseRequestIdAsync(Guid purchaseRequestId, CancellationToken cancellationToken = default)
    {
        return await WithDetails()
            .FirstOrDefaultAsync(x => x.PurchaseRequestId == purchaseRequestId, cancellationToken);
    }

    public Task<bool> ExistsByPurchaseRequestIdAsync(Guid purchaseRequestId, CancellationToken cancellationToken = default)
    {
        return _context.PurchaseOrders.AnyAsync(x => x.PurchaseRequestId == purchaseRequestId, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetPurchaseOrderNumbersByPeriodAsync(DateTime poDate, CancellationToken cancellationToken = default)
    {
        var prefix = DocumentNumberGenerator.GetPeriodPrefix("PO", poDate);
        return await _context.PurchaseOrders
            .AsNoTracking()
            .Where(x => x.PurchaseOrderNo.StartsWith(prefix))
            .Select(x => x.PurchaseOrderNo)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(PurchaseOrder entity, CancellationToken cancellationToken = default)
        => _context.PurchaseOrders.AddAsync(entity, cancellationToken).AsTask();

    public void Update(PurchaseOrder entity)
        => _context.PurchaseOrders.Update(entity);

    public void Delete(PurchaseOrder entity)
    {
        _context.PurchaseOrderDetails.RemoveRange(entity.Details);
        _context.PurchaseOrders.Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    private IQueryable<PurchaseOrder> WithDetails()
    {
        return _context.PurchaseOrders
            .Include(x => x.Status)
            .Include(x => x.Vendor)
            .Include(x => x.Company)
            .Include(x => x.PurchaseRequest).ThenInclude(x => x.Status)
            .Include(x => x.PurchaseRequest).ThenInclude(x => x.ProcurementRequest)
            .Include(x => x.Details).ThenInclude(x => x.Material)
            .Include(x => x.Details).ThenInclude(x => x.Uom);
    }
}
