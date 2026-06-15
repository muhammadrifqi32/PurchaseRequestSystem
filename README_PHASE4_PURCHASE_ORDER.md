# Phase 4 — Purchase Order Generation Testing Guide

Phase 4 implements Purchase Order generation from an approved Purchase Request.

## 1. Run Project

```bash
dotnet restore
dotnet build
dotnet run
```

Swagger should show these endpoints:

```text
GET    /api/purchase-orders
GET    /api/purchase-orders/{id}
GET    /api/purchase-orders/by-purchase-request/{purchaseRequestId}
POST   /api/purchase-orders/from-purchase-request/{purchaseRequestId}
PUT    /api/purchase-orders/{id}
DELETE /api/purchase-orders/{id}
```

## 2. Run Optional Database Patch

Run this script in SSMS if your database was created before Phase 4 or if you need dummy vendor/company data:

```text
Database/Phase4_MSSQL_PurchaseOrderPatch.sql
```

This script:

```text
- Ensures OPEN status exists
- Makes tbl_purchase_order.VendorId nullable
- Makes tbl_purchase_order.CompanyId nullable
- Inserts dummy vendor
- Inserts dummy company
```

Dummy IDs:

```text
VendorId:
55555555-5555-5555-5555-555555555501

CompanyId:
55555555-5555-5555-5555-555555555502

CreatedBy / UpdatedBy / requester account:
aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1
```

## 3. Prerequisite Before Generate PO

Purchase Request must already be fully approved.

Valid status:

```text
APPROVED
```

Example from Phase 3:

```text
DRAFT → PENDING_GM_APPROVAL → PENDING_CHAIRMAN_CONFIRMATION → APPROVED
```

Use `GET /api/procurement-requests` or `GET /api/purchase-requests/{id}` to find an approved `purchaseRequestId` and its `purchaseRequestDetailId`.

## 4. Generate PO from Approved Purchase Request

Endpoint:

```text
POST /api/purchase-orders/from-purchase-request/{purchaseRequestId}
```

Example body:

```json
{
  "vendorId": "55555555-5555-5555-5555-555555555501",
  "companyId": "55555555-5555-5555-5555-555555555502",
  "poDate": "2026-06-15",
  "taxRate": 0.11,
  "notes": "PO generated from approved PR",
  "purchaseOrderAttachmentPath": "optional/path/po.pdf",
  "createdBy": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1",
  "details": [
    {
      "purchaseRequestDetailId": "replace-with-purchase-request-detail-id",
      "unitPrice": 15000,
      "notes": "Unit price for material"
    }
  ]
}
```

Expected response:

```text
201 Created
```

Expected important fields:

```text
purchaseOrderNo = PO-YYYYMM-0001
status = OPEN
subtotalAmount = quantity x unitPrice
TaxAmount = subtotalAmount x taxRate
GrandtotalAmount = subtotalAmount + taxAmount
```

## 5. Duplicate PO Test

Run the same generate PO request again for the same `purchaseRequestId`.

Expected:

```text
400 Bad Request
Purchase Order already exists for this Purchase Request
```

## 6. Negative Test: PR Not Approved

Try generate PO from a Purchase Request whose status is not `APPROVED`.

Expected:

```text
400 Bad Request
Purchase Request is not approved yet
```

## 7. Get PO

```text
GET /api/purchase-orders
GET /api/purchase-orders/{purchaseOrderId}
GET /api/purchase-orders/by-purchase-request/{purchaseRequestId}
```

Expected:

```text
statusCode = 200
status = OPEN
details are visible
vendorName/companyName are visible if provided
```

## 8. Update PO

Only `OPEN` Purchase Order can be updated.

Endpoint:

```text
PUT /api/purchase-orders/{purchaseOrderId}
```

Example body:

```json
{
  "vendorId": "55555555-5555-5555-5555-555555555501",
  "companyId": "55555555-5555-5555-5555-555555555502",
  "poDate": "2026-06-15",
  "taxRate": 0.11,
  "notes": "Updated PO notes",
  "purchaseOrderAttachmentPath": "optional/path/updated-po.pdf",
  "updatedBy": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1",
  "details": [
    {
      "purchaseOrderDetailId": "replace-with-purchase-order-detail-id",
      "unitPrice": 17500,
      "notes": "Updated unit price"
    }
  ]
}
```

Expected:

```text
200 OK
Totals recalculated
```

## 9. Delete PO

Only `OPEN` Purchase Order can be deleted.

Endpoint:

```text
DELETE /api/purchase-orders/{purchaseOrderId}
```

Expected:

```text
200 OK
```

Later, when Goods Receipt exists, delete should be blocked if PO already has GR.

## 10. Phase 4 Pass Criteria

Phase 4 is considered working if:

```text
GET /api/purchase-orders works
Generate PO from APPROVED PR works
PO details copied from PR details
Subtotal, tax amount, and grand total are correct
Duplicate PO generation returns 400
Generate PO from non-approved PR returns 400
GET PO by ID works
GET PO by PurchaseRequestId works
Update OPEN PO works
Delete OPEN PO works
```
