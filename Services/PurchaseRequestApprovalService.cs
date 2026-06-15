using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Common.Enums;
using PurchaseRequestSystem.Common.Exceptions;
using PurchaseRequestSystem.Data;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Services;

public class PurchaseRequestApprovalService : IPurchaseRequestApprovalService
{
    private static readonly string[] SubmittableStatuses = ["DRAFT", "REVISION_REQUIRED"];
    private static readonly string[] PendingGmStatuses = ["PENDING_GM_APPROVAL", "SUBMITTED", "PENDING"];
    private static readonly string[] GmApprovedWaitingChairmanStatuses = ["PENDING_CHAIRMAN_CONFIRMATION", "GM_APPROVED", "APPROVED"];

    private readonly AppDbContext _context;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IApprovalRecordRepository _approvalRecordRepository;
    private readonly IStatusLookupService _statusLookupService;
    private readonly IApprovalStageLookupService _approvalStageLookupService;
    private readonly IGenericRepository<Account> _accountRepository;

    public PurchaseRequestApprovalService(
        AppDbContext context,
        IPurchaseRequestRepository purchaseRequestRepository,
        IApprovalRecordRepository approvalRecordRepository,
        IStatusLookupService statusLookupService,
        IApprovalStageLookupService approvalStageLookupService,
        IGenericRepository<Account> accountRepository)
    {
        _context = context;
        _purchaseRequestRepository = purchaseRequestRepository;
        _approvalRecordRepository = approvalRecordRepository;
        _statusLookupService = statusLookupService;
        _approvalStageLookupService = approvalStageLookupService;
        _accountRepository = accountRepository;
    }

    public async Task<SubmitResponseDto> SubmitAsync(Guid purchaseRequestId, SubmitPurchaseRequestDto? dto, CancellationToken cancellationToken = default)
    {
        var purchaseRequest = await GetPurchaseRequestRequiredAsync(purchaseRequestId, cancellationToken);

        if (!purchaseRequest.Details.Any())
            throw new ValidationException("Purchase Request must have at least one detail before submit.");

        if (!await _statusLookupService.IsAnyAsync(purchaseRequest.StatusId, SubmittableStatuses, cancellationToken))
            throw new BusinessRuleException("Purchase Request is not eligible for submit. Only DRAFT or REVISION_REQUIRED purchase request can be submitted.");

        var submittedBy = await ResolveActorAsync(dto?.SubmittedBy ?? Guid.Empty, purchaseRequest.CreatedBy, "SubmittedBy", cancellationToken);
        var pendingGmStatus = await _statusLookupService.GetFirstExistingAsync(["PENDING_GM_APPROVAL", "SUBMITTED", "PENDING"], cancellationToken);
        var gmRecordStatus = await _statusLookupService.GetFirstExistingAsync(["PENDING_GM_APPROVAL", "PENDING", "SUBMITTED"], cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.UtcNow;
        purchaseRequest.StatusId = pendingGmStatus.StatusId;
        purchaseRequest.Notes = MergeNotes(purchaseRequest.Notes, dto?.Notes);
        purchaseRequest.UpdatedAt = now;
        purchaseRequest.UpdatedBy = submittedBy;

        purchaseRequest.ProcurementRequest.StatusId = pendingGmStatus.StatusId;
        purchaseRequest.ProcurementRequest.SubmittedAt = now;
        purchaseRequest.ProcurementRequest.UpdatedAt = now;
        purchaseRequest.ProcurementRequest.UpdatedBy = submittedBy;

        await UpsertApprovalRecordAsync(
            purchaseRequest.ProcurementRequestId,
            "GM",
            gmRecordStatus.StatusId,
            dto?.Notes,
            submittedBy,
            now,
            cancellationToken);

        await AddActivityLogAsync(
            submittedBy,
            DocumentType.PURCHASE_REQUEST,
            purchaseRequest.PurchaseRequestId,
            "SUBMIT_TO_GM",
            $"Purchase request {purchaseRequest.PurchaseRequestNo} submitted to GM approval.",
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ToSubmitResponse(purchaseRequest, pendingGmStatus);
    }

    public Task<SubmitResponseDto> ApproveByGmAsync(Guid purchaseRequestId, ApprovalActionDto dto, CancellationToken cancellationToken = default)
        => ProcessGmActionAsync(
            purchaseRequestId,
            dto,
            approvalRecordStatusCandidates: ["APPROVED"],
            targetStatusCandidates: ["PENDING_CHAIRMAN_CONFIRMATION", "GM_APPROVED", "APPROVED"],
            actionCode: "GM_APPROVE",
            successDescription: "Purchase request approved by GM and moved to Chairman confirmation.",
            requireNotes: false,
            cancellationToken);

    public Task<SubmitResponseDto> RejectByGmAsync(Guid purchaseRequestId, ApprovalActionDto dto, CancellationToken cancellationToken = default)
        => ProcessGmActionAsync(
            purchaseRequestId,
            dto,
            approvalRecordStatusCandidates: ["REJECTED"],
            targetStatusCandidates: ["REJECTED"],
            actionCode: "GM_REJECT",
            successDescription: "Purchase request rejected by GM.",
            requireNotes: true,
            cancellationToken);

    public Task<SubmitResponseDto> RequestRevisionByGmAsync(Guid purchaseRequestId, ApprovalActionDto dto, CancellationToken cancellationToken = default)
        => ProcessGmActionAsync(
            purchaseRequestId,
            dto,
            approvalRecordStatusCandidates: ["REVISION_REQUIRED"],
            targetStatusCandidates: ["REVISION_REQUIRED"],
            actionCode: "GM_REQUEST_REVISION",
            successDescription: "GM requested revision for purchase request.",
            requireNotes: true,
            cancellationToken);

    public Task<SubmitResponseDto> RecordChairmanApprovalAsync(Guid purchaseRequestId, ChairmanConfirmationDto dto, CancellationToken cancellationToken = default)
        => ProcessChairmanActionAsync(
            purchaseRequestId,
            dto,
            targetStatusCandidates: ["CHAIRMAN_APPROVED", "APPROVED"],
            finalStatusCandidates: ["APPROVED"],
            actionCode: "CHAIRMAN_APPROVE_RECORDED",
            successDescription: "Chairman approval recorded by GM.",
            cancellationToken);

    public Task<SubmitResponseDto> RecordChairmanRejectionAsync(Guid purchaseRequestId, ChairmanConfirmationDto dto, CancellationToken cancellationToken = default)
        => ProcessChairmanActionAsync(
            purchaseRequestId,
            dto,
            targetStatusCandidates: ["REJECTED"],
            finalStatusCandidates: ["REJECTED"],
            actionCode: "CHAIRMAN_REJECTION_RECORDED",
            successDescription: "Chairman rejection recorded by GM.",
            cancellationToken);

    public Task<SubmitResponseDto> RecordChairmanRevisionAsync(Guid purchaseRequestId, ChairmanConfirmationDto dto, CancellationToken cancellationToken = default)
        => ProcessChairmanActionAsync(
            purchaseRequestId,
            dto,
            targetStatusCandidates: ["REVISION_REQUIRED"],
            finalStatusCandidates: ["REVISION_REQUIRED"],
            actionCode: "CHAIRMAN_REVISION_RECORDED",
            successDescription: "Chairman revision request recorded by GM.",
            cancellationToken);

    public async Task<List<ApprovalHistoryResponseDto>> GetApprovalHistoryAsync(Guid purchaseRequestId, CancellationToken cancellationToken = default)
    {
        var purchaseRequest = await _purchaseRequestRepository.GetByIdWithProcurementRequestAsync(purchaseRequestId, cancellationToken)
            ?? throw new NotFoundException("Purchase Request not found");

        var records = await _approvalRecordRepository.GetByProcurementRequestIdAsync(purchaseRequest.ProcurementRequestId, cancellationToken);

        return records.Select(x => new ApprovalHistoryResponseDto
        {
            ApprovalRecordId = x.ApprovalRecordId,
            ProcurementRequestId = x.ProcurementRequestId,
            ApprovalStage = x.ApprovalStage?.StageName ?? string.Empty,
            Status = x.Status?.StatusName ?? string.Empty,
            Notes = x.Notes,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy
        }).ToList();
    }

    private async Task<SubmitResponseDto> ProcessGmActionAsync(
        Guid purchaseRequestId,
        ApprovalActionDto dto,
        string[] approvalRecordStatusCandidates,
        string[] targetStatusCandidates,
        string actionCode,
        string successDescription,
        bool requireNotes,
        CancellationToken cancellationToken)
    {
        if (dto is null)
            throw new ValidationException("Request body is required.");

        if (requireNotes)
            RequireNotes(dto.Notes, "Notes are required for this GM action.");

        var purchaseRequest = await GetPurchaseRequestRequiredAsync(purchaseRequestId, cancellationToken);

        if (!await _statusLookupService.IsAnyAsync(purchaseRequest.StatusId, PendingGmStatuses, cancellationToken))
            throw new BusinessRuleException("Purchase Request is not eligible for GM approval.");

        var actionBy = await ResolveActorAsync(dto.ActionBy, null, "ActionBy", cancellationToken);
        var approvalRecordStatus = await _statusLookupService.GetFirstExistingAsync(approvalRecordStatusCandidates, cancellationToken);
        var targetStatus = await _statusLookupService.GetFirstExistingAsync(targetStatusCandidates, cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.UtcNow;
        await UpsertApprovalRecordAsync(
            purchaseRequest.ProcurementRequestId,
            "GM",
            approvalRecordStatus.StatusId,
            dto.Notes,
            actionBy,
            now,
            cancellationToken);

        purchaseRequest.StatusId = targetStatus.StatusId;
        purchaseRequest.UpdatedAt = now;
        purchaseRequest.UpdatedBy = actionBy;

        purchaseRequest.ProcurementRequest.StatusId = targetStatus.StatusId;
        purchaseRequest.ProcurementRequest.UpdatedAt = now;
        purchaseRequest.ProcurementRequest.UpdatedBy = actionBy;

        await AddActivityLogAsync(
            actionBy,
            DocumentType.PURCHASE_REQUEST,
            purchaseRequest.PurchaseRequestId,
            actionCode,
            successDescription,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ToSubmitResponse(purchaseRequest, targetStatus);
    }

    private async Task<SubmitResponseDto> ProcessChairmanActionAsync(
        Guid purchaseRequestId,
        ChairmanConfirmationDto dto,
        string[] targetStatusCandidates,
        string[] finalStatusCandidates,
        string actionCode,
        string successDescription,
        CancellationToken cancellationToken)
    {
        if (dto is null)
            throw new ValidationException("Request body is required.");

        RequireNotes(dto.Notes, "Notes are required for Chairman confirmation because the decision is recorded from an offline approval.");

        var purchaseRequest = await GetPurchaseRequestRequiredAsync(purchaseRequestId, cancellationToken);

        if (!await _statusLookupService.IsAnyAsync(purchaseRequest.StatusId, GmApprovedWaitingChairmanStatuses, cancellationToken))
            throw new BusinessRuleException("Purchase Request is not eligible for Chairman confirmation.");

        var gmRecord = await _approvalRecordRepository.GetByProcurementRequestAndStageAsync(
            purchaseRequest.ProcurementRequestId,
            "GM",
            cancellationToken);

        if (gmRecord is null || !await _statusLookupService.IsAnyAsync(gmRecord.StatusId, ["APPROVED"], cancellationToken))
            throw new BusinessRuleException("GM approval must be APPROVED before Chairman confirmation can be recorded.");

        var recordedBy = await ResolveActorAsync(dto.RecordedBy, null, "RecordedBy", cancellationToken);
        var chairmanRecordStatus = await _statusLookupService.GetFirstExistingAsync(targetStatusCandidates, cancellationToken);
        var finalStatus = await _statusLookupService.GetFirstExistingAsync(finalStatusCandidates, cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.UtcNow;
        await UpsertApprovalRecordAsync(
            purchaseRequest.ProcurementRequestId,
            "Chairman",
            chairmanRecordStatus.StatusId,
            dto.Notes,
            recordedBy,
            now,
            cancellationToken);

        purchaseRequest.StatusId = finalStatus.StatusId;
        purchaseRequest.UpdatedAt = now;
        purchaseRequest.UpdatedBy = recordedBy;

        purchaseRequest.ProcurementRequest.StatusId = finalStatus.StatusId;
        purchaseRequest.ProcurementRequest.UpdatedAt = now;
        purchaseRequest.ProcurementRequest.UpdatedBy = recordedBy;

        await AddActivityLogAsync(
            recordedBy,
            DocumentType.PURCHASE_REQUEST,
            purchaseRequest.PurchaseRequestId,
            actionCode,
            successDescription,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ToSubmitResponse(purchaseRequest, finalStatus);
    }

    private async Task<PurchaseRequest> GetPurchaseRequestRequiredAsync(Guid purchaseRequestId, CancellationToken cancellationToken)
    {
        return await _purchaseRequestRepository.GetPurchaseRequestWithDetailsAsync(purchaseRequestId, cancellationToken)
            ?? throw new NotFoundException("Purchase Request not found");
    }

    private async Task<Guid> ResolveActorAsync(Guid providedActorId, Guid? fallbackActorId, string fieldName, CancellationToken cancellationToken)
    {
        var actorId = providedActorId != Guid.Empty ? providedActorId : fallbackActorId;

        if (!actorId.HasValue || actorId.Value == Guid.Empty)
            throw new ValidationException($"{fieldName} is required.");

        var exists = await _accountRepository.QueryActive()
            .AsNoTracking()
            .AnyAsync(x => x.AccountId == actorId.Value, cancellationToken);

        if (!exists)
            throw new ValidationException($"Account '{actorId.Value}' does not exist.");

        return actorId.Value;
    }

    private static void RequireNotes(string? notes, string message)
    {
        if (string.IsNullOrWhiteSpace(notes))
            throw new ValidationException(message);
    }

    private async Task UpsertApprovalRecordAsync(
        Guid procurementRequestId,
        string stageName,
        Guid statusId,
        string? notes,
        Guid actorId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var stage = await _approvalStageLookupService.GetRequiredAsync(stageName, cancellationToken);
        var record = await _approvalRecordRepository.GetByProcurementRequestAndStageAsync(procurementRequestId, stage.StageName, cancellationToken);

        if (record is null)
        {
            await _approvalRecordRepository.AddAsync(new ApprovalRecord
            {
                ApprovalRecordId = Guid.NewGuid(),
                ProcurementRequestId = procurementRequestId,
                ApprovalStageId = stage.ApprovalStageId,
                StatusId = statusId,
                Notes = notes,
                CreatedAt = now,
                CreatedBy = actorId
            }, cancellationToken);
            return;
        }

        record.StatusId = statusId;
        record.Notes = notes;
        record.UpdatedAt = now;
        record.UpdatedBy = actorId;
        await _approvalRecordRepository.UpdateAsync(record, cancellationToken);
    }

    private async Task AddActivityLogAsync(
        Guid accountId,
        DocumentType documentType,
        Guid documentId,
        string action,
        string? description,
        CancellationToken cancellationToken)
    {
        var accountExists = await _accountRepository.QueryActive()
            .AsNoTracking()
            .AnyAsync(x => x.AccountId == accountId, cancellationToken);

        if (!accountExists)
            return;

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
    }

    private static string? MergeNotes(string? existingNotes, string? newNotes)
    {
        if (string.IsNullOrWhiteSpace(newNotes))
            return existingNotes;

        if (string.IsNullOrWhiteSpace(existingNotes))
            return newNotes.Trim();

        return $"{existingNotes}\n{newNotes.Trim()}";
    }

    private static SubmitResponseDto ToSubmitResponse(PurchaseRequest purchaseRequest, Status status)
    {
        return new SubmitResponseDto
        {
            Id = purchaseRequest.PurchaseRequestId,
            DocumentNo = purchaseRequest.PurchaseRequestNo ?? string.Empty,
            StatusId = status.StatusId,
            StatusName = status.StatusName,
            SubmittedAt = purchaseRequest.ProcurementRequest?.SubmittedAt
        };
    }
}
