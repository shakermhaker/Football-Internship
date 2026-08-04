using Core.DataAccess.EntityFramework;
using Core.Entities;
using DataAccess.Abstract;
using Entities.Concrete;
using FootballField.DataAccess.Concrete.EntityFramework;
using System;
using System.Collections.Generic;
using System.Text;
using DataAccess.Abstract;
namespace DataAccess.Concrete
{
    public class EfUserOperationClaimDal: EfEntityRepositoryBase<UserOperationClaim, FootballFieldContext>, IUserOperationClaimDal
    {
    }
}
