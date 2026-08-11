using Castle.DynamicProxy;
using Core.CrossCuttingConcerns.Logging;
using Core.Utilities.Interceptors;
using System;
using System.Collections.Generic;
using System.Text;
using Core.CrossCuttingConcerns.Logging;
using Serilog;


namespace Core.Aspects.Autofac.Logging
{
    public class LogAspect : MethodInterception
    {
        protected override void OnBefore(IInvocation invocation)
        {
            var logParameters = new List<LogParameter>();

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

            Log.Information("Kritik İşlem Logu: {@LogDetail}", logDetail);
        }
    }
}
