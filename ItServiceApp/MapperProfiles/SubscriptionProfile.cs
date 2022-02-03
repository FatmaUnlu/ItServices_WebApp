using AutoMapper;
using ItServiceApp.Models.Entities;
using ItServiceApp.ViewModels;

namespace ItServiceApp.MapperProfiles
{
    public class SubscriptionProfile :Profile
    {
        public SubscriptionProfile()
        {
            CreateMap<SubscriptionType, SubscriptionTypeViewModel>().ReverseMap();
            CreateMap<Address, AddressViewModel>().ReverseMap();

        }
    }
}
