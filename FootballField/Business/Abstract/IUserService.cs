
using Entities.Concrete;
using System.Collections.Generic;

namespace Business.Abstract
{
    public interface IUserService
    {
        List<OperationClaim> GetClaims(User user);
        User Get(User user);
        void Update(User user);
        void Add(User user);
        User GetByMail(string email);
    }
}