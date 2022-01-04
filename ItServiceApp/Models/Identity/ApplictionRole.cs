using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ItServiceApp.Models.Identity
{
    public class ApplictionRole:IdentityRole //Identity den kalıtım aldığımız için direkt oluşur tablolar.
    {
        public ApplictionRole()
        {

        }
        public ApplictionRole(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
        [StringLength(100)]
        public string Description { get;  set; }
    }
}
