using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseRequestSystem.Models;

[Table("tbl_purchase_request_detail")]
public class PurchaseRequestDetail
{
    public Guid PurchaseRequestDetailId { get; set; }
    public Guid PurchaseRequestId { get; set; }
    /// <summary>Filled for PROJECT flow to enforce approved Proposal Detail remaining quantity.</summary>
    public Guid? ProposalDetailId { get; set; }
    public Guid MaterialId { get; set; }
    public Guid UomId { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public PurchaseRequest PurchaseRequest { get; set; } = null!;
    public ProposalDetail? ProposalDetail { get; set; }
    public Material Material { get; set; } = null!;
    public Uom Uom { get; set; } = null!;
}
