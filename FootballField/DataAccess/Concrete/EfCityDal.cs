using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using FootballField.DataAccess.Concrete.EntityFramework;
using System;
using System.Collections.Generic;
using System.Text;
// using DataAccess.Concrete.EntityFramework.Contexts; // Context'in olduğu namespace

namespace DataAccess.Concrete.EntityFramework
{
    public class EfCityDal : EfEntityRepositoryBase<City, FootballFieldContext>, ICityDal
    {
    }
}