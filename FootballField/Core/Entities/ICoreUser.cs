using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public interface ICoreUser : IEntity
    {
        int Id { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Email { get; set; }
        byte[] PasswordHash { get; set; }
        byte[] PasswordSalt { get; set; }
        bool Status { get; set; }
    }
}
