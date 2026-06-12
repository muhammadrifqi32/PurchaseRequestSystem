namespace PurchaseRequestSystem.DTOs;

public class CreateVendorDto
{
    public string? VendorCode { get; set; }
    public string? VendorName { get; set; }
}

public class UpdateVendorDto
{
    public string? VendorCode { get; set; }
    public string? VendorName { get; set; }
}

public class VendorResponseDto
{
    public Guid VendorId { get; set; }
    public string VendorCode { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
