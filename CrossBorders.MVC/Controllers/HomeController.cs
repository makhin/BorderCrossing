using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CrossBorders.MVC.Models;
using CrossBorders.MVC.Services;

namespace CrossBorders.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICrossService _crossService;

        public HomeController(ILogger<HomeController> logger, ICrossService crossService)
        {
            _logger = logger;
            _crossService = crossService;
        }

        public IActionResult Index()
        {
            return View(new CreatePost());
        }
        
        [HttpPost]
        [DisableRequestSizeLimit] 
        [Consumes("multipart/form-data")]         
        public IActionResult Create(CreatePost model)
        {
            if (model.LocationHistory != null)
            {
                var historyStat = _crossService.HistoryProcess(model.LocationHistory);
                return View("Calculate", historyStat);
            }
            return RedirectToAction("Index","Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        [HttpPost]
        public IActionResult Calculate(CalcPost model)
        {
            var result = _crossService.FindCrosses(model);
            return View("Result", result);
        }
    }
}