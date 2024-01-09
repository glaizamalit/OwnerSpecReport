using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OwnerSpecReport.Models;

namespace OwnerSpecReport.Controllers
{
    public class ChargeSetupController : Controller
    {
        private readonly ILogger<ChargeSetupController> _logger;
        private readonly BNReportDbContext _BNReportDb;

        public ChargeSetupController( ILogger<ChargeSetupController> logger, BNReportDbContext BNReportDb)
        {
            _logger = logger;
            _BNReportDb = BNReportDb;           
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
