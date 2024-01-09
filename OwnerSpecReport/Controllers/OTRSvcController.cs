using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using OwnerSpecReport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace OwnerSpecReport.Controllers
{
    [Route("api/otrsvc")]
    [ApiController]
    public class OTRSvcController : ControllerBase
    {
        private readonly ILogger<OTRSvcController> _logger;
        private readonly WallemRptModelDbContext _WallemRptDb;
        private readonly BNReportDbContext _BNReportDb;
        public OTRSvcController(ILogger<OTRSvcController> logger, BNReportDbContext BNReportDb, WallemRptModelDbContext WallemRptDb)
        {
            _logger = logger;
            _BNReportDb = BNReportDb;
            _WallemRptDb = WallemRptDb;
        }

        public IActionResult Get()
        {
            try
            {
                Response.StatusCode = Convert.ToInt32(HttpStatusCode.OK);
                return new OkObjectResult("");

                //Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                //return new JsonResult("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                return new JsonResult("");
            }
        }

        [Route("getReportList")]
        public IActionResult GetReportList(string ReportType)
        {
            try
            {
                var reportList = new List<SelectListItem>();

                if (ReportType == "NYK Excel")
                {
                    reportList.Add(new SelectListItem { Text = "AC00101", Value = "AC00101" });
                    reportList.Add(new SelectListItem { Text = "M00101", Value = "M00101" });
                    reportList.Add(new SelectListItem { Text = "PR00101", Value = "PR00101" });
                    reportList.Add(new SelectListItem { Text = "T00101", Value = "T00101" });
                    reportList.Add(new SelectListItem { Text = "T00201", Value = "T00201" });
                    reportList.Add(new SelectListItem { Text = "T00301", Value = "T00301" });
                    reportList.Add(new SelectListItem { Text = "T00401", Value = "T00401" });
                }
                else if (ReportType == "Interocean Excel")
                {
                    reportList.Add(new SelectListItem { Text = "Fleet Cost Analysis", Value = "FCA" });
                }
                else
                {
                    // reportList.Add(new SelectListItem { Text = "Genco", Value = "Genco" });
                    reportList.Add(new SelectListItem { Text = "Cost Analysis and Prognois", Value = "CAP" });
                }
                return new JsonResult(reportList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                return new JsonResult("Error");                
            }
        }


        [Route("getVesselList")]
        public IActionResult GetVesselList(string VesselGroup)
        {
            try
            {
                var vesselList = _BNReportDb.WAL_Vessels.Where(x => x.Vessel_Group == VesselGroup).Select(x=> new { Text = x.Vessel_Name, Value = x.Vessel_Code});                
                return new JsonResult(vesselList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                return new JsonResult("Error");
            }
        }

        //[Route("getManualAccrualList")]
        //public JsonResult GetManualAccrualList()
        //{
        //    try
        //    {
        //        var lst = _BNReportDb.WAL_MANUALACCRs.Select(x => new { Text = x.Vessel_Code, Value = x.Voucher_Number });
        //        return new JsonResult(lst);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.ToString());
        //        Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
        //        return new JsonResult("Error");
        //    }
        //}
    }
}