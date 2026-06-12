# Purchase Request System Backend

## Phase 1 & Phase 2 Testing Guide

Dokumen ini digunakan untuk membantu tim memahami, menjalankan, dan melakukan testing backend Purchase Request System sampai Phase 2.

Backend saat ini mencakup:

1. **Phase 1 — Master Data CRUD**
2. **Phase 2 — Proposal, Procurement Request, dan Purchase Request**

Belum termasuk:

* Authentication / Login
* Purchase Order
* Goods Receipt
* Payment
* Dashboard
* Approval lanjutan

Database yang digunakan saat ini: **MSSQL / SQL Server**.

---

# 1. Prerequisite

Pastikan environment berikut sudah tersedia:

* .NET SDK sesuai versi project
* SQL Server / MSSQL
* SSMS atau database client lain
* Swagger dapat diakses setelah project running

Jalankan command berikut:

```bash
dotnet restore
dotnet build
dotnet run
```

Expected:

* Build berhasil
* API running
* Swagger muncul
* Tidak ada error connection string

---

# 2. Database Schema Check

Pastikan database sudah memiliki table Phase 1 dan Phase 2.

## Phase 1 Tables

Minimal harus ada:

```text
tbl_company
tbl_vendor
tbl_role
tbl_request_type
tbl_status
tbl_approval_stage
tbl_uom
tbl_material
tbl_account
tbl_user_detail
```

## Phase 2 Tables

Minimal harus ada:

```text
tbl_proposal
tbl_proposal_detail
tbl_procurement_request
tbl_purchase_request
tbl_purchase_request_detail
```

---

# 3. Phase 2 Database Patch

Sebelum melakukan testing Phase 2, pastikan schema database sudah sinkron dengan code Phase 2.

Pada Phase 2 terdapat beberapa tambahan column yang wajib ada di database:

```text
tbl_proposal_detail.StatusId
tbl_purchase_request.PurchaseRequestNo
tbl_purchase_request_detail.ProposalDetailId
```

Jika column tersebut belum ada, endpoint seperti berikut bisa error:

```text
GET /api/proposals
GET /api/procurement-requests
POST /api/proposals
POST /api/procurement-requests/project
```

Contoh error:

```json
{
  "statusCode": 500,
  "message": "Invalid column name 'StatusId'.\r\nInvalid column name 'StatusId'.",
  "data": null
}
```

Atau:

```text
Invalid column name 'PurchaseRequestNo'
```

## 3.1 Run Phase 2 Manual Patch

Jalankan script berikut di SSMS pada database yang digunakan oleh aplikasi.

```sql
/* =========================================================
   PHASE 2 MANUAL PATCH - SAFE VERSION FOR SQL SERVER
   ========================================================= */

-- 1. Add StatusId to tbl_proposal_detail
IF OBJECT_ID('tbl_proposal_detail', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('tbl_proposal_detail', 'StatusId') IS NULL
    BEGIN
        ALTER TABLE tbl_proposal_detail 
        ADD StatusId UNIQUEIDENTIFIER NULL;
    END;
END;

-- 2. Add PurchaseRequestNo to tbl_purchase_request
IF OBJECT_ID('tbl_purchase_request', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('tbl_purchase_request', 'PurchaseRequestNo') IS NULL
    BEGIN
        ALTER TABLE tbl_purchase_request 
        ADD PurchaseRequestNo NVARCHAR(450) NULL;
    END;
END;

-- 3. Add ProposalDetailId to tbl_purchase_request_detail
IF OBJECT_ID('tbl_purchase_request_detail', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('tbl_purchase_request_detail', 'ProposalDetailId') IS NULL
    BEGIN
        ALTER TABLE tbl_purchase_request_detail 
        ADD ProposalDetailId UNIQUEIDENTIFIER NULL;
    END;
END;
```

## 3.2 Verify Column Patch

Setelah menjalankan patch di atas, jalankan query berikut:

```sql
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (
    'tbl_proposal_detail',
    'tbl_purchase_request',
    'tbl_purchase_request_detail'
)
AND COLUMN_NAME IN (
    'StatusId',
    'PurchaseRequestNo',
    'ProposalDetailId'
)
ORDER BY TABLE_NAME, COLUMN_NAME;
```

Expected result:

```text
tbl_proposal_detail          StatusId
tbl_purchase_request         PurchaseRequestNo
tbl_purchase_request_detail  ProposalDetailId
```

Jika 3 column tersebut sudah muncul, lanjut ke step berikutnya.

## 3.3 Add Index and Foreign Key

Setelah column berhasil dibuat, jalankan script berikut:

```sql
-- Index for tbl_proposal_detail.StatusId
IF OBJECT_ID('tbl_proposal_detail', 'U') IS NOT NULL
AND COL_LENGTH('tbl_proposal_detail', 'StatusId') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 
    FROM sys.indexes
    WHERE name = 'IX_tbl_proposal_detail_StatusId'
      AND object_id = OBJECT_ID('tbl_proposal_detail')
)
BEGIN
    EXEC('CREATE INDEX IX_tbl_proposal_detail_StatusId ON tbl_proposal_detail(StatusId)');
END;

-- FK tbl_proposal_detail.StatusId -> tbl_status.StatusId
IF OBJECT_ID('tbl_proposal_detail', 'U') IS NOT NULL
AND OBJECT_ID('tbl_status', 'U') IS NOT NULL
AND COL_LENGTH('tbl_proposal_detail', 'StatusId') IS NOT NULL
AND NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_tbl_proposal_detail_tbl_status_StatusId'
)
BEGIN
    ALTER TABLE tbl_proposal_detail
    ADD CONSTRAINT FK_tbl_proposal_detail_tbl_status_StatusId
    FOREIGN KEY (StatusId) REFERENCES tbl_status(StatusId);
END;

-- Unique index for tbl_purchase_request.PurchaseRequestNo
IF OBJECT_ID('tbl_purchase_request', 'U') IS NOT NULL
AND COL_LENGTH('tbl_purchase_request', 'PurchaseRequestNo') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 
    FROM sys.indexes
    WHERE name = 'IX_tbl_purchase_request_PurchaseRequestNo'
      AND object_id = OBJECT_ID('tbl_purchase_request')
)
BEGIN
    EXEC('CREATE UNIQUE INDEX IX_tbl_purchase_request_PurchaseRequestNo 
          ON tbl_purchase_request(PurchaseRequestNo)
          WHERE PurchaseRequestNo IS NOT NULL');
END;

-- Index for tbl_purchase_request_detail.ProposalDetailId
IF OBJECT_ID('tbl_purchase_request_detail', 'U') IS NOT NULL
AND COL_LENGTH('tbl_purchase_request_detail', 'ProposalDetailId') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 
    FROM sys.indexes
    WHERE name = 'IX_tbl_purchase_request_detail_ProposalDetailId'
      AND object_id = OBJECT_ID('tbl_purchase_request_detail')
)
BEGIN
    EXEC('CREATE INDEX IX_tbl_purchase_request_detail_ProposalDetailId 
          ON tbl_purchase_request_detail(ProposalDetailId)');
END;

-- FK tbl_purchase_request_detail.ProposalDetailId -> tbl_proposal_detail.ProposalDetailId
IF OBJECT_ID('tbl_purchase_request_detail', 'U') IS NOT NULL
AND OBJECT_ID('tbl_proposal_detail', 'U') IS NOT NULL
AND COL_LENGTH('tbl_purchase_request_detail', 'ProposalDetailId') IS NOT NULL
AND NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_tbl_purchase_request_detail_tbl_proposal_detail_ProposalDetailId'
)
BEGIN
    ALTER TABLE tbl_purchase_request_detail
    ADD CONSTRAINT FK_tbl_purchase_request_detail_tbl_proposal_detail_ProposalDetailId
    FOREIGN KEY (ProposalDetailId) REFERENCES tbl_proposal_detail(ProposalDetailId);
END;
```

## 3.4 Important Note

Script patch dibagi menjadi dua tahap:

1. Add column
2. Add index dan foreign key

Jangan digabung menjadi satu batch besar jika SQL Server menampilkan error seperti:

```text
Invalid column name 'PurchaseRequestNo'
```

Hal tersebut bisa terjadi karena SQL Server melakukan compile script sebelum column baru terbaca dalam batch yang sama.

Gunakan script sesuai urutan di atas.

---

# 4. Required Master Data Before Testing

Sebelum test Phase 2, pastikan master data berikut sudah tersedia.

## 4.1 Status

Pastikan `tbl_status` memiliki status berikut:

```text
DRAFT
SUBMITTED
PENDING
APPROVED
REJECTED
REVISION_REQUIRED
UNDER_REVIEW
PARTIALLY_APPROVED
```

Status yang saat ini sudah digunakan oleh aplikasi:

```text
DRAFT
SUBMITTED
APPROVED
REJECTED
REVISION_REQUIRED
PARTIALLY_APPROVED
UNDER_REVIEW
```

## 4.2 Request Type

Pastikan `tbl_request_type` memiliki:

```text
PROJECT
NON_PROJECT
```

Contoh data:

```text
PROJECT      - Project
NON_PROJECT  - Non-Project
```

## 4.3 Role

Minimal role berikut harus tersedia:

```text
REQUESTER
PROCURE
GM
ADMIN
```

---

# 5. Add Dummy Requester and Reviewer

Karena authentication/login belum diimplementasikan, testing Phase 2 membutuhkan dummy account.

Minimal harus ada:

1. **Requester**

   * Digunakan untuk create Proposal, Procurement Request, dan Purchase Request.
2. **Reviewer / Procure**

   * Digunakan sebagai data dummy reviewer untuk proses review proposal di fase berikutnya atau jika nanti field reviewer digunakan.

## 5.1 Insert Dummy Requester

Jalankan script berikut di SSMS:

```sql
DECLARE @RequesterAccountId UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1';
DECLARE @RequesterUserId UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1';
DECLARE @RequesterRoleId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111101';

IF NOT EXISTS (
    SELECT 1 
    FROM tbl_account 
    WHERE AccountId = @RequesterAccountId
)
BEGIN
    INSERT INTO tbl_account (
        AccountId,
        RoleId,
        Email,
        Password,
        LastLoginAt,
        IsDeleted,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    )
    VALUES (
        @RequesterAccountId,
        @RequesterRoleId,
        'requester@test.com',
        'DUMMY_PASSWORD_NOT_USED_YET',
        NULL,
        0,
        GETDATE(),
        NULL,
        NULL,
        NULL
    );
END;

IF NOT EXISTS (
    SELECT 1 
    FROM tbl_user_detail 
    WHERE UserId = @RequesterUserId
)
BEGIN
    INSERT INTO tbl_user_detail (
        UserId,
        AccountId,
        FullName,
        Phone,
        CreatedAt,
        UpdatedAt
    )
    VALUES (
        @RequesterUserId,
        @RequesterAccountId,
        'Test Requester',
        '081234567890',
        GETDATE(),
        NULL
    );
END;
```

Requester ID yang digunakan untuk testing:

```text
aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1
```

## 5.2 Insert Dummy Reviewer / Procure

Jalankan script berikut di SSMS:

```sql
DECLARE @ProcureAccountId UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2';
DECLARE @ProcureUserId UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2';
DECLARE @ProcureRoleId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111102';

IF NOT EXISTS (
    SELECT 1 
    FROM tbl_account 
    WHERE AccountId = @ProcureAccountId
)
BEGIN
    INSERT INTO tbl_account (
        AccountId,
        RoleId,
        Email,
        Password,
        LastLoginAt,
        IsDeleted,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    )
    VALUES (
        @ProcureAccountId,
        @ProcureRoleId,
        'procure@test.com',
        'DUMMY_PASSWORD_NOT_USED_YET',
        NULL,
        0,
        GETDATE(),
        NULL,
        NULL,
        NULL
    );
END;

IF NOT EXISTS (
    SELECT 1 
    FROM tbl_user_detail 
    WHERE UserId = @ProcureUserId
)
BEGIN
    INSERT INTO tbl_user_detail (
        UserId,
        AccountId,
        FullName,
        Phone,
        CreatedAt,
        UpdatedAt
    )
    VALUES (
        @ProcureUserId,
        @ProcureAccountId,
        'Test Procure',
        '081234567891',
        GETDATE(),
        NULL
    );
END;
```

Reviewer / Procure ID:

```text
aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2
```

## 5.3 Verify Dummy Accounts

Jalankan query berikut:

```sql
SELECT 
    a.AccountId,
    a.Email,
    r.RoleCode,
    r.RoleName,
    ud.UserId,
    ud.FullName,
    ud.Phone
FROM tbl_account a
JOIN tbl_role r ON r.RoleId = a.RoleId
LEFT JOIN tbl_user_detail ud ON ud.AccountId = a.AccountId
WHERE a.Email IN (
    'requester@test.com',
    'procure@test.com'
)
ORDER BY a.Email;
```

Expected result:

```text
requester@test.com  REQUESTER  Test Requester
procure@test.com    PROCURE    Test Procure
```

---

# 6. Current Sample Data for Testing

Gunakan data berikut untuk testing awal.

## Requester

```json
"requesterId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"
```

## Reviewer / Procure

```json
"reviewerId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"
```

Note:

Saat ini review endpoint belum wajib menggunakan reviewer karena authentication belum dibuat. ID reviewer disiapkan untuk kebutuhan testing dan pengembangan berikutnya.

## UoM

```json
"uomId": "1bf2adcd-7f2f-4b43-a434-b1e01d706c49"
```

## Material

```json
"materialId": "06de6f35-a3b2-4cab-a815-0bd26179129f"
```

Material:

```text
MAT001 - Cement
UoM: PCS - Pieces
```

---

# 7. API Response Standard

Semua API response harus mengikuti format berikut:

```json
{
  "statusCode": 200,
  "message": "Data retrieved successfully",
  "data": {}
}
```

Untuk error:

```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "data": null
}
```

Atau:

```json
{
  "statusCode": 404,
  "message": "Data not found",
  "data": null
}
```

System error tidak boleh menampilkan stack trace mentah.

---

# 8. Phase 1 Testing — Master Data CRUD

## 8.1 Endpoint yang harus muncul di Swagger

```text
GET    /api/companies
POST   /api/companies
GET    /api/companies/{id}
PUT    /api/companies/{id}
DELETE /api/companies/{id}

GET    /api/uoms
POST   /api/uoms
GET    /api/uoms/{id}
PUT    /api/uoms/{id}
DELETE /api/uoms/{id}

GET    /api/materials
POST   /api/materials
GET    /api/materials/{id}
PUT    /api/materials/{id}
DELETE /api/materials/{id}

GET    /api/statuses
GET    /api/request-types
GET    /api/roles
GET    /api/vendors
GET    /api/approval-stages
```

## 8.2 GET All Master Data

Test:

```text
GET /api/companies
GET /api/uoms
GET /api/materials
GET /api/statuses
GET /api/request-types
GET /api/roles
```

Expected:

```json
{
  "statusCode": 200,
  "message": "Data retrieved successfully",
  "data": []
}
```

Data boleh kosong atau berisi data. Yang penting tidak error.

## 8.3 Create Company

Endpoint:

```text
POST /api/companies
```

Body:

```json
{
  "companyCode": "CMP001",
  "companyName": "PT Test Company"
}
```

Expected:

```text
201 Created
```

## 8.4 Create UoM

Endpoint:

```text
POST /api/uoms
```

Body:

```json
{
  "uomCode": "PCS",
  "uomName": "Pieces"
}
```

Expected:

```text
201 Created
```

## 8.5 Create Material with Valid UoM

Endpoint:

```text
POST /api/materials
```

Body:

```json
{
  "materialCode": "MAT001",
  "materialName": "Cement",
  "uomId": "1bf2adcd-7f2f-4b43-a434-b1e01d706c49"
}
```

Expected:

```text
201 Created
```

## 8.6 Create Material with Invalid UoM

Endpoint:

```text
POST /api/materials
```

Body:

```json
{
  "materialCode": "MAT002",
  "materialName": "Steel",
  "uomId": "11111111-1111-1111-1111-111111111111"
}
```

Expected:

```text
400 Bad Request
```

## 8.7 Duplicate Code Validation

Create Company dengan `companyCode` yang sama dua kali.

Expected untuk request kedua:

```text
400 Bad Request
```

## 8.8 Phase 1 Pass Criteria

Phase 1 dianggap aman jika:

```text
dotnet build berhasil
dotnet run berhasil
Swagger muncul
GET semua master data berhasil
POST Company berhasil
POST UoM berhasil
POST Material dengan UoM valid berhasil
POST Material dengan UoM invalid return 400
Duplicate code return 400
GET by invalid ID return 404
Status master tersedia
RequestType master tersedia
Requester dummy tersedia
```

---

# 9. Phase 2 Testing — Proposal, Procurement Request, Purchase Request

Phase 2 terdiri dari dua flow:

1. **Project Flow**

   * Proposal dibuat
   * Proposal disubmit
   * Proposal direview / approved
   * Procurement Request dibuat dari Proposal
   * Purchase Request otomatis dibuat
   * Purchase Request bisa disubmit

2. **Non-Project Flow**

   * Procurement Request langsung dibuat tanpa Proposal
   * Purchase Request otomatis dibuat

---

# 10. Phase 2 Endpoint List

Pastikan endpoint berikut muncul di Swagger.

## Proposal

```text
GET    /api/proposals
GET    /api/proposals/{id}
POST   /api/proposals
PUT    /api/proposals/{id}
DELETE /api/proposals/{id}
POST   /api/proposals/{id}/submit
POST   /api/proposals/{id}/review
```

## Procurement Request

```text
GET    /api/procurement-requests
GET    /api/procurement-requests/{id}
POST   /api/procurement-requests/project
POST   /api/procurement-requests/non-project
```

## Purchase Request

```text
GET    /api/purchase-requests/{id}
POST   /api/purchase-requests/{id}/submit
```

---

# 11. Test Project Flow

## Step 1 — Create Proposal

Endpoint:

```text
POST /api/proposals
```

Body:

```json
{
  "requesterId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1",
  "proposalDate": "2026-06-12",
  "purpose": "Project material request",
  "proposalAttachmentPath": "optional/path/file.pdf",
  "details": [
    {
      "materialId": "06de6f35-a3b2-4cab-a815-0bd26179129f",
      "uomId": "1bf2adcd-7f2f-4b43-a434-b1e01d706c49",
      "description": "Cement for project",
      "requestedQty": 500
    }
  ]
}
```

Expected:

```text
201 Created
```

Simpan dari response:

```text
proposalId
proposalDetailId
```

Expected status awal:

```text
Proposal status = DRAFT
Proposal detail status = DRAFT
```

## Step 2 — Submit Proposal

Endpoint:

```text
POST /api/proposals/{proposalId}/submit
```

Body:

```json
{}
```

Expected:

```text
200 OK
```

Expected result:

```text
Proposal status = SUBMITTED
submittedAt terisi
```

## Step 3 — Review Proposal

Endpoint:

```text
POST /api/proposals/{proposalId}/review
```

Body:

```json
{
  "status": "APPROVED",
  "notes": "Approved by Procure",
  "details": [
    {
      "proposalDetailId": "isi-dengan-proposal-detail-id",
      "approvedQty": 400,
      "status": "APPROVED"
    }
  ]
}
```

Expected:

```text
200 OK
```

Expected result:

```text
Proposal status = APPROVED
RequestedQty = 500
ApprovedQty = 400
```

## Step 4 — Create Project Procurement Request Batch 1

Endpoint:

```text
POST /api/procurement-requests/project
```

Body:

```json
{
  "proposalId": "isi-dengan-proposal-id-approved",
  "requesterId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1",
  "requestDate": "2026-06-12",
  "details": [
    {
      "proposalDetailId": "isi-dengan-proposal-detail-id-approved",
      "materialId": "06de6f35-a3b2-4cab-a815-0bd26179129f",
      "uomId": "1bf2adcd-7f2f-4b43-a434-b1e01d706c49",
      "description": "Cement request batch 1",
      "quantity": 200,
      "notes": "First batch"
    }
  ]
}
```

Expected:

```text
201 Created
```

Expected result:

```text
Procurement Request created
Purchase Request created
Purchase Request Detail created
Request Type = PROJECT
ProposalId terisi
```

Simpan:

```text
procurementRequestId
purchaseRequestId
purchaseRequestDetailId
```

## Step 5 — Create Project Procurement Request Batch 2

Gunakan proposal dan proposal detail yang sama.

Body:

```json
{
  "proposalId": "isi-dengan-proposal-id-approved",
  "requesterId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1",
  "requestDate": "2026-06-12",
  "details": [
    {
      "proposalDetailId": "isi-dengan-proposal-detail-id-approved",
      "materialId": "06de6f35-a3b2-4cab-a815-0bd26179129f",
      "uomId": "1bf2adcd-7f2f-4b43-a434-b1e01d706c49",
      "description": "Cement request batch 2",
      "quantity": 200,
      "notes": "Second batch"
    }
  ]
}
```

Expected:

```text
201 Created
```

Total quantity sekarang:

```text
Batch 1 = 200
Batch 2 = 200
Total = 400
ApprovedQty = 400
```

Ini masih valid.

## Step 6 — Test Exceed Quantity

Coba request lagi quantity `1` dari proposal detail yang sama.

Body:

```json
{
  "proposalId": "isi-dengan-proposal-id-approved",
  "requesterId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1",
  "requestDate": "2026-06-12",
  "details": [
    {
      "proposalDetailId": "isi-dengan-proposal-detail-id-approved",
      "materialId": "06de6f35-a3b2-4cab-a815-0bd26179129f",
      "uomId": "1bf2adcd-7f2f-4b43-a434-b1e01d706c49",
      "description": "Cement request exceed test",
      "quantity": 1,
      "notes": "Should fail"
    }
  ]
}
```

Expected:

```text
400 Bad Request
```

Reason:

```text
Total requested quantity would exceed approved quantity.
```

Ini adalah business rule terpenting di Phase 2.

## Step 7 — Submit Purchase Request

Endpoint:

```text
POST /api/purchase-requests/{purchaseRequestId}/submit
```

Body:

```json
{}
```

Expected:

```text
200 OK
```

Expected result:

```text
Purchase Request status = SUBMITTED
Procurement Request status = SUBMITTED
submittedAt terisi
```

## Step 8 — Submit Purchase Request Twice

Submit lagi Purchase Request yang sama.

Expected:

```text
400 Bad Request
```

Reason:

```text
Only DRAFT or REVISION_REQUIRED Purchase Request can be submitted.
```

---

# 12. Test Non-Project Flow

Endpoint:

```text
POST /api/procurement-requests/non-project
```

Body:

```json
{
  "requesterId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1",
  "requestDate": "2026-06-12",
  "details": [
    {
      "materialId": "06de6f35-a3b2-4cab-a815-0bd26179129f",
      "uomId": "1bf2adcd-7f2f-4b43-a434-b1e01d706c49",
      "description": "Office cement test item",
      "quantity": 10,
      "notes": "For non-project test"
    }
  ]
}
```

Expected:

```text
201 Created
```

Expected result:

```text
RequestType = NON_PROJECT
proposalId = null
Purchase Request created
Purchase Request Detail created
Status = DRAFT
```

---

# 13. Validation Test

## 13.1 Invalid Quantity

For non-project request, test quantity `0`.

Expected:

```text
400 Bad Request
```

## 13.2 Invalid Requester

Use random requester ID.

Expected:

```text
400 Bad Request
```

or:

```text
404 Not Found
```

The important part: it must not return 500.

## 13.3 Invalid Material

Use random material ID.

Expected:

```text
400 Bad Request
```

or:

```text
404 Not Found
```

## 13.4 Invalid UoM

Use random UoM ID.

Expected:

```text
400 Bad Request
```

or:

```text
404 Not Found
```

## 13.5 Approved Quantity More Than Requested Quantity

When reviewing proposal, set approvedQty greater than requestedQty.

Example:

```json
{
  "status": "APPROVED",
  "notes": "Invalid approved quantity test",
  "details": [
    {
      "proposalDetailId": "isi-dengan-proposal-detail-id",
      "approvedQty": 600,
      "status": "APPROVED"
    }
  ]
}
```

Expected:

```text
400 Bad Request
```

---

# 14. Expected GET Result

## 14.1 GET Proposals

Endpoint:

```text
GET /api/proposals
```

Expected:

```text
statusCode = 200
data contains proposals
proposal has details
statusName is visible
materialName is visible
uomName is visible
approvedQty is visible
remainingQty is visible
```

Example important fields:

```json
{
  "proposalNo": "PROP-202606-0001",
  "statusName": "APPROVED",
  "details": [
    {
      "materialCode": "MAT001",
      "materialName": "Cement",
      "uomCode": "PCS",
      "requestedQty": 500,
      "approvedQty": 400,
      "remainingQty": 0
    }
  ]
}
```

## 14.2 GET Procurement Requests

Endpoint:

```text
GET /api/procurement-requests
```

Expected:

```text
statusCode = 200
data contains procurement requests
requestTypeCode is visible
purchaseRequest is visible
purchaseRequest.details is visible
```

Example important fields:

```json
{
  "procurementRequestNo": "PRQ-202606-0001",
  "requestTypeCode": "PROJECT",
  "proposalNo": "PROP-202606-0001",
  "purchaseRequest": {
    "purchaseRequestNo": "PR-202606-0001",
    "statusName": "DRAFT",
    "details": [
      {
        "materialCode": "MAT001",
        "quantity": 200
      }
    ]
  }
}
```

---

# 15. Phase 2 Pass Criteria

Phase 2 dianggap aman jika:

```text
GET /api/proposals berhasil
POST /api/proposals berhasil
GET /api/proposals/{id} menampilkan details
POST /api/proposals/{id}/submit berhasil
POST /api/proposals/{id}/review berhasil
Review approvedQty > requestedQty return 400
POST /api/procurement-requests/project berhasil
Project PR batch 1 quantity 200 berhasil
Project PR batch 2 quantity 200 berhasil
Project PR exceed approvedQty return 400
GET /api/procurement-requests berhasil
GET /api/procurement-requests/{id} berhasil
GET /api/purchase-requests/{id} berhasil
POST /api/purchase-requests/{id}/submit berhasil
Submit PR dua kali return 400
POST /api/procurement-requests/non-project berhasil
Non-project quantity 0 return 400
Invalid requester tidak return 500
```

---

# 16. Known Notes

1. Auth belum diimplementasikan, jadi `requesterId`, `createdBy`, dan `updatedBy` masih menggunakan dummy account.

2. `reviewerId` atau `procureId` sudah disediakan sebagai dummy reviewer, tetapi belum tentu digunakan langsung oleh endpoint review karena authentication belum dibuat.

3. `updatedBy` saat review bisa null untuk saat ini. Ini bukan blocker.

4. Proposal detail bisa tetap berstatus `DRAFT` saat proposal header sudah `SUBMITTED`. Detail status akan lebih relevan setelah proses review.

5. Untuk Project Flow, `remainingQty` akan berkurang berdasarkan total quantity dari Purchase Request yang sudah dibuat dari proposal detail tersebut.

6. Jika muncul error seperti:

```text
Invalid column name 'StatusId'
Invalid column name 'PurchaseRequestNo'
Invalid column name 'ProposalDetailId'
```

berarti schema database Phase 2 belum sinkron. Pastikan manual database patch Phase 2 sudah dijalankan.

7. Jika muncul error constraint violation saat create proposal atau procurement request, cek kembali:

   * Requester account sudah ada
   * Material sudah ada
   * UoM sudah ada
   * Status master tersedia
   * Request type master tersedia
   * Column patch Phase 2 sudah dijalankan

---

# 17. Recommended Team Testing Order

Gunakan urutan ini agar lebih mudah tracing error:

```text
1. Restore package
2. Build project
3. Run database migration jika dibutuhkan
4. Jalankan Phase 2 manual database patch
5. Verify required Phase 2 columns
6. Insert dummy requester dan dummy reviewer/procure
7. Verify dummy accounts
8. Run project
9. Open Swagger
10. GET all master data
11. Confirm Status, RequestType, Material, UoM, and Requester exist
12. Test Phase 1 Master Data CRUD
13. Create Proposal
14. Submit Proposal
15. Review Proposal as APPROVED
16. Create Project Procurement Request batch 1
17. Create Project Procurement Request batch 2
18. Test exceed quantity and expect 400
19. Submit Purchase Request
20. Test submit Purchase Request twice and expect 400
21. Create Non-Project Procurement Request
22. GET Proposals and Procurement Requests
```

If all steps pass, Phase 1 and Phase 2 can be considered working.