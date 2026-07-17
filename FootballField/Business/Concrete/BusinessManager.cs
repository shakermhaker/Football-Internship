using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;

namespace Business.Concrete
{
    public class BusinessManager : IBusinessService
    {
        private readonly IBusinessDal _businessDal;

        public BusinessManager(IBusinessDal businessDal)
        {
            _businessDal = businessDal;
        }

        // Metot imzası IBusinessService ile birebir aynı olmalı
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
        public IDataResult<List<Entities.Concrete.Business>> GetFilteredBusinesses(int? cityId, int? districtId, string? search)
        {
            var result = _businessDal.GetAll(b =>
                // 1. Durum: İlçe seçilmişse direkt ilçeye göre filtrele (Şehre bakmaya gerek yok)
                (!districtId.HasValue || b.DistrictId == districtId.Value) &&

                // 2. Durum: Sadece Şehir seçilmişse (İlçe boşsa), Navigation Property (District) üzerinden CityId'ye in!
                (!cityId.HasValue || districtId.HasValue || b.District.CityId == cityId.Value) &&

                // 3. Durum: Arama kutusunda metin varsa isme göre filtrele
                (string.IsNullOrEmpty(search) || b.Name.ToLower().Contains(search.ToLower()))
            );

            return new SuccessDataResult<List<Entities.Concrete.Business>>(result, "Halı sahalar başarıyla listelendi.");
        }
    }
}