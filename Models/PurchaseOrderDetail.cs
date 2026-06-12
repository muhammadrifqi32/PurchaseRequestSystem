using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseRequestSystem.Models;

[Table("tbl_purchase_order_detail")]
public class PurchaseOrderDetail
{
    public Guid PurchaseOrderDetailId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid MaterialId { get; set; }
    public Guid UomId { get; set; }
    public int DetailNo { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    /// <summary>Stored: Quantity * UnitPrice.</summary>
    public decimal SubtotalAmount { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Material Material { get; set; } = null!;
    public Uom Uom { get; set; } = null!;
    public ICollection<GoodsReceiptDetail> GoodsReceiptDetails { get; set; } = new List<GoodsReceiptDetail>();
}
