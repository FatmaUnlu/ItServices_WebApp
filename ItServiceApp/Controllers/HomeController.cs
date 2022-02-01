using AutoMapper;
using ItServiceApp.Data;
using ItServiceApp.InjectOrnek;
using ItServiceApp.Models.Entities;
using ItServiceApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ItServiceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMyDependency _myDependency;
        private readonly MyContext _dbContext;
        private readonly IMapper _mapper;

        public HomeController(IMyDependency MyDependency, MyContext dbContext, IMapper mapper)
        {
            _myDependency = MyDependency;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            _myDependency.Log("Home/Index e girildi");

            //dbden verileri çekip list olarak mapper yapma
            //var data = _dbContext.SubscriptionTypes
            //    .ToList()
            //    .Select(x => _mapper.Map<SubscriptionTypeViewModel>(x))
            //    .OrderBy(x => x.Price)
            //    .ToList();

            //return View(data);
            return View();
        }
    }
}
