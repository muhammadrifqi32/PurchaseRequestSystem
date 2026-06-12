using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Common.Exceptions;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Services;

public class VendorService : IVendorService
{
    private readonly IGenericRepository<Vendor> _repository;

    public VendorService(IGenericRepository<Vendor> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<VendorResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.QueryActive()
            .AsNoTracking()
            .OrderBy(x => x.VendorCode)
            .Select(x => ToResponseDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<VendorResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.VendorId == id, cancellationToken);

        return entity is null
            ? throw new NotFoundException("Vendor", id)
            : ToResponseDto(entity);
    }

    public async Task<VendorResponseDto> CreateAsync(CreateVendorDto dto, CancellationToken cancellationToken = default)
    {
        var code = InputHelper.NormalizeCode(dto.VendorCode, nameof(dto.VendorCode));
        var name = InputHelper.Required(dto.VendorName, nameof(dto.VendorName));

        if (await _repository.AnyAsync(x => x.VendorCode == code, cancellationToken))
            throw new BadRequestException("Vendor code already exists.");

        var entity = new Vendor
        {
            VendorId = Guid.NewGuid(),
            VendorCode = code,
            VendorName = name,
            IsDeleted = false
        };

        AuditHelper.SetCreatedAudit(entity);
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToResponseDto(entity);
    }

    public async Task<VendorResponseDto> UpdateAsync(Guid id, UpdateVendorDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .FirstOrDefaultAsync(x => x.VendorId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("Vendor", id);

        var code = InputHelper.NormalizeCode(dto.VendorCode, nameof(dto.VendorCode));
        var name = InputHelper.Required(dto.VendorName, nameof(dto.VendorName));

        if (await _repository.AnyAsync(x => x.VendorCode == code && x.VendorId != id, cancellationToken))
            throw new BadRequestException("Vendor code already exists.");

        entity.VendorCode = code;
        entity.VendorName = name;
        AuditHelper.SetUpdatedAudit(entity);

        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToResponseDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .FirstOrDefaultAsync(x => x.VendorId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("Vendor", id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static VendorResponseDto ToResponseDto(Vendor entity) => new()
    {
        VendorId = entity.VendorId,
        VendorCode = entity.VendorCode,
        VendorName = entity.VendorName,
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
