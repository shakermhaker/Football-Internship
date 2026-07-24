using System;
using System.Collections.Generic;
using System.Text;
using Core.Utilities.Results;
using Entities.DTOs;

namespace Business.Abstract
{
    public interface IFieldService
    {
        IResult AddWithDetails(FootballFieldAddDTO fieldDto);
        IResult DeleteField(int fieldId);
        IResult UpdateWithSchedules(FootballFieldAddDTO fieldDto, int fieldId);
        IDataResult<FootballFieldAddDTO> GetFieldForEdit(int fieldId);
    }
}