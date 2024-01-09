using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OwnerSpecReport.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Linq;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Collections.Generic;
using OwnerSpecReport.CustomCode;

namespace OwnerSpecReport.Controllers
{
    public class RMInvoiceController : Controller
    {
        private readonly ILogger<RMInvoiceController> _logger;
        private readonly BNReportDbContext _BNReportDb;
        private readonly BIWallemDbContext _BIWallemDb;
        private IHttpContextAccessor _httpContextAccessor;
        private IWebHostEnvironment _env;
        private IGenInvoice _geninvoice;

        public RMInvoiceController( ILogger<RMInvoiceController> logger, BNReportDbContext BNReportDb, BIWallemDbContext BIWallemDb, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env, IGenInvoice genInvoice)
        {
            _logger = logger;
            _BNReportDb = BNReportDb;
            _BIWallemDb = BIWallemDb;
            _httpContextAccessor = httpContextAccessor;
            _env = env;
            _geninvoice = genInvoice;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult getPOList(string SMC)
        {
            string userID = _httpContextAccessor.HttpContext.Session.GetString("tkusername");
            try
            {
                var list = new List<RealMarine_Auto>();

                if (SMC == "NYK") {
                    list = _BIWallemDb.RealMarine_Autos.FromSqlRaw("exec sp_RealMarine_Billing_NYK {0}", userID).ToList();
                }
                else
                {
                    list = _BIWallemDb.RealMarine_Autos.FromSqlRaw("exec sp_RealMarine_Billing {0}, {1}", SMC, userID).ToList();
                }
                return new JsonResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                return new JsonResult("Error");
            }
        }

        public IActionResult getSMCList()
        {
            try
            {
                var list1 = _BNReportDb.WAL_Vessels.Select(x=> new { Text = x.SMC, Value = x.SMC }).Distinct().ToList();
                var list2 = "NYK".Select(x => new { Text = "NYK", Value = "NYK" }).ToList();
                var list = list2.Union(list1);

                return new JsonResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                return new JsonResult("Error");
            }
        }

        public IActionResult genJournal(string SMC)
        {
            string userID = _httpContextAccessor.HttpContext.Session.GetString("tkusername").ToUpper();
            try
            {
                string path = _env.WebRootPath + "/Download/" + userID;

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                else
                {
                    // delete all files in the folder
                    string[] fileList = Directory.GetFiles(path, "*.*");
                    foreach (string file in fileList)
                    {
                        System.IO.File.Delete(file);
                    }
                }
                string strFileName = path + "/" + "Journal_" + SMC + ".xlsx";
                var list = _BIWallemDb.RealMarine_Journals.FromSqlRaw("exec sp_RealMarine_Journal {0}, {1}", SMC, userID).ToList();
                var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("AX Journal");
                ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                ws.PageSetup.FitToPages(1, 0);                

                ws.Cell(1, 1).Value = "Created Date";
                ws.Cell(1, 2).Value = "Account Type";
                ws.Cell(1, 3).Value = "Account";
                ws.Cell(1, 4).Value = "Description";
                ws.Cell(1, 5).Value = "Currency";
                ws.Cell(1, 6).Value = "Rate Type";
                ws.Cell(1, 7).Value = "Exch Rate";
                ws.Cell(1, 8).Value = "Debit";
                ws.Cell(1, 9).Value = "Credit";
                ws.Cell(1, 10).Value = "DIM3";
                ws.Cell(1, 11).Value = "VC Name";
                ws.Cell(1, 12).Value = "Offset Type";
                ws.Cell(1, 13).Value = "Offset Account";

                for (int i = 1; i <= 13; i++)
                {
                    ws.Cell(1, i).Style.Font.Bold = true;
                }

                int intRow = 2;
                foreach (var Litem in list)
                {

                    ws.Cell(intRow, 1).Value = Litem.Created_Date;
                    ws.Cell(intRow, 2).Value = Litem.Account_Type;
                    ws.Cell(intRow, 3).Value = Litem.Account;
                    ws.Cell(intRow, 4).Value = Litem.Description;
                    ws.Cell(intRow, 5).Value = Litem.Currency;
                    ws.Cell(intRow, 6).Value = Litem.Rate_Type;
                    ws.Cell(intRow, 7).Value = Litem.Exch_Rate;
                    ws.Cell(intRow, 8).Value = Litem.Debit;
                    ws.Cell(intRow, 9).Value = Litem.Credit;
                    ws.Cell(intRow, 10).Value = Litem.DIM3;
                    ws.Cell(intRow, 11).Value = Litem.VC_Name;
                    ws.Cell(intRow, 12).Value = Litem.Offset_Type;
                    ws.Cell(intRow, 13).Value = Litem.Offset_Account;

                    intRow++;
                }
                ws.Columns().AdjustToContents();
                wb.SaveAs(strFileName);
                string strBaseFolder = strFileName.Substring(_env.WebRootPath.Length);
                var strBaseURL = Request.Scheme + "://" + Request.Host + Request.PathBase;
                string strFileURL = strBaseURL + strBaseFolder;
                _logger.LogInformation("Generated file - " + strFileURL);
                return Json(new { success = true, fileUrl = strFileURL });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                return new JsonResult("Error");
            }            
        }

        public IActionResult genEyeshare(string SMC)
        {
            string userID = _httpContextAccessor.HttpContext.Session.GetString("tkusername").ToUpper();
            try
            {
                string path = _env.WebRootPath + "/Download/" + userID;

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                else
                {
                    // delete all files in the folder
                    string[] fileList = Directory.GetFiles(path, "*.*");
                    foreach (string file in fileList)
                    {
                        System.IO.File.Delete(file);
                    }
                }
                string strFileName = path + "/" + "Eyeshare_Summary_" + SMC + ".xlsx";
                var list = _BIWallemDb.RealMarine_Eyeshares.FromSqlRaw("exec sp_RealMarine_Eyeshare {0}, {1}", SMC, userID).ToList();
                var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Eyeshare Summary");
                ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                ws.PageSetup.FitToPages(1, 0);

                ws.Cell(1, 1).Value = "Vessel";
                ws.Cell(1, 2).Value = "Invoice Ref";
                ws.Cell(1, 3).Value = "Account";
                ws.Cell(1, 4).Value = "Sum of Amount";
                
                for (int i = 1; i <= 4; i++)
                {
                    ws.Cell(1, i).Style.Font.Bold = true;
                }

                int intRow = 2;
                foreach (var Litem in list)
                {

                    ws.Cell(intRow, 1).Value = Litem.Vessel;
                    ws.Cell(intRow, 2).Value = Litem.Invoice_Ref;
                    ws.Cell(intRow, 3).Value = Litem.Account;
                    ws.Cell(intRow, 4).Value = Litem.Sum_of_Amount;
                    
                    intRow++;
                }
                ws.Columns().AdjustToContents();
                wb.SaveAs(strFileName);
                string strBaseFolder = strFileName.Substring(_env.WebRootPath.Length);
                var strBaseURL = Request.Scheme + "://" + Request.Host + Request.PathBase;
                string strFileURL = strBaseURL + strBaseFolder;
                _logger.LogInformation("Generated file - " + strFileURL);
                return Json(new { success = true, fileUrl = strFileURL });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                return new JsonResult("Error");
            }
        }

        public IActionResult genInvoice(string SMC)
        {
            try
            {
                 var dtLastMonth = DateTime.Now.AddMonths(-1);
                 string strYear = dtLastMonth.Year.ToString();
                 string strMonth = dtLastMonth.Month.ToString();
                _geninvoice.SetPeriod(strYear, strMonth);
                _geninvoice.SetSMC(SMC);
                 bool boolSuccess = _geninvoice.Run();

                if (boolSuccess == true)
                {
                    return new OkObjectResult("");
                }
                else
                {
                    Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                    return new JsonResult("Fail to generate invoice");
                }
            }catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                Response.StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError);
                return new JsonResult("Fail to generate invoice");
            }
        }

        public IActionResult finalize()
        {
            try
            {
                _BIWallemDb.Database.ExecuteSqlRaw("exec sp_Realmarine_Finalize");   
                return new OkObjectResult("");
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
