using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Business.Concrete
{
    public class BusinessImageManager : IBusinessImageService
    {
        private readonly IBusinessImageDal _businessImageDal;
        public BusinessImageManager(IBusinessImageDal businessImageDal)
        {
            _businessImageDal = businessImageDal;
        }
        public async Task<Core.Utilities.Results.IResult> AddImageAsync(int businessId, IFormFile file, bool isCover)
        {
            // KURAL 1: Mevcut resim sayısını kontrol et (Max 5)
            var currentImages = _businessImageDal.GetAll(b => b.BusinessId == businessId);

            if (currentImages.Count >= 5)
            {
                return new ErrorResult("Bir işletmeye en fazla 5 adet görsel eklenebilir!");
            }

            // KURAL 2: Eğer yeni eklenen resim "Kapak (IsCover = true)" olarak geliyorsa,
            // eskisinin kapak özelliğini iptal etmemiz lazım ki DB'de sadece 1 kapak kalsın.
            if (isCover)
            {
                var existingCover = currentImages.FirstOrDefault(i => i.IsCover);
                if (existingCover != null)
                {
                    existingCover.IsCover = false;
                    _businessImageDal.Update(existingCover); // Eskisini normale çevir
                }
            }
            // EKSTRA GÜZELLİK: Eğer bu işletmenin ilk yüklenen resmiyse, mecburen kapak olsun.
            else if (currentImages.Count == 0)
            {
                isCover = true;
            }

            // 3. Dosyayı Sunucuya / Klasöre Kaydetme İşlemi
            // (Benzersiz bir isim oluşturuyoruz ki dosyalar çakışmasın)
            string extension = Path.GetExtension(file.FileName);
            string newFileName = Guid.NewGuid().ToString() + extension;

            // wwwroot/uploads/businessImages/ klasörüne kaydedeceğiz
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "businessImages");

            // Klasör yoksa oluştur
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string filePath = Path.Combine(uploadsFolder, newFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 4. Veritabanına Yolu Kaydet
            var businessImage = new BusinessImage
            {
                BusinessId = businessId,
                ImagePath = $"/uploads/businessImages/{newFileName}", // Angular bu yolu okuyacak
                IsCover = isCover
            };

            _businessImageDal.Add(businessImage);

            return new SuccessResult("Görsel başarıyla yüklendi."); 
        }
        public async Task<Core.Utilities.Results.IResult> DeleteImageAsync(int imageId)
        {
            // 1. Resmi veritabanından bul
            var image = _businessImageDal.Get(i => i.Id == imageId);
            if (image == null)
            {
                return new ErrorResult("Silinecek görsel bulunamadı.");
            }

            // 2. Fiziksel dosyayı wwwroot klasöründen sil (Temizlik şart!)
            if (!string.IsNullOrEmpty(image.ImagePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImagePath.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            // 3. Veritabanı kaydını sil
            _businessImageDal.Delete(image);

            return new SuccessResult("Görsel başarıyla silindi.");
        }
    }
}
