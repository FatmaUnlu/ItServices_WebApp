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
        public DbSet<Deneme> Denemeler { get; set; }
    }
}
