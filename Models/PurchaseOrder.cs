using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseRequestSystem.Models;

/// <summary>One-to-one with PurchaseRequest. Created after PR is fully approved (GM + Chairman).</summary>
[Table("tbl_purchase_order")]
public class PurchaseOrder
{
    public Guid PurchaseOrderId { get; set; }
    public Guid PurchaseRequestId { get; set; }
    public string PurchaseOrderNo { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public Guid CompanyId { get; set; }
    public DateTime PoDate { get; set; }
    public Guid StatusId { get; set; }
    public string? Notes { get; set; }

    /// <summary>SUM of line subtotals before tax.</summary>
    public decimal SubtotalAmount { get; set; }
    /// <summary>e.g. 0.11 for PPN 11%.</summary>
    public decimal TaxRate { get; set; }
    /// <summary>Stored: SubtotalAmount * TaxRate.</summary>
    public decimal TaxAmount { get; set; }
    /// <summary>Stored: SubtotalAmount + TaxAmount. Final invoice amount.</summary>
    public decimal GrandtotalAmount { get; set; }
    public string? PurchaseOrderAttachmentPath { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public PurchaseRequest PurchaseRequest { get; set; } = null!;
    public Vendor Vendor { get; set; } = null!;
    public Company Company { get; set; } = null!;
    public Status Status { get; set; } = null!;
    public ICollection<PurchaseOrderDetail> Details { get; set; } = new List<PurchaseOrderDetail>();
    public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
    public ICollection<PurchaseOrderPayment> Payments { get; set; } = new List<PurchaseOrderPayment>();
}
