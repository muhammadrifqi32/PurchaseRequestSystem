using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Common.Exceptions;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Services;

public class MaterialService : IMaterialService
{
    private readonly IGenericRepository<Material> _repository;
    private readonly IGenericRepository<Uom> _uomRepository;

    public MaterialService(
        IGenericRepository<Material> repository,
        IGenericRepository<Uom> uomRepository)
    {
        _repository = repository;
        _uomRepository = uomRepository;
    }

    public async Task<IReadOnlyList<MaterialResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.QueryActive()
            .AsNoTracking()
            .Include(x => x.Uom)
            .OrderBy(x => x.MaterialCode)
            .Select(x => ToResponseDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<MaterialResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .AsNoTracking()
            .Include(x => x.Uom)
            .FirstOrDefaultAsync(x => x.MaterialId == id, cancellationToken);

        return entity is null
            ? throw new NotFoundException("Material", id)
            : ToResponseDto(entity);
    }

    public async Task<MaterialResponseDto> CreateAsync(CreateMaterialDto dto, CancellationToken cancellationToken = default)
    {
        var code = InputHelper.NormalizeCode(dto.MaterialCode, nameof(dto.MaterialCode));
        var name = InputHelper.Required(dto.MaterialName, nameof(dto.MaterialName));
        var description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        if (dto.UomId == Guid.Empty)
            throw new ValidationException("UomId is required.");

        if (await _repository.AnyAsync(x => x.MaterialCode == code, cancellationToken))
            throw new BadRequestException("Material code already exists.");

        var uom = await _uomRepository.QueryActive()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UomId == dto.UomId, cancellationToken);

        if (uom is null)
            throw new BadRequestException("UoM does not exist.");

        var entity = new Material
        {
            MaterialId = Guid.NewGuid(),
            UomId = dto.UomId,
            MaterialCode = code,
            MaterialName = name,
            Description = description
        };

        AuditHelper.SetCreatedAudit(entity);
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        entity.Uom = uom;
        return ToResponseDto(entity);
    }

    public async Task<MaterialResponseDto> UpdateAsync(Guid id, UpdateMaterialDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .Include(x => x.Uom)
            .FirstOrDefaultAsync(x => x.MaterialId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("Material", id);

        var code = InputHelper.NormalizeCode(dto.MaterialCode, nameof(dto.MaterialCode));
        var name = InputHelper.Required(dto.MaterialName, nameof(dto.MaterialName));
        var description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        if (dto.UomId == Guid.Empty)
            throw new ValidationException("UomId is required.");

        if (await _repository.AnyAsync(x => x.MaterialCode == code && x.MaterialId != id, cancellationToken))
            throw new BadRequestException("Material code already exists.");

        var uom = await _uomRepository.QueryActive()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UomId == dto.UomId, cancellationToken);

        if (uom is null)
            throw new BadRequestException("UoM does not exist.");

        entity.UomId = dto.UomId;
        entity.MaterialCode = code;
        entity.MaterialName = name;
        entity.Description = description;
        AuditHelper.SetUpdatedAudit(entity);

        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken);

        entity.Uom = uom;
        return ToResponseDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .FirstOrDefaultAsync(x => x.MaterialId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("Material", id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static MaterialResponseDto ToResponseDto(Material entity) => new()
    {
        MaterialId = entity.MaterialId,
        UomId = entity.UomId,
        UomCode = entity.Uom?.UomCode ?? string.Empty,
        UomName = entity.Uom?.UomName ?? string.Empty,
        MaterialCode = entity.MaterialCode,
        MaterialName = entity.MaterialName,
        Description = entity.Description,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
