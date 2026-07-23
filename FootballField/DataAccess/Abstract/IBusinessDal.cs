using System;
using System.Collections.Generic;
using System.Text;
using Core.DataAccess;
using Entities.Concrete;

namespace DataAccess.Abstract
{
    public interface IBusinessDal : IEntityRepository<Business> {
        List<Entities.Concrete.FootballField> GetFieldsByUserId(int businessId);
    }
}