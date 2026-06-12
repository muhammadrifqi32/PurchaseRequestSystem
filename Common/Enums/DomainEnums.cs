namespace PurchaseRequestSystem.Common.Enums;

/// <summary>
/// Used by tbl_activity_log.document_type.
/// </summary>
public enum DocumentType
{
    PROPOSAL,
    PROCUREMENT_REQUEST,
    PURCHASE_REQUEST,
    PURCHASE_ORDER,
    GOODS_RECEIPT,
    PURCHASE_ORDER_PAYMENT
}

/// <summary>
/// Used by tbl_goods_receipt_detail.discrepancy_type (nullable).
/// </summary>
public enum DiscrepancyType
{
    /// <summary>Received LESS than ordered.</summary>
    SHORTAGE,
    /// <summary>Received MORE than ordered.</summary>
    EXCESS,
    /// <summary>Correct qty but items broken/unusable.</summary>
    DAMAGED,
    /// <summary>A completely different item arrived.</summary>
    WRONG_ITEM,
    /// <summary>Anything that does not fit the categories above.</summary>
    OTHER
}

/// <summary>
/// Present in the DBML but the corresponding column in tbl_proposal_detail is
/// commented out, so it is NOT mapped to a column. Kept here for use in service
/// logic / DTOs if a per-line proposal status is later introduced.
/// </summary>
public enum ProposalDetailStatus
{
    PENDING,
    APPROVED,
    PARTIALLY_APPROVED,
    REJECTED
}
