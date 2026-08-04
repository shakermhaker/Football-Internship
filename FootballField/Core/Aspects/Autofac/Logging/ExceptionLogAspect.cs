using Castle.DynamicProxy;
using Core.CrossCuttingConcerns.Logging;
using Core.Utilities.Interceptors;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Aspects.Autofac.Logging
{
    public class ExceptionLogAspect : MethodInterception
    {
        protected override void OnException(IInvocation invocation, Exception e)
        {
            var logParameters = new List<LogParameter>();

            // Hata anında metoda hangi parametrelerin geldiğini yakala (Örn: Hangi id'yi silmeye çalıştı da patladı?)
            for (int i = 0; i < invocation.Arguments.Length; i++)
            {
                logParameters.Add(new LogParameter
                {
                    Name = invocation.GetConcreteMethod().GetParameters()[i].Name,
                    Value = invocation.Arguments[i],
                    Type = invocation.Arguments[i].GetType().Name
                });
            }

            var logDetail = new LogDetail
            {
                MethodName = invocation.Method.Name,
                LogParameters = logParameters
            };

            // Serilog'un .Error metodunu kullanıyoruz. Hatanın kendisini (e) ve parametreleri kaydediyoruz.
            Log.Error(e, "Sistem Hatası (Exception): {@LogDetail}", logDetail);
        }
    }
}
