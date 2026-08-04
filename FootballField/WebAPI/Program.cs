using Autofac;
using Autofac.Extensions.DependencyInjection;
using Business.DependencyResolvers.Autofac;
using Core.DependencyResolvers;
using Core.Extensions;
using Core.Utilities.IoC;
using Core.Utilities.Security.Encryption;
using Core.Utilities.Security.JWT;
using Entities.DTOs;
using FootballField.DataAccess.Concrete;
using FootballField.DataAccess.Concrete.EntityFramework;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPI.BackgroundServices;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;    
using System.Security.Claims;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<FootballFieldContext>(options =>
{
    // 🎯 appsettings.json dosyasından "PostgreSQL" isimli bağlantı adresini okuyoruz:
    var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");

    // Npgsql motoruna bu bağlantı adresini kullanmasını söylüyoruz:
    options.UseNpgsql(connectionString);
});
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<AppUrlSettings>(builder.Configuration.GetSection("AppUrlSettings"));
builder.Services.AddCors(options =>
{               
    options.AddPolicy("AllowAngularApp",    
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Sadece bizim Angular projesine izin ver
                  .AllowAnyHeader()                   // Gelen tüm HTTP başlıklarına (Content-Type, Authorization vb.) izin ver
                  .AllowAnyMethod()                   // GET, POST, PUT, DELETE hepsine izin ver
                  .AllowCredentials();                // İleride JWT Cookie kullanırsak sorun çıkmasın diye izin ver
        });
});
// 💡 Eski "Configuration" yerine artık "builder.Configuration" kullanıyoruz
var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>();

// JWT Kimlik Doğrulama Ayarlarını Enjekte Ediyoruz
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = tokenOptions.Issuer,
            ValidAudience = tokenOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = SecurityKeyHelper.CreateSecurityKey(tokenOptions.SecurityKey)
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Tarayıcıdan "auth_token" isimli çerez gelmiş mi diye bakıyoruz:
                var accessToken = context.Request.Cookies["auth_token"];

                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Core katmanından gelen Cross-Cutting Concerns (Caching, Performance vb.) modülleri
builder.Services.AddDependencyResolvers(new ICoreModule[] {
    new CoreModule()
});

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new AutofacBusinessModule());
});


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        formatter: new CompactJsonFormatter(),
        path: "Logs/log-.json",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();



// 1. .NET 10'un Kendi Servis Tanımlamaları (Başka hiçbir harici paket yok)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSwaggerGen(); // Klasik Swagger UI üreteci
builder.Services.AddHostedService<ReservationStatusUpdaterService>();

builder.Services.AddRateLimiter(options =>
{
    // Limit aşıldığında kullanıcıya dönülecek HTTP Status Kodu (429 Too Many Requests)
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // KURAL A: GENEL KORUMA (IP Bazlı)
    // Özelleştirilmemiş tüm endpoint'lere 1 dakikada maksimum 100 istek atılabilir (Veri kazıma botlarını engeller)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    // KURAL B: AUTH (LOGIN/REGISTER) KORUMASI (IP Bazlı - Çok Sıkı)
    options.AddPolicy("AuthLimit", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5, // 1 dakikada sadece 5 deneme (Brute-force şifre kırmayı engeller)
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    // KURAL C: REZERVASYON İŞLEMLERİ (User ID Bazlı - Spam Koruması)
    options.AddPolicy("ReservationLimit", httpContext =>
    {

        // Token'dan Kullanıcı ID'sini alıyoruz, giriş yapmamışsa IP'sini alıyoruz
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? httpContext.Connection.RemoteIpAddress?.ToString()
                     ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: userId,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 3, // 1 kullanıcı, 1 dakikada maks 3 rezervasyon/iptal yapabilir (Spam engeller)
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});


var app = builder.Build();

// 2. HTTP İstek Boru Hattı (Middleware Pipeline)
if (app.Environment.IsDevelopment())
{
    // Proje ayağa kalktığında hata vermeden direkt Swagger UI arayüzünü açar
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();

app.UseRouting();
app.UseHttpsRedirection();
app.UseRouting();   
app.UseRateLimiter();


app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();