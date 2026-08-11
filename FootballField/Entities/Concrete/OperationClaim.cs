using Entities.Concrete;
using Core.Entities;

namespace Entities.Concrete
{
    public class OperationClaim : BaseEntity, IOperationClaim
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<UserOperationClaim> UserOperationClaims { get; set; } = new List<UserOperationClaim>();

    }
}
