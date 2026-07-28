using DataAccess.Concrete.EntityFramework;
using FootballField.DataAccess.Concrete.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPI.BackgroundServices
{
    public class ReservationStatusUpdaterService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReservationStatusUpdaterService> _logger;

        public ReservationStatusUpdaterService(IServiceProvider serviceProvider, ILogger<ReservationStatusUpdaterService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Rezervasyon Durum Güncelleyici Servisi başlatıldı...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Arka plan servisleri Singleton olduğu için, Scoped olan DbContext'i bu şekilde çağırıyoruz
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<FootballFieldContext>();

                        // 🚀 PostgreSQL'in Gücünü Kullanıyoruz: 
                        // Tarih ve Saati birleştirip şu anki zamanla kıyaslayan ve eşleşenlerin StatusId'sini 3 (Bitti) yapan süper hızlı SQL
                        var sql = @"
                            UPDATE ""Reservations""
                            SET ""StatusId"" = 3
                            FROM ""FieldPriceSchedules"" fps
                            INNER JOIN ""TimeSlots"" ts ON fps.""TimeSlotId"" = ts.""Id""
                            WHERE ""Reservations"".""FieldPriceScheduleId"" = fps.""Id""
                              AND ""Reservations"".""StatusId"" = 1
                              AND (""Reservations"".""ReservationDate"" + ts.""EndTime"") < CURRENT_TIMESTAMP;";

                        int updatedCount = await context.Database.ExecuteSqlRawAsync(sql, stoppingToken);

                        if (updatedCount > 0)
                        {
                            _logger.LogInformation($"{updatedCount} adet rezervasyonun süresi geçtiği için 'Tamamlandı' (3) statüsüne çekildi.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Rezervasyon statüleri güncellenirken bir hata oluştu.");
                }

                // 🚀 Servis her 15 dakikada bir uyanıp kontrol edecek (Süreyi isteğine göre değiştirebilirsin)
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }
}