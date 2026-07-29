using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp; // YENİ: WebP Sıkıştırma için
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // --- YENİ EKLENEN GÜVENLİK KONTROLLERİ ---
            // 1. Maksimum 5 MB Sınırı
            if (file.Length > 5 * 1024 * 1024)
            {
                return new ErrorResult("Dosya boyutu çok büyük! Maksimum 5MB yükleyebilirsiniz.");
            }

            // 2. Sadece resim dosyalarına izin ver
            if (!file.ContentType.StartsWith("image/"))
            {
                return new ErrorResult("Lütfen sadece geçerli bir resim dosyası yükleyin.");
            }
            // ----------------------------------------

            // KURAL 1: Mevcut resim sayısını kontrol et (Max 5)
            var currentImages = _businessImageDal.GetAll(b => b.BusinessId == businessId);

            if (currentImages.Count >= 5)
            {
                return new ErrorResult("Bir işletmeye en fazla 5 adet görsel eklenebilir!");
            }

            // KURAL 2: Kapak fotoğrafı güncellemesi
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

            // 3. Dosyayı İşleyerek Sunucuya Kaydetme İşlemi
            // Artık orijinal uzantıyı umursamıyoruz, her şeyi .webp yapacağız!
            string newFileName = Guid.NewGuid().ToString() + ".webp";
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "businessImages");

            // Klasör yoksa oluştur
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string filePath = Path.Combine(uploadsFolder, newFileName);

            // --- YENİ EKLENEN IMAGESHARP İŞLEME KISMI ---
            using (var stream = file.OpenReadStream())
            {
                // Resmi güvenli bir şekilde belleğe alıyoruz
                using (var image = await Image.LoadAsync(stream))
                {
                    // Resmi 800x600 boyutunda, taşırmadan ortalayarak kırp (Tasarımların milimetrik olsun)
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(800, 600),
                        Mode = ResizeMode.Crop
                    }));

                    // Kaliteyi %75 yaparak WebP formatında kaydet (Dosya boyutu inanılmaz küçülür)
                    var encoder = new WebpEncoder { Quality = 75 };
                    await image.SaveAsync(filePath, encoder);
                }
            }
            // ----------------------------------------

            // 4. Veritabanına Yolu Kaydet
            var businessImage = new BusinessImage
            {
                BusinessId = businessId,
                ImagePath = $"/uploads/businessImages/{newFileName}", // Artık .webp uzantılı kaydediliyor
                IsCover = isCover
            };

            _businessImageDal.Add(businessImage);

            return new SuccessResult("Görsel başarıyla yüklenip optimize edildi.");
        }

        public async Task<Core.Utilities.Results.IResult> DeleteImageAsync(int imageId)
        {
            // SİLME METODU AYNEN KALIYOR (Çünkü DB'den okuyup siliyor, uzantının değişmesi bunu bozmaz)
            var image = _businessImageDal.Get(i => i.Id == imageId);
            if (image == null)
            {
                return new ErrorResult("Silinecek görsel bulunamadı.");
            }

            if (!string.IsNullOrEmpty(image.ImagePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImagePath.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            _businessImageDal.Delete(image);

            return new SuccessResult("Görsel başarıyla silindi.");
        }
    }
}