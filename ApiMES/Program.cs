using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ApiMES.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ApiMES.Application.Services.ACS;
using ApiMES.Application.Services.Files;
using ApiMES.Application.Services.HealthCheck;
using ApiMES.Application.Services.Users;
using ApiMES.Application.Configurations;
using ApiMES.Application.Services.Auth;
using ApiMES.Application.Services.Menu;
using ApiMES.Infrastructure.DAOs.HRMS;
using ApiMES.Infrastructure.DAOs.IM;
using ApiMES.Infrastructure.DAOs.User;
using ApiMES.Infrastructure.DAOs.VG;
using ApiMES.Infrastructure.DAOs.Common;
using ApiMES.Infrastructure.DAOs.Mongo;
using ApiMES.Domain.Entities.Users;

var builder = WebApplication.CreateBuilder(args);
Console.OutputEncoding = System.Text.Encoding.UTF8;

// 1. Load cấu hình từ appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// 2. Register cấu hình vào DI
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// 3. Logging (chuẩn ASP.NET Core)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// 3. Đăng ký DbContext cho mỗi cơ sở dữ liệu
builder.Services.AddDbContext<GateDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HRMS")));
builder.Services.AddDbContext<IMDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IM")));

// 3.1 Cấu hình Identity
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//    .AddEntityFrameworkStores<IMDbContext>()
//    .AddDefaultTokenProviders();

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IMDbContext>()
    .AddDefaultTokenProviders();

// 4. MongoDB: MongoClient và MongoService
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
    return new MongoClient(settings.MongoConnectionString);
});

builder.Services.AddMemoryCache();

// 4.1 AddScoped
// 4.1.1 Services
builder.Services.AddScoped<MongoDao>();

builder.Services.AddScoped<UserApplicationService>();

builder.Services.AddScoped<UserApplicationService>();

builder.Services.AddScoped<HealthCheckService>();

builder.Services.AddScoped<FileUploadApplicationService>();

builder.Services.AddScoped<VgApplicationService>();

builder.Services.AddScoped<MenuApplicationService, MenuApplicationService>();

builder.Services.AddScoped<UserPermissionDao>();

builder.Services.AddScoped<AuthApplicationService>();

builder.Services.AddScoped<RefreshTokenDao>();

builder.Services.AddScoped<UserDao>();

// 4.1.2 Repository

builder.Services.AddScoped<HrmsDao>();

builder.Services.AddScoped<ImDao>();

builder.Services.AddScoped<VgDao>();

builder.Services.AddScoped<MenuDao>();

// 4.1.3 Difference

builder.Services.AddScoped<DbHelper>();

builder.Services.AddScoped<DbConnectionFactory>();

// 5. Controllers và JSON config
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        //Bỏ qua các thuộc tính có giá trị null khi serialize
        opt.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        opt.JsonSerializerOptions.PropertyNamingPolicy = null;
        // Đảm bảo sử dụng mã hóa UTF-8 và hỗ trợ tất cả ký tự Unicode (bao gồm tiếng Việt)
        opt.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All);
    });

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["AccessToken"];
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Authentication failed: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse(); // ⚠ rất quan trọng: ngăn ASP.NET tự xử lý mặc định (redirect/html)

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\": \"Unauthorized: Token is missing or invalid\"}");
            }
        };
    });

// 6. Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "1.0.0",  // Use a semantic version here
        Title = "MES API",
        Description = "API for MES System (.NET 8)",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Your Name",
            Email = "you@example.com"
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Nếu bạn dùng cookie/token
    });
});

// 7. Build application
var app = builder.Build();

// 8. Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAngularClient");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
