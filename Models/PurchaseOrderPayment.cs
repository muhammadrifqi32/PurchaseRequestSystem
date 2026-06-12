using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseRequestSystem.Models;

/// <summary>One PO can have many payment records (partial to full). Optionally linked to a GR.</summary>
[Table("tbl_purchase_order_payment")]
public class PurchaseOrderPayment
{
    public Guid PurchaseOrderPaymentId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    /// <summary>Nullable: links the payment to the GR that triggered it.</summary>
    public Guid? GoodsReceiptId { get; set; }
    public string? InvoiceNo { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentReferenceNo { get; set; }
    public string? PaymentProofPath { get; set; }
    public Guid StatusId { get; set; }
    public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public GoodsReceipt? GoodsReceipt { get; set; }
    public Status Status { get; set; } = null!;
}
