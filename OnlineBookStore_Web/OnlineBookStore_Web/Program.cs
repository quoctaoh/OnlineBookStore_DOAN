using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Web.Models;
using OnlineBookStore_Web;
using Net.payOS; // Thêm thư viện này

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký các dịch vụ cơ bản
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// 2. Cấu hình Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Đọc chuỗi kết nối SQL Server và đăng ký Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<OnlineBookstore_DOANContext>(options =>
    options.UseSqlServer(connectionString));

// 4. CẤU HÌNH PAYOS (Thêm phần này để không bị lỗi ở Controller)
Net.payOS.PayOS payOS = new Net.payOS.PayOS(
    builder.Configuration["PayOS:ClientId"] ?? "",
    builder.Configuration["PayOS:ApiKey"] ?? "",
    builder.Configuration["PayOS:ChecksumKey"] ?? ""
);
builder.Services.AddSingleton(payOS);

// 5. Xây dựng ứng dụng
var app = builder.Build();

// 6. Cấu hình HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Quan trọng: Sử dụng Session trước Authorization
app.UseSession();
app.UseAuthorization();

// 7. Cấu hình Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();