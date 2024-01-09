using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OwnerSpecReport.Models;
using System;
using System.Net;
using System.Linq;

namespace OwnerSpecReport.Controllers
{
    [Route("api/ChargeSetupSvc")]
    [ApiController]
    public class ChargeSetupSvcController : ControllerBase
    {
        private readonly ILogger<OTRSvcController> _logger;
        private readonly BNReportDbContext _BNReportDb;
        public ChargeSetupSvcController(ILogger<OTRSvcController> logger, BNReportDbContext BNReportDb)
        {
            _logger = logger;
            _BNReportDb = BNReportDb;            
        }

        public IActionResult Get()
        {
            try
            {
                // var chargeList = _BNReportDb.WAL_RealMarine_Billings.OrderBy(x=>x.Vessel_Code).ToList();

                var chargeList = (from v in _BNReportDb.WAL_Vessels join c in _BNReportDb.WAL_RealMarine_Billings on v.Vessel_Code equals c.Vessel_Code
                                  orderby v.Vessel_Name
                                 select new
                                 {
                                   c.ID,  c.Vessel_Code, v.Vessel_Name, c.Charge_Account_Code, c.Amount, c.Created_Date, c.Created_By, c.Updated_By, c.Updated_Date
                                 }
                                 ).ToList();
                return new JsonResult(chargeList);                

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                return new JsonResult("Error");
            }
        }

        [HttpPost]
        [Route("postCharge")]
        public IActionResult PostCharge(RealMarine_Charge_VM charge)
        {
            try
            {
                var r = new WAL_RealMarine_Billing();
                r.Vessel_Code = charge.Vessel_Code;
                r.Amount = charge.Amount;
                r.Charge_Account_Code = charge.Charge_Account_Code;
                r.Created_By = charge.Created_By;
                r.Updated_By = charge.Updated_By;
                r.Created_Date = DateTime.Now;
                r.Updated_Date = DateTime.Now;
                _BNReportDb.WAL_RealMarine_Billings.Add(r);
                _BNReportDb.SaveChanges();

                return new OkObjectResult(charge);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                return new JsonResult("Error");
            }
        }

        [HttpPut]
        [Route("updateCharge")]
        public IActionResult UpdateCharge(RealMarine_Charge_VM charge)
        {
            try
            {
                var r = _BNReportDb.WAL_RealMarine_Billings.Find(charge.ID);
                if (r != null)
                {
                    r.Amount = charge.Amount;
                    r.Charge_Account_Code = charge.Charge_Account_Code;
                    r.Updated_By = charge.Updated_By;
                    r.Updated_Date = DateTime.Now;
                    _BNReportDb.SaveChanges();
                    return new OkObjectResult(charge);
                }
                else
                {
                    return new NotFoundObjectResult(charge);
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                return new JsonResult("Error");
            }
        }

        [HttpDelete]
        [Route("deleteCharge")]
        public IActionResult DeleteCharge( RealMarine_Charge_VM charge)
        {
            try
            {
                var r = _BNReportDb.WAL_RealMarine_Billings.Find(charge.ID);
                _BNReportDb.WAL_RealMarine_Billings.Remove(r);
                _BNReportDb.SaveChanges();
                return new OkObjectResult(charge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                return new JsonResult("Error");
            }
        }

        [Route("getVesselList")]
        public IActionResult GetVesselList()
        {
            try
            {
                var vesselList = _BNReportDb.WAL_Vessels.OrderBy(x=>x.Vessel_Code).ToList();                
                return new JsonResult(vesselList);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                return new JsonResult("Error");
            }
        }

        [Route("checkDuplication")]
        [HttpGet]
        public JsonResult CheckDuplication(string vesselCode)
        {
            bool isValid = false;
            var aoc = _BNReportDb.WAL_RealMarine_Billings.Where(a => a.Vessel_Code == vesselCode ).Select(a => a).ToList();
            if (aoc.Count() == 0)
            {
                isValid = true;
            }
            return new JsonResult(isValid);
        }

    }
}
