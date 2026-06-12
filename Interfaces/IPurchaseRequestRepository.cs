using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Interfaces;

public interface IPurchaseRequestRepository
{
    IQueryable<PurchaseRequest> Query();
    Task<PurchaseRequest?> GetPurchaseRequestWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<decimal> GetUsedQuantityByProposalDetailAsync(Guid proposalDetailId, Guid? excludedPurchaseRequestId = null, CancellationToken cancellationToken = default);
    Task<bool> CheckPurchaseRequestNoExistsAsync(string purchaseRequestNo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetPurchaseRequestNumbersByPeriodAsync(DateTime requestDate, CancellationToken cancellationToken = default);
    Task AddAsync(PurchaseRequest purchaseRequest, CancellationToken cancellationToken = default);
    void Update(PurchaseRequest purchaseRequest);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
