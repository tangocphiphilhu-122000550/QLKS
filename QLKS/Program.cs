using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QLKS.Data;
using QLKS.Helpers;
using QLKS.Repository;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext với SQL Server


// Đăng ký repository
builder.Services.AddDbContext<Qlks1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // Thay YourDbContext

builder.Services.AddScoped<INhanVienRepository, NhanVienRepository>();
builder.Services.AddScoped<EmailHelper>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

// Cấu hình JWT
var configuration = builder.Configuration;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
builder.Services.AddAuthorization();

// Thêm Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "QLKS API", Version = "v1" });
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Cấu hình Swagger UI
if (app.Environment.IsDevelopment()) // Chỉ bật Swagger khi chạy ở chế độ Development
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "QLKS API v1");
        c.RoutePrefix = "swagger"; // Truy cập tại /swagger
    });
}

app.MapControllers();

app.Run();
