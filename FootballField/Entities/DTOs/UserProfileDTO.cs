using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
        public class UserProfileDto
        {
            public Guid RowGuid { get; set; } 
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public bool Status { get; set; }
            public bool? HasBusiness { get; set; }
            public bool? IsBusinessApproved { get; set; }
            public string? BusinessName { get; set; }
            public string? AvatarPath { get; set; }
            public int? TeamAvatarId { get; set; }
        }
}
