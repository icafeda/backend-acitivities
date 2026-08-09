using API.Middleware;
using Application.Activities.Queries;
using Application.Core;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;

// 0. Khởi tạo builder
var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()// nếu muốn log ra console
    .WriteTo.File("Logs/app.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true)
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog();//dòng này build serilog vào asp net core

// 2. Xóa tất cả provider cũ và chỉ thêm lại JSON file với 

// builder.Configuration
//     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
//     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
//     .AddEnvironmentVariables();

builder.Configuration.GetConnectionString("DefaultConnection");

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
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<GetActivityList.Handler>());

builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfiles).Assembly);




// 6. Tạo ứng dụng (build app)
var app = builder.Build();

// SERILOG - ghi log toàn bộ lỗi hệ thống
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()
            ?.Error;

        if (exception != null)
        {
            Log.Error(exception, "Unhandled exception occurred");
        }

        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An unexpected error occurred.");
    });
});

// Cấu hình serilog ghi hết các hoạt động
app.Use(async (context, next) =>
{
    Log.Information($"Incoming request: {context.Request.Method} {context.Request.Path}");
    await next();
    Log.Information($"Outgoing response: {context.Response.StatusCode}");
});

app.UseMiddleware<DatabaseLoggingMiddleware>();

// 7. Run Migration & Seed
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

// 8. Các middle-ware & routing - đặt sau builder.Build()
app.UseCors("CorsPolicy");
app.MapControllers();

app.Run();

