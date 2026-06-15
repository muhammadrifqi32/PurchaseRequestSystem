/* =========================================================
   PHASE 4 MANUAL PATCH - PURCHASE ORDER - SQL SERVER
   =========================================================
   Use this only if your database was created before Phase 4
   or if you want deterministic dummy data for testing.
*/

/* 1. Ensure OPEN status exists */
IF NOT EXISTS (SELECT 1 FROM tbl_status WHERE StatusName = 'OPEN')
BEGIN
    INSERT INTO tbl_status (StatusId, StatusName, IsDeleted, CreatedAt, UpdatedAt)
    VALUES ('44444444-4444-4444-4444-444444444409', 'OPEN', 0, GETDATE(), NULL);
END;

/* 2. Make PO recipient columns nullable so PO can link to either vendor or company.
      Skip safely if the table does not exist yet. */
IF OBJECT_ID('tbl_purchase_order', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('tbl_purchase_order', 'VendorId') IS NOT NULL
    BEGIN
        ALTER TABLE tbl_purchase_order ALTER COLUMN VendorId UNIQUEIDENTIFIER NULL;
    END;

    IF COL_LENGTH('tbl_purchase_order', 'CompanyId') IS NOT NULL
    BEGIN
        ALTER TABLE tbl_purchase_order ALTER COLUMN CompanyId UNIQUEIDENTIFIER NULL;
    END;
END;

/* 3. Ensure deterministic dummy vendor exists for Swagger testing */
IF OBJECT_ID('tbl_vendor', 'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM tbl_vendor WHERE VendorId = '55555555-5555-5555-5555-555555555501')
BEGIN
    INSERT INTO tbl_vendor (
        VendorId,
        VendorCode,
        VendorName,
        IsDeleted,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    )
    VALUES (
        '55555555-5555-5555-5555-555555555501',
        'VND001',
        'PT Test Vendor',
        0,
        GETDATE(),
        NULL,
        NULL,
        NULL
    );
END;

/* 4. Ensure deterministic dummy company exists for Swagger testing */
IF OBJECT_ID('tbl_company', 'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM tbl_company WHERE CompanyId = '55555555-5555-5555-5555-555555555502')
BEGIN
    INSERT INTO tbl_company (
        CompanyId,
        CompanyCode,
        CompanyName,
        IsDeleted,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    )
    VALUES (
        '55555555-5555-5555-5555-555555555502',
        'CMP-PO-TEST',
        'PT Test PO Company',
        0,
        GETDATE(),
        NULL,
        NULL,
        NULL
    );
END;

/* 5. Verify required data */
SELECT StatusId, StatusName FROM tbl_status WHERE StatusName IN ('OPEN', 'APPROVED');
SELECT VendorId, VendorCode, VendorName FROM tbl_vendor WHERE VendorId = '55555555-5555-5555-5555-555555555501';
SELECT CompanyId, CompanyCode, CompanyName FROM tbl_company WHERE CompanyId = '55555555-5555-5555-5555-555555555502';
