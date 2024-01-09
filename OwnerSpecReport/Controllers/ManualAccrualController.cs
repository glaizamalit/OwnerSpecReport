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
    public class ManualAccrualController : Controller
    {
        private readonly ILogger<ManualAccrualController> _logger;
        private readonly WallemRptModelDbContext _WallemRptDb;
        private readonly BNReportDbContext _BNReportDb;

        public ManualAccrualController(ILogger<ManualAccrualController> logger, BNReportDbContext BNReportDb, WallemRptModelDbContext WallemRptDb)
        {
            _logger = logger;
            _BNReportDb = BNReportDb;
            _WallemRptDb = WallemRptDb;
        }
        public IActionResult Index()
        {
            //checking of users - unauthorized



            return View();
        }

        public JsonResult GetList()
        {
            try
            {
                var lst = _BNReportDb.WAL_MANUALACCRs.Select(x => new ManualAccrualClass
                {
                    ID = x.ID,
                    Vessel_Name = _BNReportDb.WAL_Vessels.Where(y => y.Vessel_Code == x.Vessel_Code).Select(y => y.Vessel_Name).FirstOrDefault(),
                    Vessel_Code = x.Vessel_Code,
                    Calendar_Month = x.Calendar_Month.ToString(),
                    PO_Number = x.PO_Number,
                    Voucher_Number = x.Voucher_Number,
                    CreatedBy = x.CreatedBy,
                    CreatedDt = x.CreatedDt,
                    UpdatedBy = x.UpdatedBy,
                    UpdatedDt = x.UpdatedDt
                }).OrderByDescending(x=>x.Calendar_Month);

                return new JsonResult(lst);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Exception:" + ex.Message + ":::" + ex.InnerException);
                throw;
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult Add(ManualAccrualClass accrual)
        {
            try
            {
                WAL_MANUALACCR acc = new WAL_MANUALACCR();
                acc.Vessel_Code = accrual.Vessel_Code;
                acc.PO_Number = accrual.PO_Number;
                acc.Voucher_Number = accrual.Voucher_Number;
                acc.Calendar_Month = DateTime.Parse(accrual.Calendar_MonthString);
                acc.CreatedBy = accrual.CreatedBy;
                acc.CreatedDt = DateTime.Now;
                acc.UpdatedBy = null;
                acc.UpdatedDt = null;

                _BNReportDb.Add(acc);
                _BNReportDb.SaveChanges();

                _logger.LogInformation("Manual Accrual Id " + acc.ID + " was successfully added to the database.");

                return new JsonResult(acc);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Exception:" + ex.Message + ":::" + ex.InnerException);
                throw;
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult Delete(ManualAccrualClass accrual)
        {
            try
            {
                WAL_MANUALACCR acc = new WAL_MANUALACCR();
                acc = _BNReportDb.WAL_MANUALACCRs.Where(x => x.ID == accrual.ID).Select(x => x).FirstOrDefault();
                _BNReportDb.Remove(acc);
                _BNReportDb.SaveChanges();

                _logger.LogInformation("Manual Accrual Id " + acc.ID + " was successfully deleted to the database.");

                return new JsonResult(acc);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Exception:" + ex.Message + ":::" + ex.InnerException);
                throw;
            }
        }


        public JsonResult CheckDuplication(string vesselCode, string calendarMonth, string voucherNumber, string pONumber)
        {
            bool isValid = false;
            DateTime calMonth = DateTime.Parse(calendarMonth + "-01");
            var aoc = _BNReportDb.WAL_MANUALACCRs.Where(a => a.Vessel_Code == vesselCode && a.Calendar_Month == calMonth && a.Voucher_Number == voucherNumber && a.PO_Number == pONumber).Select(a => a).ToList();
            if (aoc.Count() == 0)
            {
                isValid = true;
            }

            return new JsonResult(isValid);
        }


        public JsonResult GetVesselList()
        {
            try
            {
                var vesselList = _BNReportDb.WAL_Vessels.Select(x => new { Text = x.Vessel_Name, Value = x.Vessel_Code }).OrderBy(x=>x.Text);
                return new JsonResult(vesselList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                return new JsonResult("Error");
            }
        }
    }
}
