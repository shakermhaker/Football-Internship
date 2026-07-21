using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Validation;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Business.Concrete
{
    public class UserManager : IUserService
    {
        IUserDal _userDal;

        public UserManager(IUserDal userDal)
        {
            _userDal = userDal;
        }

        public List<OperationClaim> GetClaims(User user)
        {
            return _userDal.GetClaims(user);
        }
        //örnek olarak kalsınlar commentte zaten
        //[SecuredOperation("product.add,admin")]
        //[ValidationAspect(typeof(SomethingValidator))]
        //[CacheAspect]
        
        public User Get(User user)
        {
           return _userDal.Get(u => u.Id == user.Id);
        }
        public void Update(User user)
        {
            _userDal.Update(user);
        }
        //[CacheRemoveAspect("IUserService.Get")]
        

        public void Add(User user)
        {
            _userDal.Add(user);
        }

        public User GetByMail(string email)
        {
            return _userDal.Get(u => u.Email == email);
        }
        public IDataResult<UserProfileDto> GetUserProfileByEmail(string email)
        {
            // 1. Veritabanından kullanıcıyı çek
            var user = _userDal.Get(u => u.Email == email,
                include: u => u.Include(x => x.TeamAvatar));
            if (user == null)
            {
                return new ErrorDataResult<UserProfileDto>("Kullanıcı veritabanında bulunamadı.");
            }

            // 2. Entity'yi DTO'ya çevir (Hassas şifre verilerini burada eliyoruz!)
            var userProfileDto = new UserProfileDto
            {
                RowGuid = user.RowGuid,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Status = user.Status,
                TeamAvatarId = user.TeamAvatarId,
                AvatarPath = user.TeamAvatar != null
                     ? user.TeamAvatar.ImagePath
                     : ""
            };

            return new SuccessDataResult<UserProfileDto>(userProfileDto, "Kullanıcı profili getirildi.");
        }

        public IDataResult<UserProfileDto> GetUserProfileByGuid(Guid rowGuid)
        {
            var user = _userDal.Get(u => u.RowGuid == rowGuid);
            if (user == null)
            {
                return new ErrorDataResult<UserProfileDto>("Kullanıcı bulunamadı.");
            }

            var userProfileDto = new UserProfileDto
            {
                RowGuid = user.RowGuid, // int Id yok, RowGuid var!
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Status = user.Status
            };

            return new SuccessDataResult<UserProfileDto>(userProfileDto, "Kullanıcı profili getirildi.");
        }
        public async Task<IDataResult<UserProfileDto>> UpdateProfileAsync(string email, UserProfileDto updateDto)
        {
            if (string.IsNullOrEmpty(email))
            {
                return new ErrorDataResult<UserProfileDto>("Kimlik bilgisi doğrulanamadı.");
            }

            // 1. DataAccess üzerinden mevcut kullanıcı çekilir
            var user =  _userDal.Get(u => u.Email == email);
            if (user == null)
            {
                return new ErrorDataResult<UserProfileDto>("Kullanıcı bulunamadı.");
            }

            // 2. İş kuralları (Validation / Business Rules) gerekirse burada işletilir
            // Örn: var result = BusinessRules.Run(...);

            // 3. Mapping: Sadece DTO'da izin verilen ve dolu gelen alanlar Entity üzerine yazılır
            if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
                user.FirstName = updateDto.FirstName;

            if (!string.IsNullOrWhiteSpace(updateDto.LastName))
                user.LastName = updateDto.LastName;

            if (!string.IsNullOrWhiteSpace(updateDto.Phone))
                user.Phone = updateDto.Phone;

            if (updateDto.TeamAvatarId.HasValue && updateDto.TeamAvatarId.Value > 0)
            {
                user.TeamAvatarId = updateDto.TeamAvatarId.Value;
            }

            // 4. DataAccess güncellemeyi gerçekleştirir
            _userDal.Update(user);

            // 5. Güncel veri Read DTO'ya (UserProfileDto) çevrilerek frontend'e dönülür
            var updatedProfileDto = new UserProfileDto
            {
                RowGuid = user.RowGuid,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Status = user.Status,
                TeamAvatarId = user.TeamAvatarId,
                AvatarPath = user.TeamAvatar != null ? user.TeamAvatar.ImagePath : ""
            };

            return new SuccessDataResult<UserProfileDto>(updatedProfileDto, "Profil başarıyla güncellendi.");
        }

        public List<TeamAvatar> GetAvatars()
        {
            return _userDal.GetAvatars();
        }
    }
}