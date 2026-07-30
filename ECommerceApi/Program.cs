using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using ECommerceApi.Common;
using ECommerceApi.Data;
using ECommerceApi.Repositories.Base;
using ECommerceApi.Repositories.Interfaces;
using ECommerceApi.Services;
using ECommerceApi.UnitOfWork;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ----- CORS -----
const string CorsPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins("http://localhost:3000")   // origin của Next.js (dev)
              .AllowAnyHeader()                        // cho Authorization, X-Session-Id...
              .AllowAnyMethod()                        // GET/POST/PUT/DELETE
              .AllowCredentials());                    // cho cookie (plan dùng httpOnly cookie sau)
});

// ----- Services -----
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Data access: Repository<T> + UnitOfWork
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Validation: tự nạp validator + auto-validate request
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddFluentValidationAutoValidation();

// Global error handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ----- Rate limiting (chống brute-force / flood) -----
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Khi bị chặn: trả đúng envelope ApiResponse + header Retry-After (FE hiện message thân thiện)
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        await context.HttpContext.Response.WriteAsJsonAsync(
            ApiResponse<object>.Fail("Bạn thao tác quá nhanh. Vui lòng thử lại sau ít phút."), token);
    };

    // Toàn cục: chặn flood theo IP — ngưỡng cao, dùng bình thường không chạm tới
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromSeconds(10) }));

    // "auth": siết chặt endpoint nhạy cảm (login/register/quên mật khẩu/OTP) — 8 lần / phút / IP
    options.AddPolicy("auth", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 8, Window = TimeSpan.FromMinutes(1) }));
});
builder.Services.AddProblemDetails();

// Auth: JWT bearer
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero
        };
        // Đọc access token từ cookie httpOnly khi request không kèm Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (string.IsNullOrEmpty(ctx.Token) &&
                    ctx.Request.Cookies.TryGetValue(AuthCookies.Access, out var cookieToken))
                    ctx.Token = cookieToken;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IVariantService, VariantService>();
builder.Services.AddScoped<IProductImageService, ProductImageService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderNotifier, OrderNotifier>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.Configure<SePaySettings>(builder.Configuration.GetSection(SePaySettings.SectionName));
builder.Services.Configure<GoogleSettings>(builder.Configuration.GetSection(GoogleSettings.SectionName));

// Lưu ảnh: có cấu hình Cloudinary → dùng cloud; chưa có → local wwwroot (dev, mất khi redeploy)
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection(CloudinarySettings.SectionName));
var cloudinary = builder.Configuration.GetSection(CloudinarySettings.SectionName).Get<CloudinarySettings>();
if (cloudinary is not null && !string.IsNullOrWhiteSpace(cloudinary.CloudName) && !string.IsNullOrWhiteSpace(cloudinary.ApiSecret))
    builder.Services.AddScoped<IImageStorage, CloudinaryImageStorage>();
else
    builder.Services.AddScoped<IImageStorage, LocalImageStorage>();
builder.Services.AddHostedService<OrderExpiryWorker>();   // tự hủy đơn SePay quá hạn (hoàn kho + coupon)
// Email: có Smtp:Password (user-secrets) → gửi thật; chưa có → log ra console (dev)
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(SmtpSettings.SectionName));
if (!string.IsNullOrWhiteSpace(builder.Configuration["Smtp:Password"]))
    builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
else
    builder.Services.AddScoped<IEmailSender, LogEmailSender>();

var app = builder.Build();

// ----- Pipeline -----
app.UseExceptionHandler();   // phải đứng đầu để bắt mọi lỗi phía dưới

// Security headers (rẻ, áp cho mọi response)
app.Use(async (context, next) =>
{
    var h = context.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";       // chặn MIME-sniffing
    h["X-Frame-Options"] = "DENY";                 // chống clickjacking
    h["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    // Chỉ ép HTTPS ở production. Dev để HTTP thường (server-fetch của Next
    // không bị redirect sang HTTPS self-signed → tránh lỗi "self-signed certificate").
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();   // phục vụ ảnh upload ở wwwroot/uploads
app.UseCors(CorsPolicy);
app.UseRateLimiter();   // sau CORS, trước auth
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
