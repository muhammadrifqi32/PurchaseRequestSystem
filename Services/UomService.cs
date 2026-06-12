using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Common.Exceptions;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Services;

public class UomService : IUomService
{
    private readonly IGenericRepository<Uom> _repository;

    public UomService(IGenericRepository<Uom> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<UomResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.QueryActive()
            .AsNoTracking()
            .OrderBy(x => x.UomCode)
            .Select(x => ToResponseDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<UomResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UomId == id, cancellationToken);

        return entity is null
            ? throw new NotFoundException("Uom", id)
            : ToResponseDto(entity);
    }

    public async Task<UomResponseDto> CreateAsync(CreateUomDto dto, CancellationToken cancellationToken = default)
    {
        var code = InputHelper.NormalizeCode(dto.UomCode, nameof(dto.UomCode));
        var name = InputHelper.Required(dto.UomName, nameof(dto.UomName));

        if (await _repository.AnyAsync(x => x.UomCode == code, cancellationToken))
            throw new BadRequestException("UoM code already exists.");

        var entity = new Uom
        {
            UomId = Guid.NewGuid(),
            UomCode = code,
            UomName = name,
            IsDeleted = false
        };

        AuditHelper.SetCreatedAudit(entity);
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToResponseDto(entity);
    }

    public async Task<UomResponseDto> UpdateAsync(Guid id, UpdateUomDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .FirstOrDefaultAsync(x => x.UomId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("Uom", id);

        var code = InputHelper.NormalizeCode(dto.UomCode, nameof(dto.UomCode));
        var name = InputHelper.Required(dto.UomName, nameof(dto.UomName));

        if (await _repository.AnyAsync(x => x.UomCode == code && x.UomId != id, cancellationToken))
            throw new BadRequestException("UoM code already exists.");

        entity.UomCode = code;
        entity.UomName = name;
        AuditHelper.SetUpdatedAudit(entity);

        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToResponseDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .FirstOrDefaultAsync(x => x.UomId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("Uom", id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static UomResponseDto ToResponseDto(Uom entity) => new()
    {
        UomId = entity.UomId,
        UomCode = entity.UomCode,
        UomName = entity.UomName,
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
