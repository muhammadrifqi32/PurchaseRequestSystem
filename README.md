# Purchase Request System — Backend (ASP.NET Core 9 + SQL Server)

Phase 1 deliverable: project skeleton, configuration, all entity models, `AppDbContext`
with Fluent API configuration, custom exceptions, global exception middleware, the
`ApiResponse` wrapper, and seed data.

## 1. Prerequisites
- .NET 9 SDK
- SQL Server / SQL Server Express / LocalDB
- EF Core CLI tools: `dotnet tool install --global dotnet-ef` (or `dotnet tool update --global dotnet-ef`)

## 2. Configure the database
Edit `appsettings.json` → `ConnectionStrings:DefaultConnection` and `Jwt:Key`.

## 3. Restore & build
```bash
dotnet restore
dotnet build
```

## 4. Create the database (Code First migration)
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```
The `InitialCreate` migration includes the seed rows for roles, request types,
statuses, and approval stages (via `HasData`).

## 5. Run
```bash
dotnet run
```
Swagger UI: `https://localhost:<port>/swagger`

## Conventions used
- **Table names**: explicit `[Table("tbl_xxx")]` on each entity.
- **Column names**: auto snake_case via `EFCore.NamingConventions`
  (`PurchaseOrderId` → `purchase_order_id`).
- **Primary keys**: `Guid`.
- **Money**: `decimal(18,2)`; **quantities**: `decimal(18,4)`; **tax rate**: `decimal(9,4)`.
- **Enums** (`document_type`, `discrepancy_type`): stored as `varchar` via
  `.HasConversion<string>()` (portable, human-readable).
- **Audit FKs** (`created_by`, `updated_by`, `requester_id`, `received_by`): real
  FKs to `tbl_account`, configured **without** navigation properties and with
  `DeleteBehavior.Restrict` to avoid ambiguous multiple-cascade-path errors.
- **Delete behavior**: `Restrict` everywhere (soft delete via `is_deleted` instead
  of cascade).

## Roadmap (next phases)
2. DTOs (create/update/response) + AutoMapper profiles + FluentValidation validators
3. `IGenericRepository` + entity repositories
4. Service layer with the business rules (proposal review, remaining-qty check,
   GM/Chairman approval, PO generation, GR discrepancy, payment status)
5. Controllers (master data CRUD + all transaction endpoints + dashboard)
6. Auth (login, JWT issuance, role-based `[Authorize]`)
