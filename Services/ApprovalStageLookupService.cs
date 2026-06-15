using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Common.Exceptions;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Services;

public class ApprovalStageLookupService : IApprovalStageLookupService
{
    private readonly IGenericRepository<ApprovalStage> _approvalStageRepository;

    public ApprovalStageLookupService(IGenericRepository<ApprovalStage> approvalStageRepository)
    {
        _approvalStageRepository = approvalStageRepository;
    }

    public async Task<ApprovalStage> GetRequiredAsync(string stageName, CancellationToken cancellationToken = default)
    {
        var normalized = InputHelper.Required(stageName, nameof(stageName)).Trim().ToUpperInvariant();

        var stage = await _approvalStageRepository.QueryActive()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.StageName.ToUpper() == normalized, cancellationToken);

        return stage ?? throw new BadRequestException($"Approval stage '{stageName}' does not exist in master approval stage.");
    }
}
