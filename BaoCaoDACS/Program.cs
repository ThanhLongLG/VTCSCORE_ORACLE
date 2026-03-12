using BAO_CAO.Models;
using BaoCaoDACS.Models;
using BaoCaoDACS.Reponsitory;
using BaoCaoDACS.Reponsitory.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Oracle.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// === ML.NET MODEL LOADER ===
builder.Services.AddSingleton<ITransformer>(services =>
{
    var mlContext = new MLContext();
    // ⭐ ĐỔI ContentRootPath -> WebRootPath + "Models"
    var modelPath = Path.Combine(builder.Environment.WebRootPath, "Models", "match_predictor.zip");

    if (!File.Exists(modelPath))
        throw new FileNotFoundException($"Không tìm thấy model ML tại: {modelPath}");

    using var stream = File.OpenRead(modelPath);
    var model = mlContext.Model.Load(stream, out _);
    return model;
});

builder.Services.AddSingleton<PredictionEngine<MatchTrainingSample, MatchPredictionOutput>>(sp =>
{
    var mlContext = new MLContext();
    var model = sp.GetRequiredService<ITransformer>();
    return mlContext.Model.CreatePredictionEngine<MatchTrainingSample, MatchPredictionOutput>(model);
});


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});
builder.Services.AddScoped<INguoidungreponsitory, EFNguoiDungreponsitory>();
builder.Services.AddScoped<IGiaiDaureponsitory, MOMOService>();
builder.Services.AddScoped<ILoaiHinhreponsitory, EFLoaiHinheponsitory>();
builder.Services.AddScoped<IKetquareponsitory, EFKetquareponsitory>();
builder.Services.AddScoped<ITranDaureponsitory, EFTrandaureponsitory>();
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IRankingService, EFRankingService>();
builder.Services.AddScoped<IMatchPredictionService, EFMatchPredictionService>();
builder.Services.AddScoped<IMomoService, MomoService>();



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("QLTAPVO")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IVnPayService, VnPayService>();
var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Create Admin role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Create admin user if it doesn't exist
        var adminUser = await userManager.FindByEmailAsync("admin@example.com");
        if (adminUser == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Add cache control headers middleware
app.Use(async (context, next) =>
{
    // Prevent caching for HTML pages
    if (context.Request.Path.HasValue && !context.Request.Path.Value.StartsWith("/css/") && 
        !context.Request.Path.Value.StartsWith("/js/") && !context.Request.Path.Value.StartsWith("/lib/"))
    {
        context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
        context.Response.Headers.Add("Pragma", "no-cache");
        context.Response.Headers.Add("Expires", "0");
    }
    await next();
});

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();


app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=TrangQL}/{action=Index}/{id?}");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
