using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Interfaces;

public interface IProposalRepository
{
    IQueryable<Proposal> Query();
    Task<IReadOnlyList<Proposal>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<Proposal?> GetProposalWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CheckProposalNoExistsAsync(string proposalNo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetProposalNumbersByPeriodAsync(DateTime proposalDate, CancellationToken cancellationToken = default);
    Task AddAsync(Proposal proposal, CancellationToken cancellationToken = default);
    void Update(Proposal proposal);
    void Delete(Proposal proposal);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
