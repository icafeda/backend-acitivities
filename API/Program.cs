using Microsoft.EntityFrameworkCore;
using Persistence;

// 1. Tắt hoàn toàn việc load/watch file thay đổi từ level cao nhất
var options = new WebApplicationOptions
{
    Args = args
};

var builder = WebApplication.CreateBuilder(options);

// 2. Xóa tất cả provider cũ và chỉ thêm lại JSON file với 

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();


// 3. Thêm các Controllers
builder.Services.AddControllers();

// 4. Cấu hình CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

// 5. Lấy Connection String và chuyển đổi định dạng Render nếu cần
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                          ?? Environment.GetEnvironmentVariable("DATABASE_URL");

string connectionString = ParseConnectionString(rawConnectionString);

builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseNpgsql(connectionString);
});


// 6. Tạo ứng dụng (build app)
var app = builder.Build();

// 7. Các middle-ware & routing - đặt sau builder.Build()
app.UseCors("CorsPolicy");
app.MapControllers();

// 8. Run Migration & Seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await DbInitializer.SeedData(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occured during migration.");
    }
}

app.Run();

// 9. Local function
string ParseConnectionString(string? connectionUri)
{
    if (string.IsNullOrEmpty(connectionUri))
        return string.Empty;

    if (connectionUri.StartsWith("postgres://") || connectionUri.StartsWith("postgresql://"))
    {
        var uri = new Uri(connectionUri);
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
    }

    return connectionUri;
}