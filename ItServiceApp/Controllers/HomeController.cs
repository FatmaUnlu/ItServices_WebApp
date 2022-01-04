using ItServiceApp.InjectOrnek;
using Microsoft.AspNetCore.Mvc;

namespace ItServiceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMyDependency _myDependency;

        public HomeController(IMyDependency MyDependency)
        {
            _myDependency = MyDependency;
        }

        public IActionResult Index()
        {
            _myDependency.Log("Home/Index e girildi");
            return View();
        }
    }
}
