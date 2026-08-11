using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Text;
using Core.Utilities.Results;

namespace Business.Abstract
{
    public interface IUserOperationClaimService
    {
        IResult Add(UserOperationClaim userOperationClaim);

        // İleride lazım olabilecek metotlar
        IResult Update(UserOperationClaim userOperationClaim);
        IResult Delete(UserOperationClaim userOperationClaim);
    }
}
