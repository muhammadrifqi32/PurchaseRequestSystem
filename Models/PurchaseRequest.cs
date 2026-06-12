using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseRequestSystem.Models;

/// <summary>One-to-one with ProcurementRequest. Approved via GM + Chairman stages.</summary>
[Table("tbl_purchase_request")]
public class PurchaseRequest
{
    public Guid PurchaseRequestId { get; set; }
    public string? PurchaseRequestNo { get; set; }
    public Guid ProcurementRequestId { get; set; }
    public Guid StatusId { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public ProcurementRequest ProcurementRequest { get; set; } = null!;
    public Status Status { get; set; } = null!;
    public PurchaseOrder? PurchaseOrder { get; set; }
    public ICollection<PurchaseRequestDetail> Details { get; set; } = new List<PurchaseRequestDetail>();
}
