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
