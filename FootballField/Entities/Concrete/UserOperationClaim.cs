using Entities.Concrete;
using Core.Entities;

namespace Entities.Concrete
{
    public class UserOperationClaim : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int OperationClaimId { get; set; }
        public OperationClaim OperationClaim { get; set; } = null!;
    }
}
