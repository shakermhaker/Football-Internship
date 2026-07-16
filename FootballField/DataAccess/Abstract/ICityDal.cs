using System;
using System.Collections.Generic;
using System.Text;
using Core.DataAccess;
using Entities.Concrete; // City nesnenin olduğu yer

namespace DataAccess.Abstract
{
    // IEntityRepository sayesinde Add, Update, Delete, Get, GetAll gibi metotlar otomatik gelir.
    public interface ICityDal : IEntityRepository<City>
    {
        // Sadece İllere özel karmaşık bir veritabanı sorgusu (Join vb.) lazımsa buraya yazılır.
        // Şimdilik standart işlemler yettiği için içi boş kalıyor.
    }
}