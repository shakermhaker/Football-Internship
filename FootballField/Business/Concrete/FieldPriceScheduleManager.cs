using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Text;


namespace Business.Concrete
{
    public class FieldPriceScheduleManager : IFieldPriceScheduleService
    {

        private readonly IFieldPriceSheduleDal _fieldPriceSheduleDal;

        public FieldPriceScheduleManager(IFieldPriceSheduleDal fieldPriceSheduleDal)
        {
            _fieldPriceSheduleDal = fieldPriceSheduleDal;
        }
        public IResult Add(FieldPriceSchedule fieldPriceSchedule)
        {
            _fieldPriceSheduleDal.Add(fieldPriceSchedule);
            return new SuccessResult("Fiyatlandırma başarıyla eklendi.");
        }


    }
}
