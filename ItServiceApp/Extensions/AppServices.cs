using System;
using ItServiceApp.InjectOrnek;
using ItServiceApp.MapperProfiles;
using ItServiceApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ItServiceApp.Extensions
{
    public static class AppServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAutoMapper(options =>
            {
                options.AddProfile(typeof(AccountProfile));
                options.AddProfile(typeof(PaymentProfile));
            });

            services.AddTransient<IEmailSender, EmailSender>(); //her mail kişiye özel ve farklı oldugu için her ihtiyaç duyulduğunda tekrar üretilimesi gerekir.
            services.AddScoped<IPaymentService, IyzikoPaymentService>(); //IPaymentService kullanacagm yerde IyzikoPayment servis kullanılsın
            services.AddScoped<IMyDependency, newMyDependency>(); 
            //loose coupling
            //services.AddTransient<EmailSender>();
            return services;
        }
    }
}
