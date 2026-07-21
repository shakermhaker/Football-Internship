using Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Abstract
{
    public interface IFieldDal : IEntityRepository<Entities.Concrete.FootballField>
    {
    }
    
}
