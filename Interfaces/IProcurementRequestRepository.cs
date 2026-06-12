using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Interfaces;

public interface IProcurementRequestRepository
{
    IQueryable<ProcurementRequest> Query();
    Task<IReadOnlyList<ProcurementRequest>> GetAllWithPurchaseRequestAsync(CancellationToken cancellationToken = default);
    Task<ProcurementRequest?> GetProcurementRequestWithPurchaseRequestAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CheckProcurementRequestNoExistsAsync(string procurementRequestNo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetProcurementRequestNumbersByPeriodAsync(DateTime requestDate, CancellationToken cancellationToken = default);
    Task AddAsync(ProcurementRequest procurementRequest, CancellationToken cancellationToken = default);
    void Update(ProcurementRequest procurementRequest);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
