using Core.Utilities.Results;
using Core.Utilities.Security.JWT;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IAuthService
    {
        User Get(User user);

        Task<IDataResult<User>> Register(UserForRegisterDto userForRegisterDto, string password);
        IDataResult<User> Login(UserForLoginDto userForLoginDto);
        IResult UserExists(string email);
        IDataResult<AccessToken> CreateAccessToken(User user);
        Task<bool> SendVerificationEmailAsync(User user);
        Task<IResult> ResendVerificationEmailAsync(string email);
        Task<IDataResult<User>> VerifyUserAccountAsync(int userId, string token);
    }
}