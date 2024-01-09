using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OwnerSpecReport.Models;
using System;
using Microsoft.EntityFrameworkCore;

namespace OwnerSpecReport.Controllers
{
    public class FundingDataController : Controller
    {
        private readonly ILogger<FundingDataController> _logger;
        private readonly WallemProDbContext dbWallemPro;
        
        public FundingDataController(ILogger<FundingDataController> logger, WallemProDbContext WallemProDb)
        {
            _logger = logger;
            dbWallemPro = WallemProDb;
        }

        public IActionResult Index()
        {
            var f = new FormModel();
            f.Year = DateTime.Today.Year;
            f.Month = DateTime.Today.Month;

            return View(f);
        }

        [HttpPost]
        public ActionResult RefreshAJAX([FromBody] FormModel f)
        {
            var reportDate = new DateTime(f.Year, f.Month, 1);
            dbWallemPro.Database.ExecuteSqlRaw("exec sp_WAL_FundingRequestTrans {0}", reportDate);
            _logger.LogInformation("Funding data refreshed for month - " + reportDate.ToString("yyyy-MM-dd"));
            return Json(new { success = true});
        }
    }
}
