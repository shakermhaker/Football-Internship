using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using FootballField.DataAccess.Concrete.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfBusinessDal : EfEntityRepositoryBase<Business, FootballFieldContext>, IBusinessDal {

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
