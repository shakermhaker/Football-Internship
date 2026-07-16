using System;
using System.Collections.Generic;
using System.Text;
using Core.Utilities.Results;
using Entities.DTOs;

namespace Business.Abstract
{
    public interface IBusinessService
    {
        IResult Add(BusinessForRegisterDTO businessDto, int userId);
    }
}
