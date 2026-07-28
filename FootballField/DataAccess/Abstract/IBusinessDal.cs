using Core.DataAccess;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Abstract
{
    public interface IBusinessDal : IEntityRepository<Business> {
        List<Entities.Concrete.FootballField> GetFieldsByUserId(int businessId);
        BusinessDetailDto GetBusinessDetails(int businessId);
        List<BusinessDetailDto> GetFilteredBusinessList(int? cityId, int? districtId, string search);
    }
}