using Microsoft.AspNetCore.Mvc;

namespace PGS.Azure.ServiceFabric.VotingWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
