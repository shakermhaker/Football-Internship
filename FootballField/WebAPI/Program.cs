var builder = WebApplication.CreateBuilder(args);

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
app.UseAuthorization();
app.MapControllers();

app.Run();