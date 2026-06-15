using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Interfaces;

public interface IApprovalStageLookupService
{
    Task<ApprovalStage> GetRequiredAsync(string stageName, CancellationToken cancellationToken = default);
}
