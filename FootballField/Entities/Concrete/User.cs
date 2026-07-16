using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Concrete
{
    public class User : IEntity , ICoreUser
    {
        public int Id { get; set; }
        public Guid RowGuid { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public byte[] PasswordSalt { get; set; }
        public byte[] PasswordHash { get; set; }

        public string Phone { get; set; } = string.Empty;
        public bool Status { get; set; }

        public bool IsEmailConfirmed { get; set; }
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpires { get; set; }
        public DateTime BirthDate { get; set; }

        // Navigation Properties
        //public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<UserOperationClaim> UserOperationClaims { get; set; } = new List<UserOperationClaim>();

        public ICollection<Business> UserBusinesses { get; set; } = new List<Business>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
