using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Common.Enums;
using PurchaseRequestSystem.Common.Exceptions;
using PurchaseRequestSystem.Data;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Services;

public class ProposalService : IProposalService
{
    private static readonly string[] EditableStatuses = ["DRAFT", "REVISION_REQUIRED"];
    private readonly AppDbContext _context;
    private readonly IProposalRepository _proposalRepository;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IGenericRepository<Material> _materialRepository;
    private readonly IGenericRepository<Uom> _uomRepository;
    private readonly IStatusLookupService _statusLookupService;

    public ProposalService(
        AppDbContext context,
        IProposalRepository proposalRepository,
        IPurchaseRequestRepository purchaseRequestRepository,
        IGenericRepository<Account> accountRepository,
        IGenericRepository<Material> materialRepository,
        IGenericRepository<Uom> uomRepository,
        IStatusLookupService statusLookupService)
    {
        _context = context;
        _proposalRepository = proposalRepository;
        _purchaseRequestRepository = purchaseRequestRepository;
        _accountRepository = accountRepository;
        _materialRepository = materialRepository;
        _uomRepository = uomRepository;
        _statusLookupService = statusLookupService;
    }

    public async Task<IReadOnlyList<ProposalResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var proposals = await _proposalRepository.GetAllWithDetailsAsync(cancellationToken);
        var response = new List<ProposalResponseDto>();

        foreach (var proposal in proposals)
            response.Add(await ToResponseDtoAsync(proposal, cancellationToken));

        return response;
    }

    public async Task<ProposalResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var proposal = await _proposalRepository.GetProposalWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Proposal", id);

        return await ToResponseDtoAsync(proposal, cancellationToken);
    }

    public async Task<ProposalResponseDto> CreateAsync(CreateProposalDto dto, CancellationToken cancellationToken = default)
    {
        ValidateCreateProposal(dto);
        await ValidateRequesterAsync(dto.RequesterId, cancellationToken);
        await ValidateProposalDetailsAsync(dto.Details, cancellationToken);

        var draftStatus = await _statusLookupService.GetRequiredAsync("DRAFT", cancellationToken);
        var proposalNo = await GenerateProposalNoAsync(dto.ProposalDate, cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var proposal = new Proposal
        {
            ProposalId = Guid.NewGuid(),
            ProposalNo = proposalNo,
            RequesterId = dto.RequesterId,
            ProposalDate = dto.ProposalDate.Date,
            Purpose = dto.Purpose?.Trim(),
            StatusId = draftStatus.StatusId,
            ProposalAttachmentPath = dto.ProposalAttachmentPath?.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = dto.RequesterId,
            Details = dto.Details.Select(detail => new ProposalDetail
            {
                ProposalDetailId = Guid.NewGuid(),
                MaterialId = detail.MaterialId,
                UomId = detail.UomId,
                Description = detail.Description?.Trim(),
                RequestedQty = detail.RequestedQty,
                ApprovedQty = 0,
                StatusId = draftStatus.StatusId,
                CreatedAt = DateTime.UtcNow
            }).ToList()
        };

        await _proposalRepository.AddAsync(proposal, cancellationToken);
        await _proposalRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var saved = await _proposalRepository.GetProposalWithDetailsAsync(proposal.ProposalId, cancellationToken)
            ?? proposal;

        return await ToResponseDtoAsync(saved, cancellationToken);
    }

    public async Task<ProposalResponseDto> UpdateAsync(Guid id, UpdateProposalDto dto, CancellationToken cancellationToken = default)
    {
        ValidateUpdateProposal(dto);
        await ValidateRequesterAsync(dto.RequesterId, cancellationToken);
        await ValidateProposalDetailsAsync(dto.Details.Select(x => new CreateProposalDetailDto
        {
            MaterialId = x.MaterialId,
            UomId = x.UomId,
            Description = x.Description,
            RequestedQty = x.RequestedQty
        }).ToList(), cancellationToken);

        var proposal = await _proposalRepository.GetProposalWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Proposal", id);

        await EnsureStatusAllowedAsync(proposal.StatusId, EditableStatuses, "Only DRAFT or REVISION_REQUIRED proposal can be updated.", cancellationToken);

        if (proposal.ProcurementRequests.Any())
            throw new ValidationException("Proposal that has already been used by a procurement request cannot be updated.");

        var draftStatus = await _statusLookupService.GetRequiredAsync("DRAFT", cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        proposal.RequesterId = dto.RequesterId;
        proposal.ProposalDate = dto.ProposalDate.Date;
        proposal.Purpose = dto.Purpose?.Trim();
        proposal.ProposalAttachmentPath = dto.ProposalAttachmentPath?.Trim();
        proposal.StatusId = draftStatus.StatusId;
        proposal.UpdatedAt = DateTime.UtcNow;
        proposal.UpdatedBy = dto.RequesterId;

        _context.ProposalDetails.RemoveRange(proposal.Details);
        proposal.Details = dto.Details.Select(detail => new ProposalDetail
        {
            ProposalDetailId = detail.ProposalDetailId.GetValueOrDefault(Guid.NewGuid()),
            ProposalId = proposal.ProposalId,
            MaterialId = detail.MaterialId,
            UomId = detail.UomId,
            Description = detail.Description?.Trim(),
            RequestedQty = detail.RequestedQty,
            ApprovedQty = 0,
            StatusId = draftStatus.StatusId,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _proposalRepository.Update(proposal);
        await _proposalRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var saved = await _proposalRepository.GetProposalWithDetailsAsync(id, cancellationToken)
            ?? proposal;

        return await ToResponseDtoAsync(saved, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var proposal = await _proposalRepository.GetProposalWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Proposal", id);

        if (proposal.ProcurementRequests.Any())
            throw new ValidationException("Proposal cannot be deleted because it is already used by procurement request.");

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        _context.ProposalDetails.RemoveRange(proposal.Details);
        _proposalRepository.Delete(proposal);
        await _proposalRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<SubmitResponseDto> SubmitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var proposal = await _proposalRepository.GetProposalWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Proposal", id);

        if (!proposal.Details.Any())
            throw new ValidationException("Proposal must have at least one detail before submit.");

        await EnsureStatusAllowedAsync(proposal.StatusId, EditableStatuses, "Only DRAFT or REVISION_REQUIRED proposal can be submitted.", cancellationToken);

        var submittedStatus = await _statusLookupService.GetFirstExistingAsync(
            ["SUBMITTED", "PENDING_REVIEW", "UNDER_REVIEW", "PENDING"], cancellationToken);

        proposal.StatusId = submittedStatus.StatusId;
        proposal.SubmittedAt = DateTime.UtcNow;
        proposal.UpdatedAt = DateTime.UtcNow;
        proposal.UpdatedBy = proposal.RequesterId;

        _proposalRepository.Update(proposal);
        await _proposalRepository.SaveChangesAsync(cancellationToken);

        await AddActivityLogIfPossibleAsync(proposal.RequesterId, DocumentType.PROPOSAL, proposal.ProposalId, "SUBMIT", "Proposal submitted.", cancellationToken);

        return new SubmitResponseDto
        {
            Id = proposal.ProposalId,
            DocumentNo = proposal.ProposalNo,
            StatusId = submittedStatus.StatusId,
            StatusName = submittedStatus.StatusName,
            SubmittedAt = proposal.SubmittedAt
        };
    }

    public async Task<ProposalResponseDto> ReviewAsync(Guid id, ReviewProposalDto dto, CancellationToken cancellationToken = default)
    {
        var proposal = await _proposalRepository.GetProposalWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Proposal", id);

        await EnsureStatusAllowedAsync(proposal.StatusId, ["SUBMITTED", "PENDING", "UNDER_REVIEW"], "Only SUBMITTED, PENDING, or UNDER_REVIEW proposal can be reviewed.", cancellationToken);
        ValidateReviewProposal(dto, proposal);

        var requestedReviewStatus = InputHelper.Required(dto.Status, nameof(dto.Status)).ToUpperInvariant();
        var approvedStatus = await _statusLookupService.GetRequiredAsync("APPROVED", cancellationToken);
        var rejectedStatus = await _statusLookupService.GetRequiredAsync("REJECTED", cancellationToken);
        var revisionStatus = await _statusLookupService.GetRequiredAsync("REVISION_REQUIRED", cancellationToken);
        var partialStatus = await _statusLookupService.GetFirstExistingAsync(["PARTIALLY_APPROVED", "APPROVED"], cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        foreach (var reviewDetail in dto.Details)
        {
            var detail = proposal.Details.First(x => x.ProposalDetailId == reviewDetail.ProposalDetailId);
            var detailStatusName = InputHelper.Required(reviewDetail.Status, nameof(reviewDetail.Status)).ToUpperInvariant();

            detail.ApprovedQty = detailStatusName == "REJECTED" ? 0 : reviewDetail.ApprovedQty;
            detail.StatusId = detailStatusName switch
            {
                "APPROVED" => approvedStatus.StatusId,
                "PARTIALLY_APPROVED" => partialStatus.StatusId,
                "REJECTED" => rejectedStatus.StatusId,
                _ => detail.StatusId
            };
            detail.UpdatedAt = DateTime.UtcNow;
        }

        var finalStatus = requestedReviewStatus switch
        {
            "REVISION_REQUIRED" => revisionStatus,
            "REJECTED" => rejectedStatus,
            _ => proposal.Details.Any(x => x.ApprovedQty > 0) ? approvedStatus : rejectedStatus
        };

        proposal.StatusId = finalStatus.StatusId;
        proposal.UpdatedAt = DateTime.UtcNow;
        proposal.UpdatedBy = dto.ReviewedBy;

        _proposalRepository.Update(proposal);
        await _proposalRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        if (dto.ReviewedBy.HasValue)
        {
            await AddActivityLogIfPossibleAsync(dto.ReviewedBy.Value, DocumentType.PROPOSAL, proposal.ProposalId, "REVIEW", dto.Notes ?? $"Proposal reviewed as {finalStatus.StatusName}.", cancellationToken);
        }

        var saved = await _proposalRepository.GetProposalWithDetailsAsync(id, cancellationToken)
            ?? proposal;

        return await ToResponseDtoAsync(saved, cancellationToken);
    }

    private async Task<string> GenerateProposalNoAsync(DateTime proposalDate, CancellationToken cancellationToken)
    {
        var existingNumbers = await _proposalRepository.GetProposalNumbersByPeriodAsync(proposalDate, cancellationToken);
        var proposalNo = DocumentNumberGenerator.Generate("PROP", proposalDate, existingNumbers);

        while (await _proposalRepository.CheckProposalNoExistsAsync(proposalNo, cancellationToken))
        {
            existingNumbers = existingNumbers.Append(proposalNo).ToList();
            proposalNo = DocumentNumberGenerator.Generate("PROP", proposalDate, existingNumbers);
        }

        return proposalNo;
    }

    private static void ValidateCreateProposal(CreateProposalDto dto)
    {
        var errors = new List<string>();
        if (dto.RequesterId == Guid.Empty) errors.Add("RequesterId is required.");
        ValidationHelper.EnsureDate(dto.ProposalDate, nameof(dto.ProposalDate), errors);
        if (dto.Details is null || dto.Details.Count == 0) errors.Add("Proposal must have at least one detail.");
        ValidationHelper.ThrowIfAny(errors);
    }

    private static void ValidateUpdateProposal(UpdateProposalDto dto)
    {
        var errors = new List<string>();
        if (dto.RequesterId == Guid.Empty) errors.Add("RequesterId is required.");
        ValidationHelper.EnsureDate(dto.ProposalDate, nameof(dto.ProposalDate), errors);
        if (dto.Details is null || dto.Details.Count == 0) errors.Add("Proposal must have at least one detail.");
        ValidationHelper.ThrowIfAny(errors);
    }

    private async Task ValidateProposalDetailsAsync(IReadOnlyList<CreateProposalDetailDto> details, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        for (var index = 0; index < details.Count; index++)
        {
            var detail = details[index];
            var row = index + 1;

            if (detail.MaterialId == Guid.Empty) errors.Add($"Details[{row}].MaterialId is required.");
            if (detail.UomId == Guid.Empty) errors.Add($"Details[{row}].UomId is required.");
            ValidationHelper.EnsurePositive(detail.RequestedQty, $"Details[{row}].RequestedQty", errors);

            if (detail.MaterialId != Guid.Empty && !await _materialRepository.Query().AnyAsync(x => x.MaterialId == detail.MaterialId, cancellationToken))
                errors.Add($"Details[{row}].Material does not exist.");

            if (detail.UomId != Guid.Empty && !await _uomRepository.QueryActive().AnyAsync(x => x.UomId == detail.UomId, cancellationToken))
                errors.Add($"Details[{row}].UoM does not exist.");
        }

        ValidationHelper.ThrowIfAny(errors);
    }

    private async Task ValidateRequesterAsync(Guid requesterId, CancellationToken cancellationToken)
    {
        if (requesterId == Guid.Empty)
            throw new ValidationException("RequesterId is required.");

        var exists = await _accountRepository.QueryActive().AnyAsync(x => x.AccountId == requesterId, cancellationToken);
        if (!exists)
            throw new ValidationException("Requester does not exist.");
    }

    private static void ValidateReviewProposal(ReviewProposalDto dto, Proposal proposal)
    {
        var errors = new List<string>();
        var allowedHeaderStatuses = new[] { "APPROVED", "REJECTED", "REVISION_REQUIRED" };
        var allowedDetailStatuses = new[] { "APPROVED", "PARTIALLY_APPROVED", "REJECTED" };
        var headerStatus = dto.Status?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(headerStatus) || !allowedHeaderStatuses.Contains(headerStatus))
            errors.Add("Review status must be APPROVED, REJECTED, or REVISION_REQUIRED.");

        if (dto.Details is null || dto.Details.Count == 0)
            errors.Add("Review must contain at least one proposal detail.");

        var reviewedIds = (dto.Details ?? new List<ReviewProposalDetailDto>()).Select(x => x.ProposalDetailId).ToList();
        var duplicateIds = reviewedIds.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
        foreach (var duplicateId in duplicateIds)
            errors.Add($"Proposal detail '{duplicateId}' is reviewed more than once.");

        var missingDetailIds = proposal.Details.Select(x => x.ProposalDetailId).Except(reviewedIds).ToList();
        foreach (var missingId in missingDetailIds)
            errors.Add($"Proposal detail '{missingId}' must be reviewed.");

        var normalizedDetailStatuses = new List<string>();

        foreach (var reviewDetail in dto.Details ?? new List<ReviewProposalDetailDto>())
        {
            var detail = proposal.Details.FirstOrDefault(x => x.ProposalDetailId == reviewDetail.ProposalDetailId);
            var detailStatus = reviewDetail.Status?.Trim().ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(detailStatus))
                normalizedDetailStatuses.Add(detailStatus);

            if (detail is null)
            {
                errors.Add($"Proposal detail '{reviewDetail.ProposalDetailId}' does not exist in this proposal.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(detailStatus) || !allowedDetailStatuses.Contains(detailStatus))
                errors.Add($"Proposal detail '{reviewDetail.ProposalDetailId}' status must be APPROVED, PARTIALLY_APPROVED, or REJECTED.");

            if (reviewDetail.ApprovedQty < 0)
                errors.Add($"Proposal detail '{reviewDetail.ProposalDetailId}' approved quantity cannot be negative.");

            if (reviewDetail.ApprovedQty > detail.RequestedQty)
                errors.Add($"Proposal detail '{reviewDetail.ProposalDetailId}' approved quantity cannot exceed requested quantity.");

            if (detailStatus is "APPROVED" or "PARTIALLY_APPROVED" && reviewDetail.ApprovedQty <= 0)
                errors.Add($"Proposal detail '{reviewDetail.ProposalDetailId}' approved quantity must be greater than 0 when status is approved or partially approved.");
        }

        if (headerStatus == "REJECTED" && normalizedDetailStatuses.Any(x => x != "REJECTED"))
            errors.Add("When proposal status is REJECTED, all proposal details must be REJECTED.");

        if (headerStatus == "APPROVED" && !normalizedDetailStatuses.Any(x => x is "APPROVED" or "PARTIALLY_APPROVED"))
            errors.Add("When proposal status is APPROVED, at least one proposal detail must be APPROVED or PARTIALLY_APPROVED.");

        ValidationHelper.ThrowIfAny(errors);
    }

    private async Task EnsureStatusAllowedAsync(Guid currentStatusId, IEnumerable<string> allowedStatuses, string message, CancellationToken cancellationToken)
    {
        if (!await _statusLookupService.IsAnyAsync(currentStatusId, allowedStatuses, cancellationToken))
            throw new ValidationException(message);
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

    private async Task<ProposalResponseDto> ToResponseDtoAsync(Proposal entity, CancellationToken cancellationToken)
    {
        var details = new List<ProposalDetailResponseDto>();

        foreach (var detail in entity.Details.OrderBy(x => x.CreatedAt))
        {
            var usedQty = await _purchaseRequestRepository.GetUsedQuantityByProposalDetailAsync(detail.ProposalDetailId, null, cancellationToken);
            details.Add(new ProposalDetailResponseDto
            {
                ProposalDetailId = detail.ProposalDetailId,
                ProposalId = detail.ProposalId,
                MaterialId = detail.MaterialId,
                MaterialCode = detail.Material?.MaterialCode,
                MaterialName = detail.Material?.MaterialName,
                UomId = detail.UomId,
                UomCode = detail.Uom?.UomCode,
                UomName = detail.Uom?.UomName,
                Description = detail.Description,
                RequestedQty = detail.RequestedQty,
                ApprovedQty = detail.ApprovedQty,
                RemainingQty = Math.Max(0, detail.ApprovedQty - usedQty),
                StatusId = detail.StatusId,
                StatusName = detail.Status?.StatusName,
                CreatedAt = detail.CreatedAt,
                UpdatedAt = detail.UpdatedAt
            });
        }

        return new ProposalResponseDto
        {
            ProposalId = entity.ProposalId,
            ProposalNo = entity.ProposalNo,
            RequesterId = entity.RequesterId,
            ProposalDate = entity.ProposalDate,
            Purpose = entity.Purpose,
            StatusId = entity.StatusId,
            StatusName = entity.Status?.StatusName ?? string.Empty,
            ProposalAttachmentPath = entity.ProposalAttachmentPath,
            SubmittedAt = entity.SubmittedAt,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
            Details = details
        };
    }
}
