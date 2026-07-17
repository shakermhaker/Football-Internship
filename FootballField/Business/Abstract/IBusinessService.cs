using System;
using System.Collections.Generic;
using System.Text;
using Core.Utilities.Results;
using Entities.DTOs;
using Entities.Concrete; 

namespace Business.Abstract
{
    public interface IBusinessService
    {
        IResult Add(BusinessForRegisterDTO businessDto, int userId);
        IDataResult<Entities.Concrete.Business> GetByUserId(int userId);
        IDataResult<List<Entities.Concrete.Business>> GetFilteredBusinesses(int? cityId, int? districtId, string? search);
    }
}
