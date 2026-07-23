using Autofac;
using Autofac.Extras.DynamicProxy;
using Business.Abstract;
using Business.Concrete;
using Castle.DynamicProxy;
using Core.Utilities.Interceptors;
using Core.Utilities.Security.JWT;
using DataAccess.Abstract;
using DataAccess.Concrete;
using DataAccess.Concrete.EntityFramework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Module = Autofac.Module;


namespace Business.DependencyResolvers.Autofac
{
    public class AutofacBusinessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterType<EmailManager>().As<IEmailService>();

            builder.RegisterType<UserManager>().As<IUserService>();
            builder.RegisterType<EfUserDal>().As<IUserDal>();

            builder.RegisterType<EfFieldDal>().As<IFieldDal>();
            builder.RegisterType<EfFieldPriceSheduleDal>().As<IFieldPriceSheduleDal>();

            builder.RegisterType<FieldManager>().As<IFieldService>();
            builder.RegisterType<FieldPriceScheduleManager>().As<IFieldPriceScheduleService>();


            builder.RegisterType<AuthManager>().As<IAuthService>();
            builder.RegisterType<JwtHelper>().As<ITokenHelper>();

            builder.RegisterType<EfTimeSlotDal>().As<ITimeSlotDal>();




            builder.RegisterType<HttpContextAccessor>().As<HttpContextAccessor>();

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces()
                .EnableInterfaceInterceptors(new ProxyGenerationOptions()
                {
                    Selector = new AspectInterceptorSelector()
                }).SingleInstance();
            // Veri Erişim (DAL) Katmanı Kayıtları
            builder.RegisterType<EfCityDal>().As<ICityDal>().SingleInstance();
            builder.RegisterType<EfDistrictDal>().As<IDistrictDal>().SingleInstance();

            // İş (Business) Katmanı Kaydı
            builder.RegisterType<LocationsManager>().As<ILocationService>().SingleInstance();
            builder.RegisterType<BusinessManager>().As<IBusinessService>().SingleInstance();
            builder.RegisterType<EfBusinessDal>().As<IBusinessDal>().SingleInstance();
        }

    }
}