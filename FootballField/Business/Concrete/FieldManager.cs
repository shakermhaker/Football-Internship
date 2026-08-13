using Business.Abstract;
using Core.Aspects.Autofac.Logging;
using Core.Aspects.Autofac.Performance;
using Core.Aspects.Autofac.Transaction;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Concrete
{
    public class FieldManager : IFieldService
    {
        private readonly IFieldDal _fieldDal;
        private readonly ITimeSlotDal _timeSlotDal;
        private readonly IFieldPriceSheduleDal _fieldPriceScheduleDal;
        private readonly IFieldPriceScheduleService _scheduleService;
        private readonly IReservationDal _reservationDal;

        public FieldManager(
            IFieldDal fieldDal,
            ITimeSlotDal timeSlotDal,
            IFieldPriceScheduleService scheduleService,
            IFieldPriceSheduleDal fieldPriceScheduleDal,
            IReservationDal reservationDal)
        {
            _fieldDal = fieldDal;
            _timeSlotDal = timeSlotDal;
            _scheduleService = scheduleService;
            _reservationDal = reservationDal;
            _fieldPriceScheduleDal = fieldPriceScheduleDal;
        }


        [TransactionScopeAspect]
        [LogAspect]
        [ExceptionLogAspect]
        [PerformanceAspect(2)]
        public IResult AddWithDetails(FootballFieldAddDTO fieldDto)
        {
            // 1. ADIM: ANA SAHAYI EKLE
            var field = new Entities.Concrete.FootballField
            {
                BusinessId = fieldDto.BusinessId,
                FieldName = fieldDto.Name,
                // Type, Capacity vs...
            };
            _fieldDal.Add(field);

            // 2. ADIM: GRUPLARI (YEŞİL ALANLARI) DÖN
            foreach (var group in fieldDto.ScheduleGroups)
            {
                // 3. ADIM: O GRUBUN PERİYOTLARINI (TURUNCU ALANLARI) DÖN
                foreach (var period in group.Periods)
                {
                    // Eğer önyüz "24:00" gönderirse, bunu C#'ın ve Veritabanının (SQL) anlayacağı "00:00" formatına çeviriyoruz.
                    TimeSpan parsedStartTime = period.StartTime == "24:00" ? TimeSpan.Zero : TimeSpan.Parse(period.StartTime);
                    TimeSpan parsedEndTime = period.EndTime == "24:00" ? TimeSpan.Zero : TimeSpan.Parse(period.EndTime);

                    // -- MÜKERRER SAAT KONTROLÜ --
                    var existingTimeSlot = _timeSlotDal.GetByTimes(parsedStartTime, parsedEndTime);
                    int timeSlotId;

                    if (existingTimeSlot != null)
                    {
                        timeSlotId = existingTimeSlot.Id;
                    }
                    else
                    {
                        var newTimeSlot = new TimeSlot
                        {
                            StartTime = parsedStartTime,
                            EndTime = parsedEndTime
                        };
                        _timeSlotDal.Add(newTimeSlot);
                        timeSlotId = newTimeSlot.Id;
                    }

                    // 4. ADIM: GÜNLER İLE SAATLERİ "FieldPriceSchedules" TABLOSUNDA BİRLEŞTİR
                    foreach (var dayId in group.SelectedDayIds)
                    {
                        var priceSchedule = new FieldPriceSchedule
                        {
                            FootballFieldId = field.Id,
                            TimeSlotId = timeSlotId,
                            DayId = dayId,
                            Price = period.Price
                        };

                        _scheduleService.Add(priceSchedule);
                    }
                } // İÇ DÖNGÜ BİTİŞİ (Periyotlar)
            } // DIŞ DÖNGÜ BİTİŞİ (Gruplar)

            // İŞTE DOĞRU YER BURASI! Her şey bittikten sonra, dışarıda return ediyoruz.
            return new SuccessResult("Halı saha ve tüm rezervasyon fiyatlandırmaları başarıyla oluşturuldu.");
        }

        [TransactionScopeAspect]
        [LogAspect]
        [ExceptionLogAspect]
        [PerformanceAspect(2)]
        public IResult DeleteField(int fieldId)
        {
            // 1. Önce sahayı bul
            var field = _fieldDal.Get(f => f.Id == fieldId);

            if (field == null)
            {
                return new ErrorResult("Silinmek istenen saha bulunamadı.");
            }

            // 2. Sahayı sil (Cascade açıksa bağlı fiyat programları da otomatik silinir)
            _fieldDal.Delete(field);

            return new SuccessResult("Halı saha ve ona bağlı tüm programlar başarıyla silindi.");
        }

        // FieldManager.cs dosyasının içine ekle:
        public IDataResult<FootballFieldAddDTO> GetFieldForEdit(int fieldId)
        {
            // 1. Ana sahayı çek
            var field = _fieldDal.Get(f => f.Id == fieldId);
            if (field == null) return new ErrorDataResult<FootballFieldAddDTO>("Saha bulunamadı.");

            // 2. Sahaya ait tüm periyotları TimeSlot detayıyla çek
            var schedules = _fieldPriceScheduleDal.GetAll(s => s.FootballFieldId == fieldId && s.IsDeleted == false);
            var detailedSchedules = schedules.Select(s => new
            {
                s.DayId,
                s.Price,
                TimeSlot = _timeSlotDal.Get(t => t.Id == s.TimeSlotId)
            }).Where(x => x.TimeSlot != null).ToList();

            var days = detailedSchedules.Select(x => x.DayId).Distinct().ToList();
            var daySchedules = new Dictionary<int, List<PeriodDto>>();

            // 3. ADIM: HER GÜN İÇİN KENDİ İÇİNDEKİ SAATLERİ (MOR ARTILARI) HESAPLA
            foreach (var day in days)
            {
                var daySlots = detailedSchedules.Where(x => x.DayId == day)
                                                .OrderBy(x => x.TimeSlot.StartTime)
                                                .ToList();

                var mergedPeriods = new List<PeriodDto>();
                if (!daySlots.Any()) continue;

                // 1. Döngü başlamadan önce ilk saatin kendi süresini (örn: 60 dk) hesaplayıp kenara alıyoruz:
                var currentStart = daySlots.First().TimeSlot.StartTime;
                var currentEnd = daySlots.First().TimeSlot.EndTime;
                var currentPrice = daySlots.First().Price;
                // DİNAMİK SÜRE HESAPLAMA (Sadece ilk tekil periyodun farkı):
                var firstSlotStart = daySlots.First().TimeSlot.StartTime;
                var firstSlotEnd = daySlots.First().TimeSlot.EndTime;

                if (firstSlotEnd <= firstSlotStart)
                {
                    firstSlotEnd = firstSlotEnd.Add(TimeSpan.FromDays(1)); // 00:00 ise ertesi güne sarkıt (24 saat ekle)
                }
                var singleMatchDuration = (int)(firstSlotEnd - firstSlotStart).TotalMinutes;
                for (int i = 1; i < daySlots.Count; i++)
                {
                    var slot = daySlots[i];

                    if (slot.TimeSlot.StartTime == currentEnd && slot.Price == currentPrice)
                    {
                        currentEnd = slot.TimeSlot.EndTime; // Sadece saati uzatıyoruz, süre (60) bozulmuyor
                    }
                    else
                    {
                        // Ardışıklık bozulduğunda:
                        mergedPeriods.Add(new PeriodDto
                        {
                            StartTime = currentStart.ToString(@"hh\:mm"),
                            EndTime = currentEnd.ToString(@"hh\:mm"),
                            Duration = singleMatchDuration, // Dinamik olarak hesaplanan tekil süreyi basıyoruz!
                            Price = currentPrice
                        });

                        // Yeni periyodu başlatıyoruz:
                        currentStart = slot.TimeSlot.StartTime;
                        currentEnd = slot.TimeSlot.EndTime;
                        currentPrice = slot.Price;
                        // Yeni grubun da tekil süresini dinamik hesaplıyoruz:
                        // Yeni grubun da tekil süresini dinamik hesaplıyoruz (Gece yarısı geçişi destekli):
                        var currentSlotStart = slot.TimeSlot.StartTime;
                        var currentSlotEnd = slot.TimeSlot.EndTime;

                        if (currentSlotEnd <= currentSlotStart)
                        {
                            currentSlotEnd = currentSlotEnd.Add(TimeSpan.FromDays(1));
                        }
                        singleMatchDuration = (int)(currentSlotEnd - currentSlotStart).TotalMinutes;
                    }
                }

                // Son kalanı ekle:
                mergedPeriods.Add(new PeriodDto
                {
                    StartTime = currentStart.ToString(@"hh\:mm"),
                    EndTime = currentEnd.ToString(@"hh\:mm"),
                    Duration = singleMatchDuration,
                    Price = currentPrice
                });

                // Bu günün tüm programı hazır!
                daySchedules[day] = mergedPeriods;
            }

            // 4. ADIM: AYNI PROGRAMA SAHİP GÜNLERİ TEK BİR YEŞİL BLOKTA BİRLEŞTİR
            // Eğer Pazartesi'den Cuma'ya kadarki günlerin "mergedPeriods" listesi BİREBİR aynıysa onları tek blok yap!
            var groupedDays = daySchedules.GroupBy(kvp =>
                string.Join("|", kvp.Value.Select(p => $"{p.StartTime}-{p.EndTime}-{p.Price}"))
            );

            var scheduleGroupsResult = new List<ScheduleGroupDto>();
            foreach (var group in groupedDays)
            {
                scheduleGroupsResult.Add(new ScheduleGroupDto
                {
                    SelectedDayIds = group.Select(x => x.Key).ToList(), // Ortak günler (Pzt, Salı...)
                    Periods = group.First().Value // O günlerin ortak periyotları (İçteki mor artılar)
                });
            }

            var fieldDto = new FootballFieldAddDTO
            {
                Name = field.FieldName,
                BusinessId = field.BusinessId,
                ScheduleGroups = scheduleGroupsResult
            };

            return new SuccessDataResult<FootballFieldAddDTO>(fieldDto, "Veriler form mimarisine uygun gruplandı.");
        }
        // ARDIŞIK SAATLERİ BİRLEŞTİREN YARDIMCI METOT (Gaps and Islands)

        // Yardımcı Sınıflar (Manager içine veya uygun bir yere eklenebilir)
        public class TimeRangeInput
        {
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
        }

        public class TimeRange
        {
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
        }
        [TransactionScopeAspect]
        [LogAspect]
        [ExceptionLogAspect]
        [PerformanceAspect(2)]
        public IResult UpdateWithSchedules(FootballFieldAddDTO fieldDto, int fieldId)
        {
            // 1. Sahayı bul ve ana bilgileri güncelle
            var field = _fieldDal.Get(f => f.Id == fieldId);
            if (field == null)
            {
                return new ErrorResult("Güncellenecek saha bulunamadı.");
            }

            field.FieldName = fieldDto.Name;
            _fieldDal.Update(field);

            // 2. ÖNYÜZDEN GELEN VERİYİ DÜZ BİR "HEDEF" LİSTESİNE ÇEVİR (Flattening)
            var targetSchedules = new List<FieldPriceSchedule>();

            foreach (var group in fieldDto.ScheduleGroups)
            {
                foreach (var period in group.Periods)
                {
                    TimeSpan parsedStart = period.StartTime == "24:00" ? TimeSpan.Zero : TimeSpan.Parse(period.StartTime);
                    TimeSpan parsedEnd = period.EndTime == "24:00" ? TimeSpan.Zero : TimeSpan.Parse(period.EndTime);

                    // Saat aralığına uygun TimeSlot'u bul veya yepyeni bir tane oluştur
                    var timeSlot = _timeSlotDal.Get(t => t.StartTime == parsedStart && t.EndTime == parsedEnd);
                    if (timeSlot == null)
                    {
                        timeSlot = new TimeSlot { StartTime = parsedStart, EndTime = parsedEnd };
                        _timeSlotDal.Add(timeSlot);
                    }

                    foreach (var dayId in group.SelectedDayIds)
                    {
                        targetSchedules.Add(new FieldPriceSchedule
                        {
                            FootballFieldId = field.Id,
                            DayId = dayId,
                            TimeSlotId = timeSlot.Id,
                            Price = period.Price
                        });
                    }
                }
            }

            // 3. VERİTABANINDAKİ MEVCUT KAYITLARI ÇEK
            var existingSchedules = _fieldPriceScheduleDal.GetAll(s => s.FootballFieldId == fieldId);

            var schedulesToUpdate = new List<FieldPriceSchedule>();
            var schedulesToAdd = new List<FieldPriceSchedule>();

            // 🚀 YENİ: Sadece silinen saatlerin ID'lerini toplayacağız
            var deletedScheduleIds = new List<int>();

            // 4. MEVCUT KAYITLARI KONTROL ET
            foreach (var existing in existingSchedules)
            {
                var incomingMatch = targetSchedules.FirstOrDefault(x => x.DayId == existing.DayId && x.TimeSlotId == existing.TimeSlotId);

                if (incomingMatch == null)
                {
                    // 🚨 SAAT SİLİNMİŞ (İşte mağduriyet burada başlıyor!)
                    if (existing.IsDeleted == false)
                    {
                        existing.IsDeleted = true;
                        schedulesToUpdate.Add(existing);

                        // İptal edilen bu periyodun ID'sini listeye at
                        deletedScheduleIds.Add(existing.Id);
                    }
                }
                else
                {
                    // 💰 FİYAT DEĞİŞMİŞ (Veya Diriltilmiş)
                    bool needsUpdate = false;

                    if (existing.Price != incomingMatch.Price || existing.IsDeleted == true)
                    {
                        existing.Price = incomingMatch.Price;
                        existing.IsDeleted = false;
                        needsUpdate = true;

                        // DİKKAT: Fiyat değişimi eski rezervasyonları etkilemez! 
                        // O yüzden deletedScheduleIds listesine EKLEMİYORUZ.
                    }

                    if (needsUpdate) schedulesToUpdate.Add(existing);
                }
            }

            // 5. YEPYENİ EKLENECEK KAYITLARI BUL
            foreach (var target in targetSchedules)
            {
                bool existsInDb = existingSchedules.Any(x => x.DayId == target.DayId && x.TimeSlotId == target.TimeSlotId);

                if (!existsInDb)
                {
                    schedulesToAdd.Add(new FieldPriceSchedule
                    {
                        FootballFieldId = target.FootballFieldId,
                        DayId = target.DayId,
                        TimeSlotId = target.TimeSlotId,
                        Price = target.Price,
                        IsDeleted = false
                    });
                    // Yeni saat eklenmesi de kimseyi mağdur etmez, listeye eklemiyoruz.
                }
            }

            // 6. DB YANSITMA (UpdateBulk ve AddBulk işlemleri)
            foreach (var item in schedulesToUpdate) { _fieldPriceScheduleDal.Update(item); }
            foreach (var item in schedulesToAdd) { _fieldPriceScheduleDal.Add(item); }

            // 7. 🚀 SADECE SİLİNEN (PASİFE ÇEKİLEN) SAATLER VARSA TELAFİ ET
            if (deletedScheduleIds.Any())
            {
                // Artık Saha ID'sini değil, doğrudan bozulan periyotların listesini yolluyoruz!
                _reservationDal.CompensateUsersForScheduleChange(deletedScheduleIds);
            }

            return new SuccessResult("Halı saha ve rezervasyon programları akıllı bir şekilde güncellendi.");
        }

        }
    }
