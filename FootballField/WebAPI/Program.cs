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

var builder = WebApplication.CreateBuilder(args);
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



// 1. .NET 10'un Kendi Servis Tanımlamaları (Başka hiçbir harici paket yok)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Klasik Swagger UI üreteci

var app = builder.Build();

// 2. HTTP İstek Boru Hattı (Middleware Pipeline)
if (app.Environment.IsDevelopment())
{
    // Proje ayağa kalktığında hata vermeden direkt Swagger UI arayüzünü açar
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();


app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();