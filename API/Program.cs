using Application.Activities.Queries;
using Application.Core;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Persistence;


var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

// 6. Tạo ứng dụng (build app)
var app = builder.Build();



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

