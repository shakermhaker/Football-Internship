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
        private readonly ITimeSlotDal _timeSlotDal; // Dal veya Service kullanabilirsin
        private readonly IFieldPriceScheduleService _scheduleService;

        public FieldManager(
            IFieldDal fieldDal,
            ITimeSlotDal timeSlotDal,
            IFieldPriceScheduleService scheduleService)
        {
            _fieldDal = fieldDal;
            _timeSlotDal = timeSlotDal;
            _scheduleService = scheduleService;
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
    
       }
    }
