using BanHang.Models;
using BanHang.Services;
using BanHang.Services.Interfaces;
using CloudinaryDotNet;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DATABASE
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// IDENTITY
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// GOOGLE LOGIN
builder.Services
    .AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.CallbackPath = "/signin-google";
    });

// FOR RENDER HTTPS
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// HTTP CLIENT + LOGGING
builder.Services.AddHttpClient();
builder.Services.AddLogging();

// MOMO
builder.Services.Configure<MoMoOption>(builder.Configuration.GetSection("MoMo"));
builder.Services.AddScoped<MoMoService>();

// VNPAY
builder.Services.Configure<VNPayOptions>(builder.Configuration.GetSection("VNPay"));
builder.Services.AddScoped<VNPayService>();

// EMAIL
builder.Services.AddTransient<IEmailSender, EmailSender>();

// MVC + RAZOR
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// REPOSITORIES
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<BanHang.Repositories.IHomeRepository, BanHang.Repositories.HomeRepository>();
builder.Services.AddScoped<BanHang.Repositories.IDonHangRepository, BanHang.Repositories.DonHangRepository>();
builder.Services.AddScoped<BanHang.Repositories.IProductDetailRepository, BanHang.Repositories.ProductDetailRepository>();
builder.Services.AddScoped<BanHang.Repositories.IDanhMucRepository, BanHang.Repositories.DanhMucRepository>();
builder.Services.AddScoped<BanHang.Repositories.IKhuVucRepository, BanHang.Repositories.KhuVucRepository>();
builder.Services.AddScoped<BanHang.Repositories.IAdminDonHangRepository, BanHang.Repositories.AdminDonHangRepository>();
builder.Services.AddScoped<BanHang.Repositories.IThongKeRepository, BanHang.Repositories.ThongKeRepository>();
builder.Services.AddScoped<BanHang.Repositories.IOrderRepository, BanHang.Repositories.OrderRepository>();

// SERVICES
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAdminProductService, AdminProductService>();
builder.Services.AddScoped<BanHang.Services.IHomeService, BanHang.Services.HomeService>();
builder.Services.AddScoped<BanHang.Services.IDonHangService, BanHang.Services.DonHangService>();
builder.Services.AddScoped<BanHang.Services.IProductDetailService, BanHang.Services.ProductDetailService>();

// COOKIE
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// SESSION
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// CLOUDINARY
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddSingleton(serviceProvider =>
{
    var config = builder.Configuration
        .GetSection("CloudinarySettings")
        .Get<CloudinarySettings>();

    var account = new Account(
        config!.CloudName,
        config.ApiKey,
        config.ApiSecret
    );

    return new Cloudinary(account);
});

var app = builder.Build();

// MIDDLEWARE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

// AUTO MIGRATION + ADMIN
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        string[] roles = { "Admin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = "admin@gmail.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "123456");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("🔥 MIGRATION ERROR: " + ex.Message);
    }
}

// ROUTE ADMIN
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// ROUTE DEFAULT
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();