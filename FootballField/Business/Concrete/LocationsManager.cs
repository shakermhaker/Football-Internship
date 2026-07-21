using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs; // CityDto ve DistrictDto'nun burada olduğunu varsayıyoruz
using System.Collections.Generic;
using System.Linq;

namespace Business.Concrete
{
    public class LocationsManager : ILocationService
    {
        private readonly ICityDal _cityDal;
        private readonly IDistrictDal _districtDal;

        // Constructor Injection ile Veri Erişim Katmanı (DAL) soyutlamalarını alıyoruz
        public LocationsManager(ICityDal cityDal, IDistrictDal districtDal)
        {
            _cityDal = cityDal;
            _districtDal = districtDal;
        }

        public IDataResult<List<CityDTO>> GetAllCities()
        {
            // 1. Veritabanından tüm illeri çek (IEntityRepository'deki GetAll metodu)
            var cities = _cityDal.GetAll();

            if (cities == null || !cities.Any())
            {
                return new ErrorDataResult<List<CityDTO>>("Sistemde kayıtlı il bulunamadı.");
            }

            // 2. Entity'leri DTO'ya çevir (Tıpkı UserProfileDto'da yaptığın gibi)
            var cityDtos = cities.Select(c => new CityDTO
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();

            // 3. Başarılı sonucu döndür
            return new SuccessDataResult<List<CityDTO>>(cityDtos, "İller başarıyla getirildi.");
        }

        public IDataResult<List<DistrictDTO>> GetDistrictsByCityId(int cityId)
        {
            // 1. İlgili ilin plakasına (CityId) göre ilçeleri filtreleyip getir
            var districts = _districtDal.GetAll(d => d.CityId == cityId);

            if (districts == null || !districts.Any())
            {
                return new ErrorDataResult<List<DistrictDTO>>("Bu ile ait ilçe bulunamadı veya geçersiz il kodu.");
            }

            // 2. Entity'leri DTO'ya çevir
            var districtDtos = districts.Select(d => new DistrictDTO
            {
                Id = d.Id,
                Name = d.Name,
                CityId = d.CityId
            }).ToList();

            // 3. Başarılı sonucu döndür
            return new SuccessDataResult<List<DistrictDTO>>(districtDtos, "İlçeler başarıyla getirildi.");
        }
    }
}