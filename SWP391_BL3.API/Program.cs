using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SWP391_BL3.BLL.Configurations;
using SWP391_BL3.DAL.Data;
using SWP391_BL3.API.Hubs;
using SWP391_BL3.DAL.Repositories.Implementations;
using SWP391_BL3.DAL.Repositories.Interfaces;
using SWP391_BL3.BLL.Services.Implementations;
using SWP391_BL3.BLL.Services.Interfaces;
using System.Text;

// Npgsql DateTime compatibility with existing timestamp columns
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

/*
 * ============================================
 * KI?N TR?C H? TH?NG - TR? L?I C?U H?I H?I ??NG
 * ============================================
 * 
 * Q1: T?i sao ch?n ki?n tr?c Repository-Service-Controller?
 * A1: 
 * - Repository Pattern: T?ch bi?t logic truy c?p database, d? test v? maintain
 * - Service Layer: X? l? business logic ph?c t?p, c? th? reuse gi?a c?c controller
 * - Controller: Ch? x? l? HTTP request/response, kh?ng ch?a business logic
 * - Uu di?m: Separation of Concerns, d? test, d? m? r?ng
 * 
 * Q2: C? s? d?ng Unit of Work kh?ng?
 * A2: C? - IUnitOfWork trong DAL ?? qu?n l? transaction t?p trung.
 * - BookingService / FeedbackService d?ng IUnitOfWork thay v? inject DbContext
 * - ??m b?o BLL kh?ng ph? thu?c tr?c ti?p EF Core
 */

var builder = WebApplication.CreateBuilder(args);
var secretKey = builder.Configuration["JwtSettings:SecretKey"];
var issuer = builder.Configuration["JwtSettings:Issuer"];
var audience = builder.Configuration["JwtSettings:Audience"];

// Add services to the container.
builder.Services.AddControllers();

/*
 * Q3: C? rate limiting kh?ng? L?m sao ch?ng spam/abuse?
 * A3: Hi?n t?i CHUA c? rate limiting. 
 * - C? th? th?m: AspNetCoreRateLimit package
 * - N?n implement: Gi?i h?n s? request/gi? cho m?i IP/user
 * - Production: N?n d?ng API Gateway (Azure API Management, AWS API Gateway)
 */
// ? FIX CORS - Th?m c? localhost:5173 (Vite) v? localhost:8080
// Q4: CORS configuration c? an to?n kh?ng?
// A4: Hi?n t?i ch? cho ph?p localhost (development). 
// Production: N?n ch? d?nh domain c? th? thay v? AllowAnyHeader/Method
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",  // ? Vite dev server
                "http://localhost:8080",  // ? Production/other port
                "http://localhost:3000"   // ? Create React App (n?u c?n)
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

/*
 * Q5: C? API versioning kh?ng?
 * A5: Hi?n t?i CHUA c? versioning (ch? v1).
 * - N?u thay d?i API: Client cu s? b? ?nh hu?ng
 * - N?n implement: URL versioning (/api/v1/booking) ho?c Header versioning
 * - Package: Microsoft.AspNetCore.Mvc.Versioning
 */
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BE_SWP391 API",
        Version = "v1",
        Description = "API documentation for FPT_BOOKING"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

/*
 * Q6: Database connection string c? an to?n kh?ng?
 * A6: Connection string du?c luu trong appsettings.json (kh?ng commit v?o git)
 * - Development: OK (d?ng User Secrets ho?c appsettings.Development.json)
 * - Production: N?n d?ng Azure Key Vault, AWS Secrets Manager, ho?c Environment Variables
 */
builder.Services.AddDbContext<FptBookingContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

/*
 * Q7: T?i sao d?ng AddScoped cho Repository v? Service?
 * A7: 
 * - Scoped: M?t instance cho m?i HTTP request (ph? h?p v?i DbContext)
 * - Singleton: M?t instance cho to?n b? app (kh?ng ph? h?p v? DbContext kh?ng thread-safe)
 * - Transient: M?t instance m?i m?i l?n inject (t?n t?i nguy?n, kh?ng c?n thi?t)
 * - Repository v? Service n?n l? Scoped d? share c?ng m?t DbContext trong m?t request
 */
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFacilityTypeRepository, FacilityTypeRepository>();
builder.Services.AddScoped<IFacilityTypeService, FacilityTypeService>();
builder.Services.AddScoped<IFacilityService, FacilityService>();
builder.Services.AddScoped<IFacilityRepository, FacilityRepository>();
builder.Services.AddScoped<ICampusRepository, CampusRepository>();
builder.Services.AddScoped<ICampusService, CampusService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<ISlotRepository, SlotRepository>();
builder.Services.AddScoped<ISlotService, SlotService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICheckinRepository, CheckinRepository>();
builder.Services.AddScoped<ICheckoutRepository, CheckoutRepository>();

builder.Services.AddSignalR();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<JwtSettings>(builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>());
builder.Services.AddScoped<JwtTokenGenerator>();

/*
 * Q8: JWT token c? refresh token kh?ng?
 * A8: Hi?n t?i CHUA c? refresh token.
 * - Access token: Expires sau 1 gi? (trong JwtTokenGenerator)
 * - V?n d?: User ph?i login l?i sau 1 gi?
 * - N?n implement: Refresh token (expires sau 7-30 ng?y) d? t? d?ng renew access token
 * 
 * Q9: C? x? l? token b? d?nh c?p kh?ng?
 * A9: CHUA c? token blacklist/revocation.
 * - N?u token b? leak: Ph?i d?i h?t h?n (1 gi?)
 * - N?n implement: Token blacklist trong Redis/database
 * - Ho?c: Short-lived token (15 ph?t) + refresh token
 */
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,        // Validate issuer d? ch?ng token gi? m?o
            ValidateAudience = true,      // Validate audience
            ValidateLifetime = true,      // Validate expiration time
            ValidateIssuerSigningKey = true, // Validate signature
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

/*
 * Q10: C? authorization (ph?n quy?n) kh?ng?
 * A10: C? AddAuthorization() nhung CHUA implement policy c? th?.
 * - N?n th?m: [Authorize(Roles = "Admin")] cho c?c endpoint c?n quy?n
 * - V? d?: Ch? Admin m?i du?c duy?t booking
 * - C? th? d?ng: Policy-based authorization cho logic ph?c t?p hon
 */
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.

{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();

/*
 * Q11: Middleware pipeline order c? d?ng kh?ng?
 * A11: ??ng th? t?:
 * 1. CORS tru?c Authentication (d? browser c? th? g?i preflight request)
 * 2. Authentication tru?c Authorization (ph?i bi?t user l? ai tru?c khi check quy?n)
 * 3. Authorization tru?c Controllers (check quy?n tru?c khi v?o controller)
 */
// ? IMPORTANT: UseCors PH?I d?t TRU?C UseAuthentication v? UseAuthorization
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//// ? Map SignalR Hub v?i CORS
//app.MapHub<NotificationHub>("/notificationHub").RequireCors("AllowReactApp");

app.Run();