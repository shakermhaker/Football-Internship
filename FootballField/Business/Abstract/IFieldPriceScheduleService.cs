using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Text;
using Core.Utilities.Results;

namespace Business.Abstract
{
    public interface IFieldPriceScheduleService
    {
        IResult Add(FieldPriceSchedule fieldPriceSchedule);
    }
}
