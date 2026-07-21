using System;
using System.Collections.Generic;
using System.Text;
using Core.DataAccess;
using Entities.Concrete; // District nesnenin olduğu yer

namespace DataAccess.Abstract
{
    public interface IDistrictDal : IEntityRepository<District>
    {
    }
}
