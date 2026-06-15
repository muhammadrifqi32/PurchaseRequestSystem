using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Interfaces;

public interface IPurchaseOrderRepository
{
    IQueryable<PurchaseOrder> Query();
    Task<List<PurchaseOrder>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<PurchaseOrder?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PurchaseOrder?> GetByPurchaseRequestIdAsync(Guid purchaseRequestId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPurchaseRequestIdAsync(Guid purchaseRequestId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetPurchaseOrderNumbersByPeriodAsync(DateTime poDate, CancellationToken cancellationToken = default);
    Task AddAsync(PurchaseOrder entity, CancellationToken cancellationToken = default);
    void Update(PurchaseOrder entity);
    void Delete(PurchaseOrder entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
