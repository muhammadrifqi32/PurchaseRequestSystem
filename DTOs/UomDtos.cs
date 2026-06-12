namespace PurchaseRequestSystem.DTOs;

public class CreateUomDto
{
    public string? UomCode { get; set; }
    public string? UomName { get; set; }
}

public class UpdateUomDto
{
    public string? UomCode { get; set; }
    public string? UomName { get; set; }
}

public class UomResponseDto
{
    public Guid UomId { get; set; }
    public string UomCode { get; set; } = string.Empty;
    public string UomName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
