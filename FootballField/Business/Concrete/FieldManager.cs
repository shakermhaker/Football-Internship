using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using Business.Abstract;

namespace Business.Concrete
{
    public class FieldManager : IFieldService
    {
        private readonly IFieldDal _fieldDal;
        private readonly ITimeSlotDal _timeSlotDal;
        private readonly IFieldPriceSheduleDal _fieldPriceScheduleDal;
        private readonly IFieldPriceScheduleService _scheduleService;

        public FieldManager(
            IFieldDal fieldDal,
            ITimeSlotDal timeSlotDal,
            IFieldPriceScheduleService scheduleService,
            IFieldPriceSheduleDal fieldPriceScheduleDal)
        {
            _fieldDal = fieldDal;
            _timeSlotDal = timeSlotDal;
            _scheduleService = scheduleService;
            _fieldPriceScheduleDal = fieldPriceScheduleDal;
        }

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
                    TimeSpan parsedStartTime = TimeSpan.Parse(period.StartTime);
                    TimeSpan parsedEndTime = TimeSpan.Parse(period.EndTime);

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
            var schedules = _fieldPriceScheduleDal.GetAll(s => s.FootballFieldId == fieldId);
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
                var singleMatchDuration = (int)(daySlots.First().TimeSlot.EndTime - daySlots.First().TimeSlot.StartTime).TotalMinutes;

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
                        singleMatchDuration = (int)(slot.TimeSlot.EndTime - slot.TimeSlot.StartTime).TotalMinutes;
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
        private List<TimeRange> MergeConsecutiveTimeSlots(IEnumerable<TimeRangeInput> timeSlots)
        {
            var sortedSlots = timeSlots.OrderBy(x => x.StartTime).ToList();
            var merged = new List<TimeRange>();

            if (!sortedSlots.Any()) return merged;

            var currentStart = sortedSlots.First().StartTime;
            var currentEnd = sortedSlots.First().EndTime;

            for (int i = 1; i < sortedSlots.Count; i++)
            {
                // Eğer bir önceki saatin bitişi, sonraki saatin başlangıcına eşitse (Ardışıksa)
                if (sortedSlots[i].StartTime == currentEnd)
                {
                    currentEnd = sortedSlots[i].EndTime; // Süreyi uzat
                }
                else
                {
                    // Ardışıklık bozuldu, öncekini kaydet ve yeni başlat
                    merged.Add(new TimeRange { StartTime = currentStart, EndTime = currentEnd });
                    currentStart = sortedSlots[i].StartTime;
                    currentEnd = sortedSlots[i].EndTime;
                }
            }
            // Son kalanı ekle
            merged.Add(new TimeRange { StartTime = currentStart, EndTime = currentEnd });

            return merged;
        }

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
        public IResult UpdateWithSchedules(FootballFieldAddDTO fieldDto, int fieldId)
        {
            // 1. Sahayı bul
            var field = _fieldDal.Get(f => f.Id == fieldId);
            if (field == null)
            {
                return new ErrorResult("Güncellenecek saha bulunamadı.");
            }

            // Sahanın adını güncelle
            field.FieldName = fieldDto.Name; // Senin DTO'daki property adın neyse (Name / FieldName)
            _fieldDal.Update(field);

            // 2. TEMİZLİK OPERASYONU: Bu sahaya ait eski tüm fiyat/saat programlarını bul ve sil
            var oldSchedules = _fieldPriceScheduleDal.GetAll(s => s.FootballFieldId == fieldId);
            foreach (var oldSchedule in oldSchedules)
            {
                _fieldPriceScheduleDal.Delete(oldSchedule);
            }

            // 3. YENİLERİ EKLEME: Tıpkı eklemedeki mantıkla yeni gelen grupları ve periyotları ekle
            foreach (var group in fieldDto.ScheduleGroups)
            {
                foreach (var period in group.Periods)
                {
                    // Saat aralığına uygun TimeSlot'u bul veya oluştur
                    var timeSlot = _timeSlotDal.Get(t => t.StartTime == TimeSpan.Parse(period.StartTime) && t.EndTime == TimeSpan.Parse(period.EndTime));

                    if (timeSlot == null)
                    {
                        timeSlot = new TimeSlot
                        {
                            StartTime = TimeSpan.Parse(period.StartTime),
                            EndTime = TimeSpan.Parse(period.EndTime)
                        };
                        _timeSlotDal.Add(timeSlot);
                    }

                    // Her bir seçilen gün için yeni kayıt at
                    foreach (var dayId in group.SelectedDayIds)
                    {
                        var schedule = new FieldPriceSchedule
                        {
                            FootballFieldId = field.Id,
                            TimeSlotId = timeSlot.Id,
                            DayId = dayId,
                            Price = period.Price
                        };
                        _fieldPriceScheduleDal.Add(schedule);
                    }
                }
            }

            return new SuccessResult("Halı saha ve rezervasyon programları başarıyla güncellendi.");
        }

    }
    }
