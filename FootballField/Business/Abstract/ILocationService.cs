using Core.Utilities.Results;
using System;
using System.Collections.Generic;
using System.Text;
using Entities.DTOs;

namespace Business.Abstract
{
    // Örnek ILocationService içeriği
    public interface ILocationService
    {
        IDataResult<List<CityDTO>> GetAllCities();
        IDataResult<List<DistrictDTO>> GetDistrictsByCityId(int cityId);
    }
}
