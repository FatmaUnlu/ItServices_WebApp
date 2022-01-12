using AutoMapper;
using ItServiceApp.Models.Identity;
using ItServiceApp.ViewModels;
using System;

namespace ItServiceApp.MapperProfiles
{
    public class AccountProfile :Profile
    {
        public AccountProfile()
        {
            CreateMap<ApplicationUser, UserProfileViewModel>().ReverseMap(); //application userdan user profile cevirme
            //CreateMap<UserProfileViewModel, ApplicationUser>(); //reversemap kullandıgımız için gerek yok 
        }

    }
}
