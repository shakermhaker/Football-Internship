using Castle.DynamicProxy;
using Core.Utilities.Interceptors;
using Core.Utilities.IoC;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Aspects.Autofac.Performance
{
    public class PerformanceAspect : MethodInterception
    {
        private int _interval; 
        private Stopwatch _stopwatch;

        
        public PerformanceAspect(int interval = 3)
        {
            _interval = interval;
            _stopwatch = ServiceTool.ServiceProvider.GetService<Stopwatch>();
        }

        protected override void OnBefore(IInvocation invocation)
        {
            _stopwatch.Start();
        }

        protected override void OnAfter(IInvocation invocation)
        {
            var methodName = $"{invocation.Method.DeclaringType.FullName}.{invocation.Method.Name}";
            var totalSeconds = _stopwatch.Elapsed.TotalSeconds;

            // 1. DURUM: İşlem bizim belirlediğimiz "Kritik Sınırı" aştıysa (Yavaş çalıştıysa)
            if (totalSeconds > _interval)
            {
                Log.Warning("SİSTEM YAVAŞLAMASI -> Metot: {MethodName} | Hedeflenen: Maks {Expected} sn | Gerçekleşen: {Actual} sn",
                            methodName, _interval, totalSeconds);
            }
            // 2. DURUM: İşlem rutin (normal) hızında tamamlandıysa
            else
            {
                // Bilgi seviyesinde (INFO) işlemin ne kadar sürdüğünü logla
                Log.Information("Performans Metriği -> Metot: {MethodName} | Çalışma Süresi: {Actual} sn",
                                methodName, totalSeconds);
            }

            _stopwatch.Reset();
        }
    }
}
