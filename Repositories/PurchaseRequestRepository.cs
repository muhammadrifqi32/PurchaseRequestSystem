using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Data;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Repositories;

public class PurchaseRequestRepository : IPurchaseRequestRepository
{
    private readonly AppDbContext _context;

    public PurchaseRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<PurchaseRequest> Query()
        => _context.PurchaseRequests.AsQueryable();

    public async Task<PurchaseRequest?> GetPurchaseRequestWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Include(x => x.Status)
            .Include(x => x.ProcurementRequest).ThenInclude(x => x.Status)
            .Include(x => x.ProcurementRequest).ThenInclude(x => x.RequestType)
            .Include(x => x.ProcurementRequest).ThenInclude(x => x.Proposal)
            .Include(x => x.Details).ThenInclude(x => x.Material)
            .Include(x => x.Details).ThenInclude(x => x.Uom)
            .Include(x => x.Details).ThenInclude(x => x.ProposalDetail)
            .FirstOrDefaultAsync(x => x.PurchaseRequestId == id, cancellationToken);
    }

    public async Task<decimal> GetUsedQuantityByProposalDetailAsync(Guid proposalDetailId, Guid? excludedPurchaseRequestId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.PurchaseRequestDetails
            .AsNoTracking()
            .Where(x => x.ProposalDetailId == proposalDetailId);

        if (excludedPurchaseRequestId.HasValue)
            query = query.Where(x => x.PurchaseRequestId != excludedPurchaseRequestId.Value);

        return await query.SumAsync(x => x.Quantity, cancellationToken);
    }

    public Task<bool> CheckPurchaseRequestNoExistsAsync(string purchaseRequestNo, CancellationToken cancellationToken = default)
        => Query().AnyAsync(x => x.PurchaseRequestNo == purchaseRequestNo, cancellationToken);

    public async Task<IReadOnlyList<string>> GetPurchaseRequestNumbersByPeriodAsync(DateTime requestDate, CancellationToken cancellationToken = default)
    {
        var prefix = DocumentNumberGenerator.GetPeriodPrefix("PR", requestDate);
        return await Query()
            .AsNoTracking()
            .Where(x => x.PurchaseRequestNo != null && x.PurchaseRequestNo.StartsWith(prefix))
            .Select(x => x.PurchaseRequestNo!)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(PurchaseRequest purchaseRequest, CancellationToken cancellationToken = default)
        => _context.PurchaseRequests.AddAsync(purchaseRequest, cancellationToken).AsTask();

    public void Update(PurchaseRequest purchaseRequest)
        => _context.PurchaseRequests.Update(purchaseRequest);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
