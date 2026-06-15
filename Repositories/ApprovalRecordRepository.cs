using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Data;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Repositories;

public class ApprovalRecordRepository : IApprovalRecordRepository
{
    private readonly AppDbContext _context;

    public ApprovalRecordRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<ApprovalRecord> Query()
        => _context.ApprovalRecords.AsQueryable();

    public async Task<ApprovalRecord?> GetByProcurementRequestAndStageAsync(Guid procurementRequestId, string stageName, CancellationToken cancellationToken = default)
    {
        var normalized = stageName.Trim().ToUpperInvariant();

        return await Query()
            .Include(x => x.ApprovalStage)
            .Include(x => x.Status)
            .FirstOrDefaultAsync(x =>
                x.ProcurementRequestId == procurementRequestId &&
                x.ApprovalStage.StageName.ToUpper() == normalized,
                cancellationToken);
    }

    public async Task<List<ApprovalRecord>> GetByProcurementRequestIdAsync(Guid procurementRequestId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .AsNoTracking()
            .Include(x => x.ApprovalStage)
            .Include(x => x.Status)
            .Where(x => x.ProcurementRequestId == procurementRequestId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(ApprovalRecord entity, CancellationToken cancellationToken = default)
        => _context.ApprovalRecords.AddAsync(entity, cancellationToken).AsTask();

    public Task UpdateAsync(ApprovalRecord entity, CancellationToken cancellationToken = default)
    {
        _context.ApprovalRecords.Update(entity);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
