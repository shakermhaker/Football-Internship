using Entities.Concrete;
using Core.Entities;

namespace Entities.Concrete
{
    public class OperationClaim : IEntity, IOperationClaim
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<UserOperationClaim> UserOperationClaims { get; set; } = new List<UserOperationClaim>();

    }
}
