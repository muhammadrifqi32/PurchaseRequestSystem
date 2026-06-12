using System.ComponentModel.DataAnnotations.Schema;
using PurchaseRequestSystem.Common.Enums;

namespace PurchaseRequestSystem.Models;

[Table("tbl_goods_receipt_detail")]
public class GoodsReceiptDetail
{
    public Guid GoodsReceiptDetailId { get; set; }
    public Guid GoodsReceiptId { get; set; }
    /// <summary>Links the received line to the specific PO line for qty matching.</summary>
    public Guid PurchaseOrderDetailId { get; set; }
    public Guid MaterialId { get; set; }
    public Guid UomId { get; set; }
    public int DetailNo { get; set; }
    public decimal ReceivedQty { get; set; }
    public bool IsMatchPo { get; set; }
    public DiscrepancyType? DiscrepancyType { get; set; }
    public string? Remarks { get; set; }
    public string? GoodsReceiptAttachmentPath { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public GoodsReceipt GoodsReceipt { get; set; } = null!;
    public PurchaseOrderDetail PurchaseOrderDetail { get; set; } = null!;
    public Material Material { get; set; } = null!;
    public Uom Uom { get; set; } = null!;
}
