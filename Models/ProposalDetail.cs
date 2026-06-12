using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseRequestSystem.Models;

[Table("tbl_proposal_detail")]
public class ProposalDetail
{
    public Guid ProposalDetailId { get; set; }
    public Guid ProposalId { get; set; }
    public Guid MaterialId { get; set; }
    public Guid UomId { get; set; }
    public string? Description { get; set; }
    public decimal RequestedQty { get; set; }
    /// <summary>Set by Procure during review. Drives the remaining-qty business rule for PRs.</summary>
    public decimal ApprovedQty { get; set; }
    public Guid? StatusId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Proposal Proposal { get; set; } = null!;
    public Material Material { get; set; } = null!;
    public Uom Uom { get; set; } = null!;
    public Status? Status { get; set; }
}
