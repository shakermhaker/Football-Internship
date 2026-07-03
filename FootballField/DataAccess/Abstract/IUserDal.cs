using Core.DataAccess;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Entities.Concrete;
namespace DataAccess.Abstract
{
    public interface IUserDal : IEntityRepository<User>
    {
        List<OperationClaim> GetClaims(ICoreUser user);
    }
}