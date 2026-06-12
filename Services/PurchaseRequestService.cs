using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Common.Enums;
using PurchaseRequestSystem.Common.Exceptions;
using PurchaseRequestSystem.Data;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Services;

public class PurchaseRequestService : IPurchaseRequestService
{
    private static readonly string[] SubmittableStatuses = ["DRAFT", "REVISION_REQUIRED"];
    private readonly AppDbContext _context;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IStatusLookupService _statusLookupService;

    public PurchaseRequestService(
        AppDbContext context,
        IPurchaseRequestRepository purchaseRequestRepository,
        IGenericRepository<Account> accountRepository,
        IStatusLookupService statusLookupService)
    {
        _context = context;
        _purchaseRequestRepository = purchaseRequestRepository;
        _accountRepository = accountRepository;
        _statusLookupService = statusLookupService;
    }

    public async Task<PurchaseRequestResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _purchaseRequestRepository.GetPurchaseRequestWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("PurchaseRequest", id);

        return ToResponseDto(entity);
    }

    public async Task<SubmitResponseDto> SubmitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var purchaseRequest = await _purchaseRequestRepository.GetPurchaseRequestWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("PurchaseRequest", id);

        if (!purchaseRequest.Details.Any())
            throw new ValidationException("Purchase Request must have at least one detail before submit.");

        if (!await _statusLookupService.IsAnyAsync(purchaseRequest.StatusId, SubmittableStatuses, cancellationToken))
            throw new ValidationException("Only DRAFT or REVISION_REQUIRED purchase request can be submitted.");

        var submittedStatus = await _statusLookupService.GetFirstExistingAsync(
            ["SUBMITTED", "PENDING_GM_APPROVAL", "PENDING"], cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        purchaseRequest.StatusId = submittedStatus.StatusId;
        purchaseRequest.UpdatedAt = DateTime.UtcNow;
        purchaseRequest.UpdatedBy = purchaseRequest.CreatedBy;

        purchaseRequest.ProcurementRequest.StatusId = submittedStatus.StatusId;
        purchaseRequest.ProcurementRequest.SubmittedAt = DateTime.UtcNow;
        purchaseRequest.ProcurementRequest.UpdatedAt = DateTime.UtcNow;
        purchaseRequest.ProcurementRequest.UpdatedBy = purchaseRequest.CreatedBy;

        _purchaseRequestRepository.Update(purchaseRequest);
        await _purchaseRequestRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        if (purchaseRequest.CreatedBy.HasValue)
        {
            await AddActivityLogIfPossibleAsync(purchaseRequest.CreatedBy.Value, DocumentType.PURCHASE_REQUEST, purchaseRequest.PurchaseRequestId, "SUBMIT", "Purchase request submitted.", cancellationToken);
        }

        return new SubmitResponseDto
        {
            Id = purchaseRequest.PurchaseRequestId,
            DocumentNo = purchaseRequest.PurchaseRequestNo ?? string.Empty,
            StatusId = submittedStatus.StatusId,
            StatusName = submittedStatus.StatusName,
            SubmittedAt = purchaseRequest.ProcurementRequest.SubmittedAt
        };
    }

    private async Task AddActivityLogIfPossibleAsync(Guid accountId, DocumentType documentType, Guid documentId, string action, string? description, CancellationToken cancellationToken)
    {
        var accountExists = await _accountRepository.QueryActive().AnyAsync(x => x.AccountId == accountId, cancellationToken);
        if (!accountExists) return;

        _context.ActivityLogs.Add(new ActivityLog
        {
            ActivityLogId = Guid.NewGuid(),
            AccountId = accountId,
            DocumentType = documentType,
            DocumentId = documentId,
            Action = action,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static PurchaseRequestResponseDto ToResponseDto(PurchaseRequest entity)
    {
        return new PurchaseRequestResponseDto
        {
            PurchaseRequestId = entity.PurchaseRequestId,
            PurchaseRequestNo = entity.PurchaseRequestNo ?? string.Empty,
            ProcurementRequestId = entity.ProcurementRequestId,
            ProcurementRequestNo = entity.ProcurementRequest?.ProcurementRequestNo,
            StatusId = entity.StatusId,
            StatusName = entity.Status?.StatusName ?? string.Empty,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
            Details = entity.Details.OrderBy(x => x.CreatedAt).Select(detail => new PurchaseRequestDetailResponseDto
            {
                PurchaseRequestDetailId = detail.PurchaseRequestDetailId,
                PurchaseRequestId = detail.PurchaseRequestId,
                ProposalDetailId = detail.ProposalDetailId,
                MaterialId = detail.MaterialId,
                MaterialCode = detail.Material?.MaterialCode,
                MaterialName = detail.Material?.MaterialName,
                UomId = detail.UomId,
                UomCode = detail.Uom?.UomCode,
                UomName = detail.Uom?.UomName,
                Description = detail.Description,
                Quantity = detail.Quantity,
                Notes = detail.Notes,
                CreatedAt = detail.CreatedAt,
                CreatedBy = detail.CreatedBy,
                UpdatedAt = detail.UpdatedAt,
                UpdatedBy = detail.UpdatedBy
            }).ToList()
        };
    }
}
