
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.DTOs;
using System.Collections.Generic;

namespace Business.Abstract
{
    public interface IUserService
    {
        List<OperationClaim> GetClaims(User user);
        User Get(User user);
        List<TeamAvatar> GetAvatars();
        void Update(User user);
        void Add(User user);
        User GetByMail(string email);
        IDataResult<UserProfileDto> GetUserProfileByEmail(string email);
        IDataResult<UserProfileDto> GetUserProfileByGuid(Guid rowGuid);
        Task<IDataResult<UserProfileDto>> UpdateProfileAsync(string email, UserProfileDto updateDto);
        
    }
}