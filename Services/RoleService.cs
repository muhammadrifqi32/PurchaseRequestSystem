using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Common.Exceptions;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Services;

public class RoleService : IRoleService
{
    private readonly IGenericRepository<Role> _repository;

    public RoleService(IGenericRepository<Role> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<RoleResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.QueryActive()
            .AsNoTracking()
            .OrderBy(x => x.RoleCode)
            .Select(x => ToResponseDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<RoleResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RoleId == id, cancellationToken);

        return entity is null
            ? throw new NotFoundException("Role", id)
            : ToResponseDto(entity);
    }

    public async Task<RoleResponseDto> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var code = InputHelper.NormalizeCode(dto.RoleCode, nameof(dto.RoleCode));
        var name = InputHelper.Required(dto.RoleName, nameof(dto.RoleName));
        var description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        if (await _repository.AnyAsync(x => x.RoleCode == code, cancellationToken))
            throw new BadRequestException("Role code already exists.");

        var entity = new Role
        {
            RoleId = Guid.NewGuid(),
            RoleCode = code,
            RoleName = name,
            Description = description
        };

        AuditHelper.SetCreatedAudit(entity);
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToResponseDto(entity);
    }

    public async Task<RoleResponseDto> UpdateAsync(Guid id, UpdateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .FirstOrDefaultAsync(x => x.RoleId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("Role", id);

        var code = InputHelper.NormalizeCode(dto.RoleCode, nameof(dto.RoleCode));
        var name = InputHelper.Required(dto.RoleName, nameof(dto.RoleName));
        var description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        if (await _repository.AnyAsync(x => x.RoleCode == code && x.RoleId != id, cancellationToken))
            throw new BadRequestException("Role code already exists.");

        entity.RoleCode = code;
        entity.RoleName = name;
        entity.Description = description;
        AuditHelper.SetUpdatedAudit(entity);

        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToResponseDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .FirstOrDefaultAsync(x => x.RoleId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("Role", id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static RoleResponseDto ToResponseDto(Role entity) => new()
    {
        RoleId = entity.RoleId,
        RoleCode = entity.RoleCode,
        RoleName = entity.RoleName,
        Description = entity.Description,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
