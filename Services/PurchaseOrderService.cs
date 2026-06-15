using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Common.Enums;
using PurchaseRequestSystem.Common.Exceptions;
using PurchaseRequestSystem.Data;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly AppDbContext _context;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IStatusLookupService _statusLookupService;
    private readonly IGenericRepository<Vendor> _vendorRepository;
    private readonly IGenericRepository<Company> _companyRepository;
    private readonly IGenericRepository<Account> _accountRepository;

    public PurchaseOrderService(
        AppDbContext context,
        IPurchaseOrderRepository purchaseOrderRepository,
        IPurchaseRequestRepository purchaseRequestRepository,
        IStatusLookupService statusLookupService,
        IGenericRepository<Vendor> vendorRepository,
        IGenericRepository<Company> companyRepository,
        IGenericRepository<Account> accountRepository)
    {
        _context = context;
        _purchaseOrderRepository = purchaseOrderRepository;
        _purchaseRequestRepository = purchaseRequestRepository;
        _statusLookupService = statusLookupService;
        _vendorRepository = vendorRepository;
        _companyRepository = companyRepository;
        _accountRepository = accountRepository;
    }

    public async Task<List<PurchaseOrderResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _purchaseOrderRepository.GetAllWithDetailsAsync(cancellationToken);
        return entities.Select(ToResponseDto).ToList();
    }

    public async Task<PurchaseOrderResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _purchaseOrderRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Purchase Order not found");

        return ToResponseDto(entity);
    }

    public async Task<PurchaseOrderResponseDto> GetByPurchaseRequestIdAsync(Guid purchaseRequestId, CancellationToken cancellationToken = default)
    {
        var entity = await _purchaseOrderRepository.GetByPurchaseRequestIdAsync(purchaseRequestId, cancellationToken)
            ?? throw new NotFoundException("Purchase Order not found for this Purchase Request");

        return ToResponseDto(entity);
    }

    public async Task<PurchaseOrderResponseDto> GenerateFromPurchaseRequestAsync(Guid purchaseRequestId, GeneratePurchaseOrderDto dto, CancellationToken cancellationToken = default)
    {
        if (dto is null)
            throw new ValidationException("Request body is required.");

        ValidateVendorOrCompany(dto.VendorId, dto.CompanyId);
        ValidateCommonHeader(dto.PoDate, dto.TaxRate, dto.CreatedBy, "CreatedBy");
        ValidateGenerateDetails(dto.Details);

        var purchaseRequest = await _purchaseRequestRepository.GetPurchaseRequestWithDetailsAsync(purchaseRequestId, cancellationToken)
            ?? throw new NotFoundException("Purchase Request not found");

        if (!purchaseRequest.Details.Any())
            throw new BusinessRuleException("Purchase Request must have at least one detail before generating Purchase Order.");

        if (!await _statusLookupService.IsAnyAsync(purchaseRequest.StatusId, ["APPROVED"], cancellationToken))
            throw new BusinessRuleException("Purchase Request is not approved yet");

        if (await _purchaseOrderRepository.ExistsByPurchaseRequestIdAsync(purchaseRequestId, cancellationToken))
            throw new BusinessRuleException("Purchase Order already exists for this Purchase Request");

        await ValidateVendorCompanyAndActorAsync(dto.VendorId, dto.CompanyId, dto.CreatedBy, "CreatedBy", cancellationToken);

        var prDetails = purchaseRequest.Details.ToList();
        var detailDtos = ValidateAndMapGenerateDetails(dto.Details, prDetails);
        var existingNumbers = await _purchaseOrderRepository.GetPurchaseOrderNumbersByPeriodAsync(dto.PoDate, cancellationToken);
        var openStatus = await _statusLookupService.GetRequiredAsync("OPEN", cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var purchaseOrder = new PurchaseOrder
        {
            PurchaseOrderId = Guid.NewGuid(),
            PurchaseRequestId = purchaseRequest.PurchaseRequestId,
            PurchaseOrderNo = DocumentNumberGenerator.Generate("PO", dto.PoDate, existingNumbers),
            VendorId = dto.VendorId,
            CompanyId = dto.CompanyId,
            PoDate = dto.PoDate.Date,
            StatusId = openStatus.StatusId,
            Notes = dto.Notes,
            TaxRate = dto.TaxRate,
            PurchaseOrderAttachmentPath = dto.PurchaseOrderAttachmentPath,
            CreatedAt = now,
            CreatedBy = dto.CreatedBy
        };

        var detailNo = 1;
        foreach (var requestDetail in prDetails.OrderBy(x => x.CreatedAt))
        {
            var detailDto = detailDtos[requestDetail.PurchaseRequestDetailId];
            var subtotal = Math.Round(requestDetail.Quantity * detailDto.UnitPrice, 2, MidpointRounding.AwayFromZero);

            purchaseOrder.Details.Add(new PurchaseOrderDetail
            {
                PurchaseOrderDetailId = Guid.NewGuid(),
                PurchaseOrderId = purchaseOrder.PurchaseOrderId,
                MaterialId = requestDetail.MaterialId,
                UomId = requestDetail.UomId,
                DetailNo = detailNo++,
                Quantity = requestDetail.Quantity,
                UnitPrice = detailDto.UnitPrice,
                SubtotalAmount = subtotal,
                Notes = detailDto.Notes,
                CreatedAt = now
            });
        }

        RecalculateTotals(purchaseOrder);

        await _purchaseOrderRepository.AddAsync(purchaseOrder, cancellationToken);
        await AddActivityLogAsync(dto.CreatedBy, purchaseOrder.PurchaseOrderId, "GENERATE_PO", $"Purchase Order {purchaseOrder.PurchaseOrderNo} generated from Purchase Request {purchaseRequest.PurchaseRequestNo}.", cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var created = await _purchaseOrderRepository.GetByIdWithDetailsAsync(purchaseOrder.PurchaseOrderId, cancellationToken)
            ?? purchaseOrder;

        return ToResponseDto(created);
    }

    public async Task<PurchaseOrderResponseDto> UpdateAsync(Guid id, UpdatePurchaseOrderDto dto, CancellationToken cancellationToken = default)
    {
        if (dto is null)
            throw new ValidationException("Request body is required.");

        ValidateVendorOrCompany(dto.VendorId, dto.CompanyId);
        ValidateCommonHeader(dto.PoDate, dto.TaxRate, dto.UpdatedBy, "UpdatedBy");
        ValidateUpdateDetails(dto.Details);

        var purchaseOrder = await _purchaseOrderRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Purchase Order not found");

        if (!await _statusLookupService.IsAnyAsync(purchaseOrder.StatusId, ["OPEN"], cancellationToken))
            throw new BusinessRuleException("Only OPEN Purchase Order can be updated.");

        await ValidateVendorCompanyAndActorAsync(dto.VendorId, dto.CompanyId, dto.UpdatedBy, "UpdatedBy", cancellationToken);

        var detailDtos = ValidateAndMapUpdateDetails(dto.Details, purchaseOrder.Details);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.UtcNow;
        purchaseOrder.VendorId = dto.VendorId;
        purchaseOrder.CompanyId = dto.CompanyId;
        purchaseOrder.PoDate = dto.PoDate.Date;
        purchaseOrder.TaxRate = dto.TaxRate;
        purchaseOrder.Notes = dto.Notes;
        purchaseOrder.PurchaseOrderAttachmentPath = dto.PurchaseOrderAttachmentPath;
        purchaseOrder.UpdatedAt = now;
        purchaseOrder.UpdatedBy = dto.UpdatedBy;

        foreach (var detail in purchaseOrder.Details)
        {
            if (!detailDtos.TryGetValue(detail.PurchaseOrderDetailId, out var detailDto))
                continue;

            detail.UnitPrice = detailDto.UnitPrice;
            detail.Notes = detailDto.Notes;
            detail.SubtotalAmount = Math.Round(detail.Quantity * detail.UnitPrice, 2, MidpointRounding.AwayFromZero);
            detail.UpdatedAt = now;
        }

        RecalculateTotals(purchaseOrder);

        _purchaseOrderRepository.Update(purchaseOrder);
        await AddActivityLogAsync(dto.UpdatedBy, purchaseOrder.PurchaseOrderId, "UPDATE_PO", $"Purchase Order {purchaseOrder.PurchaseOrderNo} updated.", cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var updated = await _purchaseOrderRepository.GetByIdWithDetailsAsync(id, cancellationToken) ?? purchaseOrder;
        return ToResponseDto(updated);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Purchase Order not found");

        if (!await _statusLookupService.IsAnyAsync(purchaseOrder.StatusId, ["OPEN"], cancellationToken))
            throw new BusinessRuleException("Only OPEN Purchase Order can be deleted.");

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        if (purchaseOrder.CreatedBy.HasValue)
        {
            await AddActivityLogAsync(purchaseOrder.CreatedBy.Value, purchaseOrder.PurchaseOrderId, "DELETE_PO", $"Purchase Order {purchaseOrder.PurchaseOrderNo} deleted.", cancellationToken);
        }

        _purchaseOrderRepository.Delete(purchaseOrder);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static void ValidateVendorOrCompany(Guid? vendorId, Guid? companyId)
    {
        if ((!vendorId.HasValue || vendorId.Value == Guid.Empty) && (!companyId.HasValue || companyId.Value == Guid.Empty))
            throw new ValidationException("VendorId or CompanyId is required.");
    }

    private static void ValidateCommonHeader(DateTime poDate, decimal taxRate, Guid actorId, string actorFieldName)
    {
        var errors = new List<string>();
        ValidationHelper.EnsureDate(poDate, "PoDate", errors);

        if (taxRate < 0)
            errors.Add("TaxRate cannot be negative.");

        if (actorId == Guid.Empty)
            errors.Add($"{actorFieldName} is required.");

        ValidationHelper.ThrowIfAny(errors);
    }

    private static void ValidateGenerateDetails(List<GeneratePurchaseOrderDetailDto>? details)
    {
        var errors = new List<string>();
        if (details is null || details.Count == 0)
            errors.Add("Purchase Order detail list cannot be empty.");

        if (details is null)
        {
            ValidationHelper.ThrowIfAny(errors);
            return;
        }

        for (var i = 0; i < details.Count; i++)
        {
            if (details[i].PurchaseRequestDetailId == Guid.Empty)
                errors.Add($"Details[{i}].PurchaseRequestDetailId is required.");

            if (details[i].UnitPrice < 0)
                errors.Add($"Details[{i}].UnitPrice cannot be negative.");
        }

        ValidationHelper.ThrowIfAny(errors);
    }

    private static void ValidateUpdateDetails(List<UpdatePurchaseOrderDetailDto>? details)
    {
        var errors = new List<string>();
        if (details is null || details.Count == 0)
            errors.Add("Purchase Order detail list cannot be empty.");

        if (details is null)
        {
            ValidationHelper.ThrowIfAny(errors);
            return;
        }

        for (var i = 0; i < details.Count; i++)
        {
            if (details[i].PurchaseOrderDetailId == Guid.Empty)
                errors.Add($"Details[{i}].PurchaseOrderDetailId is required.");

            if (details[i].UnitPrice < 0)
                errors.Add($"Details[{i}].UnitPrice cannot be negative.");
        }

        ValidationHelper.ThrowIfAny(errors);
    }

    private async Task ValidateVendorCompanyAndActorAsync(Guid? vendorId, Guid? companyId, Guid actorId, string actorFieldName, CancellationToken cancellationToken)
    {
        if (vendorId.HasValue && vendorId.Value != Guid.Empty)
        {
            var exists = await _vendorRepository.QueryActive()
                .AsNoTracking()
                .AnyAsync(x => x.VendorId == vendorId.Value, cancellationToken);

            if (!exists)
                throw new ValidationException($"Vendor '{vendorId.Value}' does not exist.");
        }

        if (companyId.HasValue && companyId.Value != Guid.Empty)
        {
            var exists = await _companyRepository.QueryActive()
                .AsNoTracking()
                .AnyAsync(x => x.CompanyId == companyId.Value, cancellationToken);

            if (!exists)
                throw new ValidationException($"Company '{companyId.Value}' does not exist.");
        }

        var actorExists = await _accountRepository.QueryActive()
            .AsNoTracking()
            .AnyAsync(x => x.AccountId == actorId, cancellationToken);

        if (!actorExists)
            throw new ValidationException($"{actorFieldName} account '{actorId}' does not exist.");
    }

    private static Dictionary<Guid, GeneratePurchaseOrderDetailDto> ValidateAndMapGenerateDetails(List<GeneratePurchaseOrderDetailDto> detailDtos, IReadOnlyCollection<PurchaseRequestDetail> purchaseRequestDetails)
    {
        var errors = new List<string>();
        var duplicateIds = detailDtos
            .GroupBy(x => x.PurchaseRequestDetailId)
            .Where(x => x.Key != Guid.Empty && x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        errors.AddRange(duplicateIds.Select(id => $"Duplicate PurchaseRequestDetailId '{id}' in request details."));

        var requestDetailIds = purchaseRequestDetails.Select(x => x.PurchaseRequestDetailId).ToHashSet();
        var dtoDetailIds = detailDtos.Select(x => x.PurchaseRequestDetailId).ToHashSet();

        foreach (var dtoId in dtoDetailIds.Where(id => !requestDetailIds.Contains(id)))
            errors.Add($"PurchaseRequestDetailId '{dtoId}' does not belong to this Purchase Request.");

        foreach (var requestDetailId in requestDetailIds.Where(id => !dtoDetailIds.Contains(id)))
            errors.Add($"PurchaseRequestDetailId '{requestDetailId}' must be included in Purchase Order details.");

        ValidationHelper.ThrowIfAny(errors);
        return detailDtos.ToDictionary(x => x.PurchaseRequestDetailId, x => x);
    }

    private static Dictionary<Guid, UpdatePurchaseOrderDetailDto> ValidateAndMapUpdateDetails(List<UpdatePurchaseOrderDetailDto> detailDtos, IEnumerable<PurchaseOrderDetail> purchaseOrderDetails)
    {
        var errors = new List<string>();
        var duplicateIds = detailDtos
            .GroupBy(x => x.PurchaseOrderDetailId)
            .Where(x => x.Key != Guid.Empty && x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        errors.AddRange(duplicateIds.Select(id => $"Duplicate PurchaseOrderDetailId '{id}' in request details."));

        var poDetailIds = purchaseOrderDetails.Select(x => x.PurchaseOrderDetailId).ToHashSet();
        foreach (var dtoId in detailDtos.Select(x => x.PurchaseOrderDetailId).Where(id => !poDetailIds.Contains(id)))
            errors.Add($"PurchaseOrderDetailId '{dtoId}' does not belong to this Purchase Order.");

        ValidationHelper.ThrowIfAny(errors);
        return detailDtos.ToDictionary(x => x.PurchaseOrderDetailId, x => x);
    }

    private static void RecalculateTotals(PurchaseOrder purchaseOrder)
    {
        purchaseOrder.SubtotalAmount = Math.Round(purchaseOrder.Details.Sum(x => x.SubtotalAmount), 2, MidpointRounding.AwayFromZero);
        purchaseOrder.TaxAmount = Math.Round(purchaseOrder.SubtotalAmount * purchaseOrder.TaxRate, 2, MidpointRounding.AwayFromZero);
        purchaseOrder.GrandtotalAmount = purchaseOrder.SubtotalAmount + purchaseOrder.TaxAmount;
    }

    private async Task AddActivityLogAsync(Guid accountId, Guid purchaseOrderId, string action, string? description, CancellationToken cancellationToken)
    {
        var accountExists = await _accountRepository.QueryActive()
            .AnyAsync(x => x.AccountId == accountId, cancellationToken);

        if (!accountExists)
            return;

        _context.ActivityLogs.Add(new ActivityLog
        {
            ActivityLogId = Guid.NewGuid(),
            AccountId = accountId,
            DocumentType = DocumentType.PURCHASE_ORDER,
            DocumentId = purchaseOrderId,
            Action = action,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static PurchaseOrderResponseDto ToResponseDto(PurchaseOrder entity)
    {
        return new PurchaseOrderResponseDto
        {
            PurchaseOrderId = entity.PurchaseOrderId,
            PurchaseRequestId = entity.PurchaseRequestId,
            PurchaseOrderNo = entity.PurchaseOrderNo,
            VendorId = entity.VendorId,
            VendorName = entity.Vendor?.VendorName,
            CompanyId = entity.CompanyId,
            CompanyName = entity.Company?.CompanyName,
            PoDate = entity.PoDate,
            StatusId = entity.StatusId,
            Status = entity.Status?.StatusName ?? string.Empty,
            Notes = entity.Notes,
            SubtotalAmount = entity.SubtotalAmount,
            TaxRate = entity.TaxRate,
            TaxAmount = entity.TaxAmount,
            GrandtotalAmount = entity.GrandtotalAmount,
            PurchaseOrderAttachmentPath = entity.PurchaseOrderAttachmentPath,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
            Details = entity.Details
                .OrderBy(x => x.DetailNo)
                .Select(detail => new PurchaseOrderDetailResponseDto
                {
                    PurchaseOrderDetailId = detail.PurchaseOrderDetailId,
                    MaterialId = detail.MaterialId,
                    MaterialCode = detail.Material?.MaterialCode,
                    MaterialName = detail.Material?.MaterialName ?? string.Empty,
                    UomId = detail.UomId,
                    UomCode = detail.Uom?.UomCode,
                    UomName = detail.Uom?.UomName ?? string.Empty,
                    DetailNo = detail.DetailNo,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    SubtotalAmount = detail.SubtotalAmount,
                    Notes = detail.Notes,
                    CreatedAt = detail.CreatedAt,
                    UpdatedAt = detail.UpdatedAt
                })
                .ToList()
        };
    }
}
