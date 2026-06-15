namespace PurchaseRequestSystem.DTOs;

public class GeneratePurchaseOrderDto
{
    public Guid? VendorId { get; set; }
    public Guid? CompanyId { get; set; }
    public DateTime PoDate { get; set; }
    public decimal TaxRate { get; set; }
    public string? Notes { get; set; }
    public string? PurchaseOrderAttachmentPath { get; set; }
    public Guid CreatedBy { get; set; }
    public List<GeneratePurchaseOrderDetailDto> Details { get; set; } = new();
}

public class GeneratePurchaseOrderDetailDto
{
    public Guid PurchaseRequestDetailId { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePurchaseOrderDto
{
    public Guid? VendorId { get; set; }
    public Guid? CompanyId { get; set; }
    public DateTime PoDate { get; set; }
    public decimal TaxRate { get; set; }
    public string? Notes { get; set; }
    public string? PurchaseOrderAttachmentPath { get; set; }
    public Guid UpdatedBy { get; set; }
    public List<UpdatePurchaseOrderDetailDto> Details { get; set; } = new();
}

public class UpdatePurchaseOrderDetailDto
{
    public Guid PurchaseOrderDetailId { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
}

public class PurchaseOrderResponseDto
{
    public Guid PurchaseOrderId { get; set; }
    public Guid PurchaseRequestId { get; set; }
    public string PurchaseOrderNo { get; set; } = string.Empty;
    public Guid? VendorId { get; set; }
    public string? VendorName { get; set; }
    public Guid? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public DateTime PoDate { get; set; }
    public Guid StatusId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandtotalAmount { get; set; }
    public string? PurchaseOrderAttachmentPath { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public List<PurchaseOrderDetailResponseDto> Details { get; set; } = new();
}

public class PurchaseOrderDetailResponseDto
{
    public Guid PurchaseOrderDetailId { get; set; }
    public Guid MaterialId { get; set; }
    public string? MaterialCode { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public Guid UomId { get; set; }
    public string? UomCode { get; set; }
    public string UomName { get; set; } = string.Empty;
    public int DetailNo { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubtotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
