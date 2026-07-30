using Castle.DynamicProxy;
using Core.Utilities.Interceptors;
using System;
using System.Transactions;

namespace Core.Aspects.Autofac.Transaction
{
    /// <summary>
    /// Bu Aspect, metodun çalışmasını bir TransactionScope içerisine alır.
    /// Metot sorunsuz biterse işlem veritabanına yansıtılır (Commit).
    /// Metot içinde bir hata (Exception) fırlatılırsa tüm DB işlemleri geri alınır (Rollback).
    /// </summary>
    public class TransactionScopeAspect : MethodInterception
    {
        public override void Intercept(IInvocation invocation)
        {
            // TransactionScope seçenekleri: Yeni bir scope başlat ve asenkron akışa izin ver
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.MaximumTimeout
            };

            using (TransactionScope transactionScope = new TransactionScope(
                TransactionScopeOption.Required,
                transactionOptions,
                TransactionScopeAsyncFlowOption.Enabled)) // Asenkron metotlar (async/await) ile uyumlu çalışması için
            {
                try
                {
                    // İlgili metodu çalıştır
                    invocation.Proceed();

                    // Hata yoksa işlemi onayla (Commit)
                    transactionScope.Complete();
                }
                catch (System.Exception e)
                {
                    // Hata fırlatılırsa catch bloğuna düşer ve .Complete() çağrılmadığı için işlem otomatik olarak İptal edilir (Rollback).
                    // Loglama işlemi yapabilirsin
                    // transactionScope.Dispose(); // using bloğu olduğu için manuel çağırmaya gerek yok ama mantığı anlamak için eklenebilir.

                    throw; // Hatayı yutma, dışarı fırlat ki API / kullanıcı hatadan haberdar olsun.
                }
            }
        }
    }
}