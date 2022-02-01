using ItServiceApp.Models.Entities;
using ItServiceApp.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ItServiceApp.Data
{
    public class MyContext :IdentityDbContext<ApplicationUser, ApplictionRole,string>
    {
        public MyContext(DbContextOptions options) :base(options) //base:kalıtım alınan clasın constructorına gider.Sql connectiona erişir.  varsayılan ıd tipi string guid
        {

        }

        //override yapmak için 
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); //default ayarları getirir.

            builder.Entity<SubscriptionType>()
                .Property(x => x.Price)
                .HasPrecision(8, 2);//toplam sayının uzunluğu 8 basamak 2 si virgülden sonra olmalı.decimal tanımlanan propertyler için belirtilmesi gerekmektedir.Data Annotation ile yapılamıyor FluentApi ile yapılabiliyor ancak.

            builder.Entity<Subscription>()
                .Property(x => x.Amount)
                .HasPrecision(8, 2);

            builder.Entity<Subscription>()
                .Property(x => x.PaidAmount)
                .HasPrecision(8, 2);

            //fluent api propert oluşturma
            //builder.Entity<SubscriptionType>()
            //    .Property(x => x.Name).IsRequired();
        }

        public DbSet<Deneme> Denemeler { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionType> SubscriptionTypes { get; set; }




    }
}
