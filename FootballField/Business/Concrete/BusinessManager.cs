using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;

namespace Business.Concrete
{
    public class BusinessManager : IBusinessService
    {
        private readonly IBusinessDal _businessDal;

        public BusinessManager(IBusinessDal businessDal)
        {
            _businessDal = businessDal;
        }

        // Metot imzası IBusinessService ile birebir aynı olmalı
        public IResult Add(BusinessForRegisterDTO businessDto, int userId)
        {
            // BusinessManager.cs dosyasındaki ilgili kısmı şöyle düzelt:
            var business = new Entities.Concrete.Business
            {
                Name = businessDto.Name,
                FullAddress = businessDto.FullAddress,
                DistrictId = businessDto.DistrictId,
                UserId = userId,
                IsApproved = false
            };

            _businessDal.Add(business);
            return new SuccessResult("İşletme oluşturma talebiniz başarıyla iletildi.");
        }
    }
}