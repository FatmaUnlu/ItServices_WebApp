using ItServiceApp.Data;
using ItServiceApp.Extensions;
using ItServiceApp.InjectOrnek;
using ItServiceApp.MapperProfiles;
using ItServiceApp.Models.Identity;
using ItServiceApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ItServiceApp
{
    public class Startup//proje ilk a��ld���nda buras� derlenir.O y�zden ayarlar� burda yap�yoruz
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)

        //sql server 
        {services.AddDbContext<MyContext>(options=>
        {
            options.UseSqlServer(Configuration.GetConnectionString("SqlConnection")); //burdaki SQlConnection� g�r�nce appsetting.json daki stringi al�r ve veri taban�na baglan�r.
        });
            //s�zle�me �artlar� (say� olsun mu b�yk harf olsun mu vs.)
            services.AddIdentity<ApplicationUser, ApplictionRole>(options =>
             {
                 options.Password.RequireDigit = true;
                 options.Password.RequireLowercase = false;
                 options.Password.RequireNonAlphanumeric = false;
                 options.Password.RequireUppercase = false;
                 options.Password.RequiredLength = 5;

                 options.Lockout.MaxFailedAccessAttempts = 3;
                 options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1); //3 kere yaml�� girme hakk�n� tamamlay�nca 1 dk beklesin
                options.Lockout.AllowedForNewUsers = false;

                 options.User.RequireUniqueEmail = true;
                 options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";                

             }).AddEntityFrameworkStores<MyContext>().AddDefaultTokenProviders(); 

            services.ConfigureApplicationCookie(options =>
            {
                //Cookie Settings
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);//cookie (ki�i sisteme girdi�i nanda ki�iye �zel cookie atan�r) nin ne kadar s�re devam edece�i belirlenir

                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            //Extensions-AppServices s�n�f�na eklendi.
          //  services.AddAutoMapper(options =>
          //  {
          //      options.AddProfile(typeof(AccountProfile));
          //  });

          //  // services.AddScoped<IMyDependency, MyDependency>();
          //  services.AddTransient<IEmailSender, EmailSender>();//ihtiya� duyulduk�a
          // // services.AddTransient< EmailSender>(); bu �ekilde yine enjekte yap�l�r ama loose coupling olmaz.
          //// services.AddScoped<IMyDependency, MyDependency>();//loose coupling
          //  services.AddScoped<IMyDependency, newMyDependency>();//loose coupling

            services.AddApplicationServices(this.Configuration);

            services.AddControllersWithViews(); //servise mvc oldu�unu bildirme



        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) //ortam de�i�keni geli�tirici olarak tan�mlan�nca sadece geli�tiricii ortam�nda �al���r.
            {
                app.UseDeveloperExceptionPage(); //canl�da �al��maz. sadece geli�tirmede
            }
          
            app.UseStaticFiles();//wwwroot klas�r� i�erisindeki yap�lar� kullanmam�z� sa�layan komut css-js vs.
            app.UseHttpsRedirection(); //uygulama httpde de �al��s�n

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider=new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(),@"node_modules")),
                RequestPath = new PathString("/vendor")
            });

            app.UseRouting();//

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //Area ��in
                endpoints.MapAreaControllerRoute("Admin", "Admin", "Admin/{controller=Manage}/{action=Index}/{Id?}"); //name,areaName,pattern

                endpoints.MapControllerRoute( 
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{Id?}");//varsay�lan bir routing olu�turuldu. Home controller�ndaki �ndex sayfas�na y�nlendirir ilk a��l��ta.
            });
        }
    }
}
