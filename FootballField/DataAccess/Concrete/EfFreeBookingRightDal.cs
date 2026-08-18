using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using FootballField.DataAccess.Concrete.EntityFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Concrete
{
    public class EfFreeBookingRightDal : EfEntityRepositoryBase<FreeBookingRight, FootballFieldContext>, IFreeBookingRightDal
    {
    }
}
