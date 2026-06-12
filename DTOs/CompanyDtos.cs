namespace PurchaseRequestSystem.DTOs;

public class CreateCompanyDto
{
    public string? CompanyCode { get; set; }
    public string? CompanyName { get; set; }
}

public class UpdateCompanyDto
{
    public string? CompanyCode { get; set; }
    public string? CompanyName { get; set; }
}

public class CompanyResponseDto
{
    public Guid CompanyId { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
