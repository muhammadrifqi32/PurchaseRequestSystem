using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Data;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Repositories;

public class ProposalRepository : IProposalRepository
{
    private readonly AppDbContext _context;

    public ProposalRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Proposal> Query()
        => _context.Proposals.AsQueryable();

    public async Task<IReadOnlyList<Proposal>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await Query()
            .AsNoTracking()
            .Include(x => x.Status)
            .Include(x => x.Details).ThenInclude(x => x.Material)
            .Include(x => x.Details).ThenInclude(x => x.Uom)
            .Include(x => x.Details).ThenInclude(x => x.Status)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Proposal?> GetProposalWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Include(x => x.Status)
            .Include(x => x.Details).ThenInclude(x => x.Material)
            .Include(x => x.Details).ThenInclude(x => x.Uom)
            .Include(x => x.Details).ThenInclude(x => x.Status)
            .Include(x => x.ProcurementRequests)
            .FirstOrDefaultAsync(x => x.ProposalId == id, cancellationToken);
    }

    public Task<bool> CheckProposalNoExistsAsync(string proposalNo, CancellationToken cancellationToken = default)
        => Query().AnyAsync(x => x.ProposalNo == proposalNo, cancellationToken);

    public async Task<IReadOnlyList<string>> GetProposalNumbersByPeriodAsync(DateTime proposalDate, CancellationToken cancellationToken = default)
    {
        var prefix = DocumentNumberGenerator.GetPeriodPrefix("PROP", proposalDate);
        return await Query()
            .AsNoTracking()
            .Where(x => x.ProposalNo.StartsWith(prefix))
            .Select(x => x.ProposalNo)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Proposal proposal, CancellationToken cancellationToken = default)
        => _context.Proposals.AddAsync(proposal, cancellationToken).AsTask();

    public void Update(Proposal proposal)
        => _context.Proposals.Update(proposal);

    public void Delete(Proposal proposal)
        => _context.Proposals.Remove(proposal);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
