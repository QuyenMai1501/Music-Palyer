using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MusicPlayer.Data;
using MusicPlayer.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(option => option.UseMySql(builder.Configuration.GetConnectionString("MySQLConnection"),
new MySqlServerVersion(new Version(8, 0, 29))));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<MusicService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddSingleton<MediaStorage>();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Thư mục gốc lưu media (nhạc, ảnh, lời) — nằm ngoài wwwroot để không mất khi deploy lại.
string mediaRoot = MediaStorage.ResolveRootPath(app.Configuration, app.Environment);
MediaStorage.EnsureDirectories(mediaRoot);

app.UseRouting();


app.UseSession();

app.UseAuthorization();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(mediaRoot, "music")),
    RequestPath = "/music",
    ServeUnknownFileTypes = true, // Cho phép phát file không có MIME rõ ràng
    DefaultContentType = "audio/mpeg" // Đặt kiểu file mặc định là MP3
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(mediaRoot, "lyrics")),
    RequestPath = "/lyrics"
});

// Ảnh upload nằm trong media root; ảnh mặc định trong wwwroot/images vẫn được ưu tiên hơn
// (middleware UseStaticFiles mặc định ở trên chạy trước và phục vụ wwwroot/images nếu có file).
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(mediaRoot, "images")),
    RequestPath = "/images"
});

app.MapControllers();
app.MapRazorPages();
app.Run();
