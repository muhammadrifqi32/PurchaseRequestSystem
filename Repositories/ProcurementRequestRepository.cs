using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Data;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Repositories;

public class ProcurementRequestRepository : IProcurementRequestRepository
{
    private readonly AppDbContext _context;

    public ProcurementRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<ProcurementRequest> Query()
        => _context.ProcurementRequests.AsQueryable();

    public async Task<IReadOnlyList<ProcurementRequest>> GetAllWithPurchaseRequestAsync(CancellationToken cancellationToken = default)
    {
        return await Query()
            .AsNoTracking()
            .Include(x => x.RequestType)
            .Include(x => x.Status)
            .Include(x => x.Proposal)
            .Include(x => x.PurchaseRequest).ThenInclude(x => x!.Status)
            .Include(x => x.PurchaseRequest).ThenInclude(x => x!.Details).ThenInclude(x => x.Material)
            .Include(x => x.PurchaseRequest).ThenInclude(x => x!.Details).ThenInclude(x => x.Uom)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProcurementRequest?> GetProcurementRequestWithPurchaseRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Include(x => x.RequestType)
            .Include(x => x.Status)
            .Include(x => x.Proposal)
            .Include(x => x.PurchaseRequest).ThenInclude(x => x!.Status)
            .Include(x => x.PurchaseRequest).ThenInclude(x => x!.Details).ThenInclude(x => x.Material)
            .Include(x => x.PurchaseRequest).ThenInclude(x => x!.Details).ThenInclude(x => x.Uom)
            .FirstOrDefaultAsync(x => x.ProcurementRequestId == id, cancellationToken);
    }

    public Task<bool> CheckProcurementRequestNoExistsAsync(string procurementRequestNo, CancellationToken cancellationToken = default)
        => Query().AnyAsync(x => x.ProcurementRequestNo == procurementRequestNo, cancellationToken);

    public async Task<IReadOnlyList<string>> GetProcurementRequestNumbersByPeriodAsync(DateTime requestDate, CancellationToken cancellationToken = default)
    {
        var prefix = DocumentNumberGenerator.GetPeriodPrefix("PRQ", requestDate);
        return await Query()
            .AsNoTracking()
            .Where(x => x.ProcurementRequestNo.StartsWith(prefix))
            .Select(x => x.ProcurementRequestNo)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(ProcurementRequest procurementRequest, CancellationToken cancellationToken = default)
        => _context.ProcurementRequests.AddAsync(procurementRequest, cancellationToken).AsTask();

    public void Update(ProcurementRequest procurementRequest)
        => _context.ProcurementRequests.Update(procurementRequest);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
