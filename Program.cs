using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PurchaseRequestSystem.Data;
using PurchaseRequestSystem.Middlewares;
using PurchaseRequestSystem.Common;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Repositories;
using PurchaseRequestSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Database ----------
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
//    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("PurchaseRequestSystem")));

// ---------- MVC / JSON ----------
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(error =>
                string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "Invalid request payload."
                    : error.ErrorMessage))
            .ToList();

        var response = new ApiResponse<ValidationErrorData>(
            StatusCodes.Status400BadRequest,
            "Validation failed",
            new ValidationErrorData { Errors = errors });

        return new BadRequestObjectResult(response);
    };
});

// ---------- AutoMapper (profiles added in the service layer phase) ----------
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ---------- Authentication / Authorization ----------
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured")))
        };
    });
builder.Services.AddAuthorization();

// ---------- Swagger / OpenAPI ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Purchase Request System API", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the JWT token (without the 'Bearer' prefix)."
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ---------- Repositories & Services registration ----------
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRequestTypeService, RequestTypeService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IStatusLookupService, StatusLookupService>();
builder.Services.AddScoped<IApprovalStageService, ApprovalStageService>();
builder.Services.AddScoped<IUomService, UomService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();

builder.Services.AddScoped<IProposalRepository, ProposalRepository>();
builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<IProcurementRequestRepository, ProcurementRequestRepository>();
builder.Services.AddScoped<IProcurementRequestService, ProcurementRequestService>();
builder.Services.AddScoped<IPurchaseRequestRepository, PurchaseRequestRepository>();
builder.Services.AddScoped<IPurchaseRequestService, PurchaseRequestService>();

var app = builder.Build();

// Global exception handling must run first so it can wrap everything below.
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
