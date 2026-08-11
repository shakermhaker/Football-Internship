using Business.Abstract;
using Core.Aspects.Autofac.Logging;
using Core.Aspects.Autofac.Performance;
using Core.Aspects.Autofac.Transaction;
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

       
        [TransactionScopeAspect]
        [LogAspect]
        [ExceptionLogAspect]
        [PerformanceAspect(2)]
        public IResult Add(FieldPriceSchedule fieldPriceSchedule)
        {
            _fieldPriceSheduleDal.Add(fieldPriceSchedule);
            return new SuccessResult("Fiyatlandırma başarıyla eklendi.");
        }


    }
}
