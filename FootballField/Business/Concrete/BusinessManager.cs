using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Core.Aspects.Autofac.Logging;
using Core.Aspects.Autofac.Performance;
using Core.Aspects.Autofac.Transaction;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete;
using Entities.Concrete;
using Entities.DTOs;

namespace Business.Concrete
{
    public class BusinessManager : IBusinessService
    {
        private readonly IBusinessDal _businessDal;
        private readonly IReservationDal _reservationDal;

        public BusinessManager(IBusinessDal businessDal, IReservationDal reservationDal)
        {
            _businessDal = businessDal;
            _reservationDal = reservationDal;
        }

        // Metot imzası IBusinessService ile birebir aynı olmalı
        [TransactionScopeAspect]
        [LogAspect]
        [ExceptionLogAspect]
        [PerformanceAspect(2)]
        public IResult Add(BusinessForRegisterDTO businessDto, int userId)
        {
            // BusinessManager.cs dosyasındaki ilgili kısmı şöyle düzelt:
            var business = new Entities.Concrete.Business
            {
                Name = businessDto.Name,
                FullAddress = businessDto.FullAddress,
                DistrictId = businessDto.DistrictId,
                UserId = userId,
                IsApproved = false
            };

            _businessDal.Add(business);
            return new SuccessResult("İşletme oluşturma talebiniz başarıyla iletildi.");
        }
        
        public IDataResult<Entities.Concrete.Business> GetByUserId(int userId)
        {
            var business = _businessDal.Get(b => b.UserId == userId);
            if (business == null)
            {
                return new ErrorDataResult<Entities.Concrete.Business>("İşletme bulunamadı.");
            }
            return new SuccessDataResult<Entities.Concrete.Business>(business);
        }
        public IDataResult<List<BusinessDetailDto>> GetFilteredBusinesses(int? cityId, int? districtId, string search)
        {
            var result = _businessDal.GetFilteredBusinessList(cityId, districtId, search);

            return new SuccessDataResult<List<BusinessDetailDto>>(result, "Halı sahalar başarıyla listelendi.");
        }
        public IDataResult<List<Entities.Concrete.FootballField>> GetFieldsByUserId(int businessId)
        {
            var fields = _businessDal.GetFieldsByUserId(businessId);
            return new SuccessDataResult<List<Entities.Concrete.FootballField>>(fields, "İşletmenin sahaları başarıyla getirildi.");
        }
        public IDataResult<BusinessDetailDto> GetBusinessDetails(int businessId)
        {
            var result = _businessDal.GetBusinessDetails(businessId);

            if (result == null)
            {
                return new ErrorDataResult<BusinessDetailDto>("İşletme bulunamadı!");
            }

            return new SuccessDataResult<BusinessDetailDto>(result, "İşletme detayları getirildi.");
        }
        [SecuredOperation("user")]
        [TransactionScopeAspect]
        [LogAspect]
        [ExceptionLogAspect]
        [PerformanceAspect(2)]
        public IResult Update(BusinessUpdateDto businessUpdateDto)
        {
            // 1. Güncellenecek işletmeyi veritabanından bul
            var business = _businessDal.Get(b => b.Id == businessUpdateDto.BusinessId);

            if (business == null)
            {
                return new ErrorResult("Güncellenmek istenen işletme bulunamadı.");
            }

            // 2. Yeni bilgileri mevcut işletmenin üzerine yaz
            business.Name = businessUpdateDto.Name;
            business.FullAddress = businessUpdateDto.FullAddress;
            business.DistrictId = businessUpdateDto.DistrictId;
            // (Şehir ID'sini güncellemeye gerek yok çünkü DistrictId zaten şehri de kapsayan tekil bir anahtardır)

            // 3. Veritabanında güncelle
            _businessDal.Update(business);

            return new SuccessResult("İşletme bilgileri başarıyla güncellendi.");
        }

        public IDataResult<BusinessDashboardDto> GetBusinessDashboardStats(int businessId, int year)
        {
            // İleride burada "Sisteme giriş yapan kişi gerçekten bu işletmenin sahibi mi?" güvenlik kontrolü eklenecek.
            var data = _reservationDal.GetBusinessDashboardStats(businessId, year);
            return new SuccessDataResult<BusinessDashboardDto>(data, "Dashboard istatistikleri başarıyla yüklendi.");
        }
    }
}