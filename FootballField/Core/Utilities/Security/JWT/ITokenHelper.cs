using Core.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Core.Utilities.Security.JWT
{
    public interface ITokenHelper
    {
        AccessToken CreateToken(ICoreUser user, IEnumerable<IOperationClaim> operationClaims);
    }
}

//23.05 Dersteyiz