using Core.DataAccess.EntityFramework; // Kendi Core katmanına göre düzenle
using DataAccess.Abstract;
using Entities.Concrete;
using FootballField.DataAccess.Concrete.EntityFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfFieldDal : EfEntityRepositoryBase<Entities.Concrete.FootballField, FootballFieldContext>, IFieldDal
    {
    }
}