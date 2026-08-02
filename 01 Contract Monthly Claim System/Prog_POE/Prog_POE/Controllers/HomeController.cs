using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Prog_POE.Models;

namespace Prog_POE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About() => View();
        public IActionResult Available() => View();
        public IActionResult Contact() => View();
        public IActionResult FindUs() => View();
        public IActionResult Apply() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
