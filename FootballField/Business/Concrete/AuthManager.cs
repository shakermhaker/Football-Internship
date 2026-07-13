using Business.Abstract;
using Core.Utilities.Results;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.JWT;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Concrete
{
    public class AuthManager : IAuthService
    {
        private IUserService _userService;
        private ITokenHelper _tokenHelper;
        private IEmailService _emailService;
        private AppUrlSettings _appUrls;

        public AuthManager(IUserService userService, ITokenHelper tokenHelper, IOptions<AppUrlSettings> appUrls, IEmailService emailService)
        {
            _userService = userService;
            _tokenHelper = tokenHelper;
            _appUrls = appUrls.Value;
            _emailService = emailService;

        }
        public User Get(User user)
        {
            var s_user = new User
            {
                Id = user.Id,
            };
            return user = _userService.Get(s_user);
        }

        public async Task<IResult> ResendVerificationEmailAsync(string email)
        {
            
            var user = _userService.GetByMail(email);
            if (user == null)
            {
                return new ErrorResult("Bu e-posta adresine ait bir kullanıcı bulunamadı.");
            }

            
            if (user.IsEmailConfirmed == true) 
            {
                return new ErrorResult("Bu hesap zaten doğrulanmış. Doğrudan giriş yapabilirsiniz.");
            }

            await SendVerificationEmailAsync(user);

            return new SuccessResult("Doğrulama bağlantısı e-posta adresinize tekrar gönderildi. Lütfen gelen kutunuzu kontrol edin.");
        }

        public async Task<bool> SendVerificationEmailAsync(User user)
        {
            string token = Guid.NewGuid().ToString(); 

            string encodedToken = Uri.EscapeDataString(token);

            
            string verificationUrl = $"{_appUrls.BaseApiUrl}/api/Auth/verifyUserAccount?userId={user.Id}&token={encodedToken}";

            string htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e9ecef; border-radius: 8px;'>
                <h2 style='color: #198754; text-align: center;'>Halı Saha Sistemine Hoş Geldin, {user.FirstName}!</h2>
                <p style='color: #555; font-size: 16px;'>Hesabını oluşturduk, ancak sahaya çıkıp maç ayarlayabilmen için önce e-posta adresini doğrulamamız gerekiyor.</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{verificationUrl}' style='background-color: #198754; color: white; padding: 14px 28px; text-decoration: none; font-weight: bold; border-radius: 6px; display: inline-block;'>
                        ⚽ Hesabımı Doğrula ve Sahaya Çık
                    </a>
                </div>
                <hr style='border: none; border-top: 1px solid #eee;' />
                <p style='color: #999; font-size: 12px; text-align: center;'>Eğer bu hesabı sen oluşturmadıysan bu e-postayı görmezden gelebilirsin.</p>
            </div>";

            // 5. Mail servisine fırlatıyoruz!
            await _emailService.SendEmailAsync(user.Email, "⚽ Halı Saha Hesabını Doğrula", htmlBody);
            return true;
        }

        public async Task<IDataResult<User>> VerifyUserAccountAsync(int userId, string token)
        {
            // 1. Kullanıcıyı ID üzerinden veritabanından çekiyoruz
            // (Senin yapında metodun adı GetById, Get ya da GetAsync olabilir)
            var s_user = new User
            {
                Id = userId,
            };
            var user = _userService.Get(s_user);

            if (user == null)
            {
                return new ErrorDataResult<User>("Kullanıcı bulunamadı.");
            }

            // 2. Kullanıcının hesabı zaten aktifse boşa işlem yapmayalım
            if (user.IsEmailConfirmed == true) // Eğer yapında IsEmailConfirmed diye ayrı bir alan varsa onu kontrol et
            {
                return new SuccessDataResult<User>(user, "Hesabınız zaten doğrulanmış.");
            }

            
            user.IsEmailConfirmed = true;

            
            _userService.Update(user);

            return new SuccessDataResult<User>(user, "E-posta adresiniz başarıyla doğrulanmış ve hesabınız aktif edilmiştir!");
        }

        public async Task<IDataResult<User>> Register(UserForRegisterDto userForRegisterDto, string password)
        {
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(password, out passwordHash, out passwordSalt);
            var user = new User
            {
                Email = userForRegisterDto.Email,
                FirstName = userForRegisterDto.FirstName,
                LastName = userForRegisterDto.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Status = true
            };
            _userService.Add(user);
            await SendVerificationEmailAsync(user);
            
            return new SuccessDataResult<User>(user, "Kayıt oldu");
        }

        public IDataResult<User> Login(UserForLoginDto userForLoginDto)
        {
            var userToCheck = _userService.GetByMail(userForLoginDto.Email);
            if (userToCheck == null)
            {
                return new ErrorDataResult<User>("Kullanıcı bulunamadı");
            }

            if (!HashingHelper.VerifyPasswordHash(userForLoginDto.Password, userToCheck.PasswordHash, userToCheck.PasswordSalt))
            {
                return new ErrorDataResult<User>("Parola hatası");
            }
            if(userToCheck.IsEmailConfirmed != true)
            {
                return new DataResult<User>(userToCheck,false,"Lütfen e-posta adresinizi doğrulayın.");
            }

            return new SuccessDataResult<User>(userToCheck, "Başarılı giriş");
        }

        public IResult UserExists(string email)
        {
            if (_userService.GetByMail(email) != null)
            {
                return new ErrorResult("Kullanıcı mevcut");
            }
            return new SuccessResult();
        }

        public IDataResult<AccessToken> CreateAccessToken(User user)
        {
            var claims = _userService.GetClaims(user);
            var accessToken = _tokenHelper.CreateToken(user, claims);
            return new SuccessDataResult<AccessToken>(accessToken, "Token oluşturuldu");
        }
    }
}