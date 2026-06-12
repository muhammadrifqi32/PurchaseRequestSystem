namespace PurchaseRequestSystem.DTOs;

public class CreateRoleDto
{
    public string? RoleCode { get; set; }
    public string? RoleName { get; set; }
    public string? Description { get; set; }
}

public class UpdateRoleDto
{
    public string? RoleCode { get; set; }
    public string? RoleName { get; set; }
    public string? Description { get; set; }
}

public class RoleResponseDto
{
    public Guid RoleId { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
