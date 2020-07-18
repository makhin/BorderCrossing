using System.Diagnostics;
using BorderCrossing.Models;
using BorderCrossing.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BorderCrossing.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBorderCrossingService _borderCrossingService;

        public HomeController(ILogger<HomeController> logger, IBorderCrossingService borderCrossingService)
        {
            _logger = logger;
            _borderCrossingService = borderCrossingService;
        }

        public IActionResult Index()
        {
            return View(new LocationHistoryFilePostRequest());
        }
        
        [HttpPost]
        [DisableRequestSizeLimit] 
        [Consumes("multipart/form-data")]         
        public IActionResult LoadFile(LocationHistoryFilePostRequest model)
        {
            if (model.LocationHistoryFile != null)
            {
                var dateRangePostRequest = _borderCrossingService.PrepareLocationHistoryAsync(null);
                return View("DateRange", dateRangePostRequest);
            }
            return RedirectToAction("Index","Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        public IActionResult Results(DateRangePostRequest model)
        {
            var borderCrossingResponse = _borderCrossingService.ParseLocationHistoryAsync(model);
            return View("Results", borderCrossingResponse);
        }
    }
}