using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Common.Exceptions;
using PurchaseRequestSystem.Data;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Services;

public class ProcurementRequestService : IProcurementRequestService
{
    private readonly AppDbContext _context;
    private readonly IProcurementRequestRepository _procurementRequestRepository;
    private readonly IProposalRepository _proposalRepository;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IGenericRepository<RequestType> _requestTypeRepository;
    private readonly IGenericRepository<Material> _materialRepository;
    private readonly IGenericRepository<Uom> _uomRepository;
    private readonly IStatusLookupService _statusLookupService;

    public ProcurementRequestService(
        AppDbContext context,
        IProcurementRequestRepository procurementRequestRepository,
        IProposalRepository proposalRepository,
        IPurchaseRequestRepository purchaseRequestRepository,
        IGenericRepository<Account> accountRepository,
        IGenericRepository<RequestType> requestTypeRepository,
        IGenericRepository<Material> materialRepository,
        IGenericRepository<Uom> uomRepository,
        IStatusLookupService statusLookupService)
    {
        _context = context;
        _procurementRequestRepository = procurementRequestRepository;
        _proposalRepository = proposalRepository;
        _purchaseRequestRepository = purchaseRequestRepository;
        _accountRepository = accountRepository;
        _requestTypeRepository = requestTypeRepository;
        _materialRepository = materialRepository;
        _uomRepository = uomRepository;
        _statusLookupService = statusLookupService;
    }

    public async Task<IReadOnlyList<ProcurementRequestResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var data = await _procurementRequestRepository.GetAllWithPurchaseRequestAsync(cancellationToken);
        return data.Select(ToResponseDto).ToList();
    }

    public async Task<ProcurementRequestResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _procurementRequestRepository.GetProcurementRequestWithPurchaseRequestAsync(id, cancellationToken)
            ?? throw new NotFoundException("ProcurementRequest", id);

        return ToResponseDto(entity);
    }

    public async Task<ProcurementRequestResponseDto> CreateProjectAsync(CreateProjectProcurementRequestDto dto, CancellationToken cancellationToken = default)
    {
        ValidateProjectRequest(dto);
        await ValidateRequesterAsync(dto.RequesterId, cancellationToken);

        var proposal = await _proposalRepository.GetProposalWithDetailsAsync(dto.ProposalId, cancellationToken)
            ?? throw new NotFoundException("Proposal", dto.ProposalId);

        if (!await _statusLookupService.IsAnyAsync(proposal.StatusId, ["APPROVED"], cancellationToken))
            throw new ValidationException("Proposal is not approved.");

        var requestType = await GetRequestTypeAsync("PROJECT", cancellationToken);
        var draftStatus = await _statusLookupService.GetRequiredAsync("DRAFT", cancellationToken);
        var procurementRequestNo = await GenerateProcurementRequestNoAsync(dto.RequestDate, cancellationToken);
        var purchaseRequestNo = await GeneratePurchaseRequestNoAsync(dto.RequestDate, cancellationToken);

        await ValidateProjectDetailsAsync(dto, proposal, cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var procurementRequest = new ProcurementRequest
        {
            ProcurementRequestId = Guid.NewGuid(),
            ProcurementRequestNo = procurementRequestNo,
            RequestTypeId = requestType.RequestTypeId,
            ProposalId = proposal.ProposalId,
            RequesterId = dto.RequesterId,
            RequestDate = dto.RequestDate.Date,
            StatusId = draftStatus.StatusId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = dto.RequesterId,
            PurchaseRequest = new PurchaseRequest
            {
                PurchaseRequestId = Guid.NewGuid(),
                PurchaseRequestNo = purchaseRequestNo,
                StatusId = draftStatus.StatusId,
                Notes = dto.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.RequesterId,
                Details = dto.Details.Select(detail => new PurchaseRequestDetail
                {
                    PurchaseRequestDetailId = Guid.NewGuid(),
                    ProposalDetailId = detail.ProposalDetailId,
                    MaterialId = detail.MaterialId,
                    UomId = detail.UomId,
                    Description = detail.Description?.Trim(),
                    Quantity = detail.Quantity,
                    Notes = detail.Notes?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.RequesterId
                }).ToList()
            }
        };

        await _procurementRequestRepository.AddAsync(procurementRequest, cancellationToken);
        await _procurementRequestRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var saved = await _procurementRequestRepository.GetProcurementRequestWithPurchaseRequestAsync(procurementRequest.ProcurementRequestId, cancellationToken)
            ?? procurementRequest;

        return ToResponseDto(saved);
    }

    public async Task<ProcurementRequestResponseDto> CreateNonProjectAsync(CreateNonProjectProcurementRequestDto dto, CancellationToken cancellationToken = default)
    {
        ValidateNonProjectRequest(dto);
        await ValidateRequesterAsync(dto.RequesterId, cancellationToken);
        await ValidateNonProjectDetailsAsync(dto.Details, cancellationToken);

        var requestType = await GetRequestTypeAsync("NON_PROJECT", cancellationToken);
        var draftStatus = await _statusLookupService.GetRequiredAsync("DRAFT", cancellationToken);
        var procurementRequestNo = await GenerateProcurementRequestNoAsync(dto.RequestDate, cancellationToken);
        var purchaseRequestNo = await GeneratePurchaseRequestNoAsync(dto.RequestDate, cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var procurementRequest = new ProcurementRequest
        {
            ProcurementRequestId = Guid.NewGuid(),
            ProcurementRequestNo = procurementRequestNo,
            RequestTypeId = requestType.RequestTypeId,
            ProposalId = null,
            RequesterId = dto.RequesterId,
            RequestDate = dto.RequestDate.Date,
            StatusId = draftStatus.StatusId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = dto.RequesterId,
            PurchaseRequest = new PurchaseRequest
            {
                PurchaseRequestId = Guid.NewGuid(),
                PurchaseRequestNo = purchaseRequestNo,
                StatusId = draftStatus.StatusId,
                Notes = dto.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.RequesterId,
                Details = dto.Details.Select(detail => new PurchaseRequestDetail
                {
                    PurchaseRequestDetailId = Guid.NewGuid(),
                    ProposalDetailId = null,
                    MaterialId = detail.MaterialId,
                    UomId = detail.UomId,
                    Description = detail.Description?.Trim(),
                    Quantity = detail.Quantity,
                    Notes = detail.Notes?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.RequesterId
                }).ToList()
            }
        };

        await _procurementRequestRepository.AddAsync(procurementRequest, cancellationToken);
        await _procurementRequestRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var saved = await _procurementRequestRepository.GetProcurementRequestWithPurchaseRequestAsync(procurementRequest.ProcurementRequestId, cancellationToken)
            ?? procurementRequest;

        return ToResponseDto(saved);
    }

    private async Task ValidateRequesterAsync(Guid requesterId, CancellationToken cancellationToken)
    {
        if (requesterId == Guid.Empty)
            throw new ValidationException("RequesterId is required.");

        var exists = await _accountRepository.QueryActive().AnyAsync(x => x.AccountId == requesterId, cancellationToken);
        if (!exists)
            throw new ValidationException("Requester does not exist.");
    }

    private static void ValidateProjectRequest(CreateProjectProcurementRequestDto dto)
    {
        var errors = new List<string>();
        if (dto.ProposalId == Guid.Empty) errors.Add("ProposalId is required for Project request.");
        if (dto.RequesterId == Guid.Empty) errors.Add("RequesterId is required.");
        ValidationHelper.EnsureDate(dto.RequestDate, nameof(dto.RequestDate), errors);
        if (dto.Details is null || dto.Details.Count == 0) errors.Add("Purchase request must have at least one detail.");
        ValidationHelper.ThrowIfAny(errors);
    }

    private static void ValidateNonProjectRequest(CreateNonProjectProcurementRequestDto dto)
    {
        var errors = new List<string>();
        if (dto.RequesterId == Guid.Empty) errors.Add("RequesterId is required.");
        ValidationHelper.EnsureDate(dto.RequestDate, nameof(dto.RequestDate), errors);
        if (dto.Details is null || dto.Details.Count == 0) errors.Add("Purchase request must have at least one detail.");
        ValidationHelper.ThrowIfAny(errors);
    }

    private async Task ValidateProjectDetailsAsync(CreateProjectProcurementRequestDto dto, Proposal proposal, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var groupedRequestedQty = dto.Details
            .GroupBy(x => x.ProposalDetailId)
            .ToDictionary(x => x.Key, x => x.Sum(d => d.Quantity));

        for (var index = 0; index < dto.Details.Count; index++)
        {
            var detail = dto.Details[index];
            var row = index + 1;

            if (detail.ProposalDetailId == Guid.Empty) errors.Add($"Details[{row}].ProposalDetailId is required.");
            if (detail.MaterialId == Guid.Empty) errors.Add($"Details[{row}].MaterialId is required.");
            if (detail.UomId == Guid.Empty) errors.Add($"Details[{row}].UomId is required.");
            ValidationHelper.EnsurePositive(detail.Quantity, $"Details[{row}].Quantity", errors);

            var proposalDetail = proposal.Details.FirstOrDefault(x => x.ProposalDetailId == detail.ProposalDetailId);
            if (proposalDetail is null)
            {
                errors.Add($"Details[{row}].ProposalDetail does not exist in selected proposal.");
                continue;
            }

            if (proposalDetail.ApprovedQty <= 0)
                errors.Add($"Details[{row}].ProposalDetail has no approved quantity.");

            if (proposalDetail.MaterialId != detail.MaterialId)
                errors.Add($"Details[{row}].Material must match selected Proposal Detail material.");

            if (proposalDetail.UomId != detail.UomId)
                errors.Add($"Details[{row}].UoM must match selected Proposal Detail UoM.");
        }

        foreach (var pair in groupedRequestedQty)
        {
            var proposalDetail = proposal.Details.FirstOrDefault(x => x.ProposalDetailId == pair.Key);
            if (proposalDetail is null) continue;

            var usedQty = await _purchaseRequestRepository.GetUsedQuantityByProposalDetailAsync(pair.Key, null, cancellationToken);
            var totalAfterRequest = usedQty + pair.Value;
            if (totalAfterRequest > proposalDetail.ApprovedQty)
            {
                errors.Add($"Requested quantity exceeds remaining approved quantity for proposal detail '{pair.Key}'. Approved: {proposalDetail.ApprovedQty}, used: {usedQty}, requested: {pair.Value}.");
            }
        }

        ValidationHelper.ThrowIfAny(errors);
    }

    private async Task ValidateNonProjectDetailsAsync(IReadOnlyList<CreatePurchaseRequestDetailDto> details, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        for (var index = 0; index < details.Count; index++)
        {
            var detail = details[index];
            var row = index + 1;

            if (detail.MaterialId == Guid.Empty) errors.Add($"Details[{row}].MaterialId is required.");
            if (detail.UomId == Guid.Empty) errors.Add($"Details[{row}].UomId is required.");
            ValidationHelper.EnsurePositive(detail.Quantity, $"Details[{row}].Quantity", errors);

            if (detail.MaterialId != Guid.Empty && !await _materialRepository.Query().AnyAsync(x => x.MaterialId == detail.MaterialId, cancellationToken))
                errors.Add($"Details[{row}].Material does not exist.");

            if (detail.UomId != Guid.Empty && !await _uomRepository.QueryActive().AnyAsync(x => x.UomId == detail.UomId, cancellationToken))
                errors.Add($"Details[{row}].UoM does not exist.");
        }

        ValidationHelper.ThrowIfAny(errors);
    }

    private async Task<RequestType> GetRequestTypeAsync(string requestTypeCodeOrName, CancellationToken cancellationToken)
    {
        var normalized = requestTypeCodeOrName.Trim().ToUpperInvariant();
        var requestType = await _requestTypeRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RequestTypeCode.ToUpper() == normalized || x.RequestTypeName.ToUpper() == normalized, cancellationToken);

        return requestType ?? throw new BadRequestException($"Request type '{normalized}' does not exist.");
    }

    private async Task<string> GenerateProcurementRequestNoAsync(DateTime requestDate, CancellationToken cancellationToken)
    {
        var existingNumbers = await _procurementRequestRepository.GetProcurementRequestNumbersByPeriodAsync(requestDate, cancellationToken);
        var number = DocumentNumberGenerator.Generate("PRQ", requestDate, existingNumbers);

        while (await _procurementRequestRepository.CheckProcurementRequestNoExistsAsync(number, cancellationToken))
        {
            existingNumbers = existingNumbers.Append(number).ToList();
            number = DocumentNumberGenerator.Generate("PRQ", requestDate, existingNumbers);
        }

        return number;
    }


    private async Task<string> GeneratePurchaseRequestNoAsync(DateTime requestDate, CancellationToken cancellationToken)
    {
        var existingNumbers = await _purchaseRequestRepository.GetPurchaseRequestNumbersByPeriodAsync(requestDate, cancellationToken);
        var number = DocumentNumberGenerator.Generate("PR", requestDate, existingNumbers);

        while (await _purchaseRequestRepository.CheckPurchaseRequestNoExistsAsync(number, cancellationToken))
        {
            existingNumbers = existingNumbers.Append(number).ToList();
            number = DocumentNumberGenerator.Generate("PR", requestDate, existingNumbers);
        }

        return number;
    }

    private static ProcurementRequestResponseDto ToResponseDto(ProcurementRequest entity)
    {
        return new ProcurementRequestResponseDto
        {
            ProcurementRequestId = entity.ProcurementRequestId,
            ProcurementRequestNo = entity.ProcurementRequestNo,
            RequestTypeId = entity.RequestTypeId,
            RequestTypeCode = entity.RequestType?.RequestTypeCode ?? string.Empty,
            RequestTypeName = entity.RequestType?.RequestTypeName ?? string.Empty,
            ProposalId = entity.ProposalId,
            ProposalNo = entity.Proposal?.ProposalNo,
            RequesterId = entity.RequesterId,
            RequestDate = entity.RequestDate,
            StatusId = entity.StatusId,
            StatusName = entity.Status?.StatusName ?? string.Empty,
            SubmittedAt = entity.SubmittedAt,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
            PurchaseRequest = entity.PurchaseRequest is null ? null : ToPurchaseRequestResponseDto(entity.PurchaseRequest, entity.ProcurementRequestNo)
        };
    }

    private static PurchaseRequestResponseDto ToPurchaseRequestResponseDto(PurchaseRequest entity, string? procurementRequestNo = null)
    {
        return new PurchaseRequestResponseDto
        {
            PurchaseRequestId = entity.PurchaseRequestId,
            PurchaseRequestNo = entity.PurchaseRequestNo ?? string.Empty,
            ProcurementRequestId = entity.ProcurementRequestId,
            ProcurementRequestNo = procurementRequestNo ?? entity.ProcurementRequest?.ProcurementRequestNo,
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
