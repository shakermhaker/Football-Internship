using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Concrete
{
    public class TeamAvatar : IEntity
    {
        public int Id { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public ICollection<User> TeamAvatarId{ get; set; } = new List<User>();

    }
}
