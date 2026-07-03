using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public interface IOperationClaim : IEntity
    {
        int Id { get; set; }
        string Name { get; set; }
    }
}
