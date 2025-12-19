using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SWP391_BL3.Configurations;
using SWP391_BL3.Data;
using SWP391_BL3.Hubs;
using SWP391_BL3.Repositories.Implementations;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Services.Implementations;
using SWP391_BL3.Services.Interfaces;
using System.Text;

/*
 * ============================================
 * KIẾN TRÚC HỆ THỐNG - TRẢ LỜI CÂU HỎI HỘI ĐỒNG
 * ============================================
 * 
 * Q1: Tại sao chọn kiến trúc Repository-Service-Controller?
 * A1: 
 * - Repository Pattern: Tách biệt logic truy cập database, dễ test và maintain
 * - Service Layer: Xử lý business logic phức tạp, có thể reuse giữa các controller
 * - Controller: Chỉ xử lý HTTP request/response, không chứa business logic
 * - Ưu điểm: Separation of Concerns, dễ test, dễ mở rộng
 * 
 * Q2: Có sử dụng Unit of Work không?
 * A2: Hiện tại CHƯA sử dụng Unit of Work pattern. 
 * - BookingService inject trực tiếp DbContext để quản lý transaction
 * - Có thể cải thiện: Tạo IUnitOfWork để quản lý transaction tập trung
 * - Lý do hiện tại: Đơn giản hóa cho project nhỏ, transaction chỉ cần ở BookingService
 */

var builder = WebApplication.CreateBuilder(args);
var secretKey = builder.Configuration["JwtSettings:SecretKey"];
var issuer = builder.Configuration["JwtSettings:Issuer"];
var audience = builder.Configuration["JwtSettings:Audience"];

// Add services to the container.
builder.Services.AddControllers();

/*
 * Q3: Có rate limiting không? Làm sao chống spam/abuse?
 * A3: Hiện tại CHƯA có rate limiting. 
 * - Có thể thêm: AspNetCoreRateLimit package
 * - Nên implement: Giới hạn số request/giờ cho mỗi IP/user
 * - Production: Nên dùng API Gateway (Azure API Management, AWS API Gateway)
 */
// ✅ FIX CORS - Thêm cả localhost:5173 (Vite) và localhost:8080
// Q4: CORS configuration có an toàn không?
// A4: Hiện tại chỉ cho phép localhost (development). 
// Production: Nên chỉ định domain cụ thể thay vì AllowAnyHeader/Method
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",  // ← Vite dev server
                "http://localhost:8080",  // ← Production/other port
                "http://localhost:3000"   // ← Create React App (nếu cần)
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

/*
 * Q5: Có API versioning không?
 * A5: Hiện tại CHƯA có versioning (chỉ v1).
 * - Nếu thay đổi API: Client cũ sẽ bị ảnh hưởng
 * - Nên implement: URL versioning (/api/v1/booking) hoặc Header versioning
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
 * Q6: Database connection string có an toàn không?
 * A6: Connection string được lưu trong appsettings.json (không commit vào git)
 * - Development: OK (dùng User Secrets hoặc appsettings.Development.json)
 * - Production: Nên dùng Azure Key Vault, AWS Secrets Manager, hoặc Environment Variables
 */
builder.Services.AddDbContext<FptBookingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

/*
 * Q7: Tại sao dùng AddScoped cho Repository và Service?
 * A7: 
 * - Scoped: Một instance cho mỗi HTTP request (phù hợp với DbContext)
 * - Singleton: Một instance cho toàn bộ app (không phù hợp vì DbContext không thread-safe)
 * - Transient: Một instance mới mỗi lần inject (tốn tài nguyên, không cần thiết)
 * - Repository và Service nên là Scoped để share cùng một DbContext trong một request
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
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICheckinRepository, CheckinRepository>();
builder.Services.AddScoped<ICheckoutRepository, CheckoutRepository>();

builder.Services.AddSignalR();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<JwtSettings>(builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>());
builder.Services.AddScoped<JwtTokenGenerator>();

/*
 * Q8: JWT token có refresh token không?
 * A8: Hiện tại CHƯA có refresh token.
 * - Access token: Expires sau 1 giờ (trong JwtTokenGenerator)
 * - Vấn đề: User phải login lại sau 1 giờ
 * - Nên implement: Refresh token (expires sau 7-30 ngày) để tự động renew access token
 * 
 * Q9: Có xử lý token bị đánh cắp không?
 * A9: CHƯA có token blacklist/revocation.
 * - Nếu token bị leak: Phải đợi hết hạn (1 giờ)
 * - Nên implement: Token blacklist trong Redis/database
 * - Hoặc: Short-lived token (15 phút) + refresh token
 */
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,        // Validate issuer để chống token giả mạo
            ValidateAudience = true,      // Validate audience
            ValidateLifetime = true,      // Validate expiration time
            ValidateIssuerSigningKey = true, // Validate signature
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

/*
 * Q10: Có authorization (phân quyền) không?
 * A10: Có AddAuthorization() nhưng CHƯA implement policy cụ thể.
 * - Nên thêm: [Authorize(Roles = "Admin")] cho các endpoint cần quyền
 * - Ví dụ: Chỉ Admin mới được duyệt booking
 * - Có thể dùng: Policy-based authorization cho logic phức tạp hơn
 */
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();

/*
 * Q11: Middleware pipeline order có đúng không?
 * A11: Đúng thứ tự:
 * 1. CORS trước Authentication (để browser có thể gửi preflight request)
 * 2. Authentication trước Authorization (phải biết user là ai trước khi check quyền)
 * 3. Authorization trước Controllers (check quyền trước khi vào controller)
 */
// ✅ IMPORTANT: UseCors PHẢI đặt TRƯỚC UseAuthentication và UseAuthorization
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//// ✅ Map SignalR Hub với CORS
//app.MapHub<NotificationHub>("/notificationHub").RequireCors("AllowReactApp");

app.Run();