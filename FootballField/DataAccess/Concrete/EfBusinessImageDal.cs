using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using FootballField.DataAccess.Concrete.EntityFramework;

namespace DataAccess.Concrete
{
    public class EfBusinessImageDal : EfEntityRepositoryBase<BusinessImage, FootballFieldContext>, IBusinessImageDal
    {
    }
}
