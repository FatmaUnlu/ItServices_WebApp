using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ItServiceApp.Areas.Admin.Controllers
{
    [Authorize(Roles ="Admin")] //rol tabanlı authotization
    public class AdminBaseController : Controller
    {

    }
}
