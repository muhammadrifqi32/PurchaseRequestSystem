/*
    Phase 2 schema additions for MSSQL.
    Preferred approach during development:
      dotnet ef migrations add Phase2ProposalProcurementPurchaseRequest
      dotnet ef database update

    Use this SQL only if you need to patch an existing local DB manually.
*/

IF COL_LENGTH('tbl_proposal_detail', 'StatusId') IS NULL
BEGIN
    ALTER TABLE tbl_proposal_detail ADD StatusId uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_tbl_proposal_detail_StatusId'
      AND object_id = OBJECT_ID('tbl_proposal_detail')
)
BEGIN
    CREATE INDEX IX_tbl_proposal_detail_StatusId ON tbl_proposal_detail(StatusId);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_tbl_proposal_detail_tbl_status_StatusId'
)
BEGIN
    ALTER TABLE tbl_proposal_detail
    ADD CONSTRAINT FK_tbl_proposal_detail_tbl_status_StatusId
    FOREIGN KEY (StatusId) REFERENCES tbl_status(StatusId);
END;

IF COL_LENGTH('tbl_purchase_request', 'PurchaseRequestNo') IS NULL
BEGIN
    ALTER TABLE tbl_purchase_request ADD PurchaseRequestNo nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_tbl_purchase_request_PurchaseRequestNo'
      AND object_id = OBJECT_ID('tbl_purchase_request')
)
BEGIN
    CREATE UNIQUE INDEX IX_tbl_purchase_request_PurchaseRequestNo
    ON tbl_purchase_request(PurchaseRequestNo)
    WHERE PurchaseRequestNo IS NOT NULL;
END;

IF COL_LENGTH('tbl_purchase_request_detail', 'ProposalDetailId') IS NULL
BEGIN
    ALTER TABLE tbl_purchase_request_detail ADD ProposalDetailId uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_tbl_purchase_request_detail_ProposalDetailId'
      AND object_id = OBJECT_ID('tbl_purchase_request_detail')
)
BEGIN
    CREATE INDEX IX_tbl_purchase_request_detail_ProposalDetailId ON tbl_purchase_request_detail(ProposalDetailId);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_tbl_purchase_request_detail_tbl_proposal_detail_ProposalDetailId'
)
BEGIN
    ALTER TABLE tbl_purchase_request_detail
    ADD CONSTRAINT FK_tbl_purchase_request_detail_tbl_proposal_detail_ProposalDetailId
    FOREIGN KEY (ProposalDetailId) REFERENCES tbl_proposal_detail(ProposalDetailId);
END;
