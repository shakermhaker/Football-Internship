using Core.DataAccess.EntityFramework;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using FootballField.DataAccess.Concrete.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfBusinessDal : EfEntityRepositoryBase<Business, FootballFieldContext>, IBusinessDal {
        public BusinessDetailDto GetBusinessDetails(int businessId)
        {
            using (FootballFieldContext context = new FootballFieldContext())
            {
                var result = from b in context.Businesses
                             where b.Id == businessId


                             where b.Id == businessId && b.IsApproved == true
                             select new BusinessDetailDto
                             {
                                 BusinessId = b.Id,
                                 Name = b.Name,
                                 FullAddress = b.FullAddress,
                                 District = b.District.Name,
                                 City = b.District.City.Name, // Kendi yapına göre bağlayabilirsin
                                 Images = context.BusinessImages
                                            .Where(i => i.BusinessId == b.Id)
                                            .Select(i => new BusinessImageDto
                                            {
                                                Id = i.Id,
                                                ImagePath = i.ImagePath,
                                                IsCover = i.IsCover
                                            }).ToList()
                             };

                return result.FirstOrDefault();
            }
        }

        public List<BusinessDetailDto> GetFilteredBusinessList(int? cityId, int? districtId, string search)
        {
            using (var context = new FootballFieldContext())
            {

                var query = context.Businesses.Where(b => b.IsApproved == true).AsQueryable();

                // 1. Durum: İlçe seçilmişse
                if (districtId.HasValue)
                {
                    query = query.Where(b => b.DistrictId == districtId.Value);
                }
                // 2. Durum: Sadece Şehir seçilmişse
                else if (cityId.HasValue)
                {
                    query = query.Where(b => b.District.CityId == cityId.Value);
                }

                // 3. Durum: Arama metni
                if (!string.IsNullOrEmpty(search))
                {
                    var lowerSearch = search.ToLower();
                    query = query.Where(b => b.Name.ToLower().Contains(lowerSearch));
                }

                // 🚀 BÜYÜ BURADA: EF Core ile veritabanından okurken SENİN DTO'na eşleştiriyoruz
                var result = query.Select(b => new BusinessDetailDto
                {
                    BusinessId = b.Id,
                    Name = b.Name,
                    City = b.District.City.Name,      // Senin DTO'nda "City"
                    District = b.District.Name,       // Senin DTO'nda "District"
                    FullAddress = b.FullAddress,      // Veritabanında adres alanı olduğunu varsayıyorum

                    // İşletmenin resimlerini senin BusinessImageDto listene dolduruyoruz
                    Images = b.BusinessImages.Select(img => new BusinessImageDto
                    {
                        Id = img.Id,
                        ImagePath = img.ImagePath,
                        IsCover = img.IsCover
                    }).ToList()
                }).ToList();

                return result;
            }
        }




        public List<Entities.Concrete.FootballField> GetFieldsByUserId(int businessId)
        {
            using (var context = new FootballFieldContext())
            {
                // İki tabloyu birleştirip, UserId'si token'dan gelenle eşleşen sahaları çekiyoruz
                return context.FootballFields

                    .Where(f => f.BusinessId == businessId) // Dahil ettiğin işletmenin UserId'sine göre filtrele
                    .ToList(); // Listeye çevirip postala
            }
        }


    }
}
