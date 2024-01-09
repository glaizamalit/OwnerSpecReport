using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using OwnerSpecReport.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace OwnerSpecReport.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WallemRptModelDbContext dbWallemRpt;
        private readonly BNReportDbContext dbBNReport;
        private IWebHostEnvironment _env;
        private static List<SelectListItem> vgSelectItemList;

        public HomeController(ILogger<HomeController> logger, BNReportDbContext BNReportDb, WallemRptModelDbContext WallemRptDb, IWebHostEnvironment env)
        {
            _logger = logger;
            dbBNReport = BNReportDb;
            dbWallemRpt = WallemRptDb;
            _env = env;
        }

        [TypeFilter(typeof(SsoAuth))]
        public IActionResult Index()
        {
            var f = new FormModel();
            f.Year = DateTime.Today.Year;
            f.Month = DateTime.Today.Month;        
                        
            var vesselGrouplist = dbBNReport.WAL_Vessels.OrderBy(x => x.Vessel_Group).Select(x => new { Value = x.Vessel_Group, Text = x.Vessel_Group }).Distinct();

            vgSelectItemList = new List<SelectListItem>();

            foreach(var vg in vesselGrouplist)
            {
                var s = new SelectListItem();
                s.Value = vg.Value;
                s.Text = vg.Text;
                vgSelectItemList.Add(s);
            }

            f.VesselGroupList = vgSelectItemList;

            // ViewBag.UserName = "Alex Kwan";
            // HttpContext.Session.SetString("UserName", "Alex Kwan");

            // Session["vesselGroupList"] = vesselGrouplist;                                   
            /*
            // get user name
            if (System.Web.HttpContext.Current.Session["tkusername"] != null)
            {
                if (Session["UserName"] == null)
                {
                    Session["UserName"] = getUserName();

                }
                else if (Session["UserName"].ToString() == "Unknown")
                {

                    Session["UserName"] = getUserName();
                }
            }
            else
            {
                Session["UserName"] = "Unknown";
            } 
            */

            return View(f);
        }

        [HttpPost]
        public ActionResult Index(FormModel f)
        {
            if (ModelState.IsValid)
            {

                /*
                string fullPathFileName = GenerateReport(f);

                // ControllerContext.HttpContext.Response.Cookies.Set(new HttpCookie("downloadStatus", "Done"));

                byte[] fileBytes = System.IO.File.ReadAllBytes(fullPathFileName);
                string filename = Path.GetFileName(fullPathFileName);
                return File(fileBytes, "application/octet-stream", filename);
                */

                ViewBag.IsValidModel = "true";
            }
            else
            {
                
                ViewBag.IsValidModel = "false";
            }
            f.VesselGroupList = vgSelectItemList;

            return View(f);
        }

        /*
        [HttpPost]
        public ActionResult AJAXTest([FromBody] UserInfo jsonData)
        {
            var fullname = jsonData.FullName;
            string strFileURL = "";
            return Json(new { success = true, fileUrl = strFileURL });
        }
        */

        [HttpPost]
        public ActionResult GenerateAJAX([FromBody] FormModel f)
        {
            string fullPathFileName = GenerateReport(f);

            // string strOutputfilename = Path.GetFileName(fullPathFileName);
            string strBaseFolder = fullPathFileName.Substring( _env.WebRootPath.Length);
            var strBaseURL = Request.Scheme + "://" + Request.Host + Request.PathBase;            
            string strFileURL = strBaseURL + strBaseFolder;
            _logger.LogInformation("Generated file - " + strFileURL);
            return Json(new { success = true, fileUrl = strFileURL });
        }

        private string GenerateReport(FormModel f)
        {
            string filename = ""; // physicall file name
            string strOutputfilename = "";
            string UserID = "";
            string strReturnFileName = "";

            /*
            if (System.Web.HttpContext.Current.Session["tkusername"] != null)
            {
                UserID = System.Web.HttpContext.Current.Session["tkusername"].ToString().ToUpper(); ;
            }
            if (UserID == "")
            {
                RedirectToAction("Index", "Unauthorized");
            }
            */

            UserID = HttpContext.Session.GetString("tkusername").ToString().ToUpper();           

            string path = _env.WebRootPath + "/Download/" + UserID;

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

            if (f.ReportType == "NYK Excel")
            {

                foreach (var r in f.ReportList)
                {
                    foreach (var v in f.VesselList)
                    {
                        if (r == "AC00101")
                        {
                            string strBaseFileName = "NYK-AC00101";
                            strOutputfilename = f.Year + f.Month.ToString().PadLeft(2, '0') + "_" + v + "_" + strBaseFileName + ".xlsx";
                            filename = path + "/" + strOutputfilename;
                            create_NYK_AC00101(filename, v, f.Year, f.Month);
                        }
                        else if (r == "PR00101")
                        {
                            string strBaseFileName = "NYK-PR00101";
                            strOutputfilename = f.Year + f.Month.ToString().PadLeft(2, '0') + "_" + v + "_" + strBaseFileName + ".xlsx";
                            filename = path + "/" + strOutputfilename;
                            create_NYK_PR00101(filename, v, f.Year, f.Month);
                        }
                        else if (r == "M00101")
                        {
                            string strBaseFileName = "NYK-M00101";
                            strOutputfilename = f.Year + f.Month.ToString().PadLeft(2, '0') + "_" + v + "_" + strBaseFileName + ".xlsx";
                            filename = path + "/" + strOutputfilename;
                            create_NYK_M00101(filename, v, f.Year, f.Month);
                        }
                        else if (r == "T00101")
                        {
                            string strBaseFileName = "NYK-T00101";
                            strOutputfilename = f.Year + f.Month.ToString().PadLeft(2, '0') + "_" + v + "_" + strBaseFileName + ".xlsx";
                            filename = path + "/" + strOutputfilename;
                            create_NYK_T00101(filename, v, f.Year, f.Month);
                        }
                        else if (r == "T00201")
                        {
                            string strBaseFileName = "NYK-T00201";
                            strOutputfilename = f.Year + f.Month.ToString().PadLeft(2, '0') + "_" + v + "_" + strBaseFileName + ".xlsx";
                            filename = path + "/" + strOutputfilename;
                            create_NYK_T00201(filename, v, f.Year, f.Month);
                        }
                        else if (r == "T00301")
                        {
                            string strBaseFileName = "NYK-T00301";
                            strOutputfilename = f.Year + f.Month.ToString().PadLeft(2, '0') + "_" + v + "_" + strBaseFileName + ".xlsx";
                            filename = path + "/" + strOutputfilename;
                            create_NYK_T00301(filename, v, f.Year, f.Month);
                        }
                        else if (r == "T00401")
                        {
                            string strBaseFileName = "NYK-T00401";
                            strOutputfilename = f.Year + f.Month.ToString().PadLeft(2, '0') + "_" + v + "_" + strBaseFileName + ".xlsx";
                            filename = path + "/" + strOutputfilename;
                            create_NYK_T00401(filename, v, f.Year, f.Month);
                        }
                    }
                }
                string zipFileName = _env.WebRootPath + "/Download/" + UserID + ".zip";
                System.IO.File.Delete(zipFileName);
                ZipFile.CreateFromDirectory(path, zipFileName);
                strReturnFileName = zipFileName;
            }
            else if (f.ReportType == "AL Seer Excel")
            {
                    if (f.ReportList.FirstOrDefault() == "CAP")
                    {
                        string strBaseFileName = "AL Seer Cost Analysis and Prognosis";
                        strOutputfilename = strBaseFileName + "-" + f.Year + f.Month.ToString().PadLeft(2, '0') + ".xlsx";
                        filename = path + "/" + strOutputfilename;
                        create_AL_Seer_CAP(filename, f.VesselList, f.Year, f.Month);
                        strReturnFileName= filename;
                    }
            }else if (f.ReportType == "Interocean Excel")
            {
                if (f.ReportList.FirstOrDefault() == "FCA")
                {
                    string strBaseFileName = "Interocean Fleet Cost Analysis";
                    strOutputfilename = strBaseFileName + "-" + f.Year + f.Month.ToString().PadLeft(2, '0') + ".xlsx";
                    filename = path + "/" + strOutputfilename;
                    create_Interocean_FCA(filename, f.VesselList, f.Year, f.Month);
                    strReturnFileName = filename;
                }
            }
            return (strReturnFileName);
        }

        private void create_NYK_AC00101(string filename, string VslCode, int Year, int Month)
        {
            string ExcelTemplate = _env.WebRootPath + "/App_Data/NYK-AC00101.xlsx";
            System.IO.File.Copy(ExcelTemplate, filename, true);

            XLWorkbook wb = new XLWorkbook(filename);
            var ws = wb.Worksheet(1);
            var vslList = dbBNReport.WAL_Vessels.Where(x => x.Vessel_Code == VslCode).FirstOrDefault();
            ws.Cell(3, 3).Value = vslList.Vessel_Name;
            ws.Cell(4, 3).Value = vslList.Owner_Name;
            ws.Cell(3, 9).Value = vslList.Owner_Vessel_Code;
            ws.Cell(4, 9).Value = vslList.Owner_Owner_Code;
            ws.Cell(3, 12).Value = Year;
            ws.Cell(4, 12).Value = DateTime.Today.ToString("yyyy/MM/dd");

            DateTime calendarMonth = new DateTime(Year, Month, 1);
            var acList = dbWallemRpt.TmplAccountCells.Where(x => x.TmplFileName == "AC00101").ToList();
            foreach (var ac in acList)
            {
                var asList = dbBNReport.WAL_ActualSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).Select(x => new { x.Account_Code, x.YTD_Actual }).FirstOrDefault();
                var bsList = dbBNReport.WAL_BudgetSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).Select(x => new { x.Account_Code, x.FY_Budget }).FirstOrDefault();

                ws.Cell(ac.RowNo, 5).Value = bsList == null ? 0 : bsList.FY_Budget;
                ws.Cell(ac.RowNo, 6).Value = asList == null ? 0 : asList.YTD_Actual;

            }
            wb.Save();
        }

        private void create_NYK_PR00101(string filename, string VslCode, int Year, int Month)
        {
            string ExcelTemplate = _env.WebRootPath + "/App_Data/NYK-PR00101.xlsx";
            System.IO.File.Copy(ExcelTemplate, filename, true);

            XLWorkbook wb = new XLWorkbook(filename);
            var ws = wb.Worksheet(1);
            var vslList = dbBNReport.WAL_Vessels.Where(x => x.Vessel_Code == VslCode).FirstOrDefault();
            ws.Cell(3, 3).Value = vslList.Vessel_Name;
            ws.Cell(4, 3).Value = vslList.Owner_Name;
            ws.Cell(3, 9).Value = vslList.Owner_Vessel_Code;
            ws.Cell(4, 9).Value = vslList.Owner_Owner_Code;
            ws.Cell(3, 12).Value = Year;
            ws.Cell(4, 12).Value = DateTime.Today.ToString("yyyy/MM/dd");

            DateTime calendarMonth = new DateTime(Year, Month, 1);
            var acList = dbWallemRpt.TmplAccountCells.Where(x => x.TmplFileName == "PR00101").ToList();
            foreach (var ac in acList)
            {
                var asList = dbBNReport.WAL_ActualSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).Select(x => new { x.Account_Code, x.YTD_Actual }).FirstOrDefault();
                var bsList = dbBNReport.WAL_BudgetSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).Select(x => new { x.Account_Code, x.FY_Budget }).FirstOrDefault();

                ws.Cell(ac.RowNo, 5).Value = bsList == null ? 0 : bsList.FY_Budget;
                ws.Cell(ac.RowNo, 6).Value = asList == null ? 0 : asList.YTD_Actual;

            }
            wb.Save();
        }

        private void create_NYK_M00101(string filename, string VslCode, int Year, int Month)
        {
            
            string ExcelTemplate = _env.WebRootPath + "/App_Data/NYK-M00101.xlsx";
            System.IO.File.Copy(ExcelTemplate, filename, true);

            XLWorkbook wb = new XLWorkbook(filename);
            var ws = wb.Worksheet(1);
            var vslList = dbBNReport.WAL_Vessels.Where(x => x.Vessel_Code == VslCode).FirstOrDefault();
            string strVesselName = vslList.Vessel_Name + " (" + vslList.Vessel_Code + ")";
            ws.Cell(3, 3).Value = strVesselName;            
            ws.Cell(4, 3).Value = vslList.Owner_Name;
            ws.Cell(3, 8).Value = vslList.Owner_Vessel_Code;
            ws.Cell(4, 8).Value = vslList.Owner_Owner_Code;
            ws.Cell(6, 8).Value = 0; //  PAYMENT IN (US$:0, JPN:1) 
            ws.Cell(2, 11).Value = Year;
            // ws.Cell(3, 11).Value = Month.ToString("00");
            ws.Cell(3, 11).FormulaA1 = "=text("+ Month + ",\"00\")";
            ws.Cell(4, 11).Value = DateTime.Today.ToString("yyyy/MM/dd");

            ws.Cell(81, 3).Value = strVesselName;
            ws.Cell(81, 8).FormulaA1 = "=CONCATENATE(\"" + vslList.Owner_Vessel_Code + "\")";

            DateTime calendarMonth = new DateTime(Year, Month, 1);
            var acList = dbWallemRpt.TmplAccountCells.Where(x => x.TmplFileName == "M00101").ToList();
            foreach (var ac in acList)
            {
                var asList = dbBNReport.WAL_ActualSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).Select(x => new { x.Account_Code, x.YTD_Actual, x.CurMth_Actual_Excluding_Accrual, x.Opening_Balance, x.CurMth_Actual }).FirstOrDefault();

                decimal? ytd_actual = 0;
                decimal? curMth_Actual_Excluding_Accrual = 0;
                decimal? curMth_Actual = 0;
                decimal? opening_balance = 0;
                decimal? total = 0;
                if (asList != null)
                {
                    ytd_actual = asList.YTD_Actual == null ? 0 : asList.YTD_Actual;
                    curMth_Actual_Excluding_Accrual = asList.CurMth_Actual_Excluding_Accrual == null ? 0 : asList.CurMth_Actual_Excluding_Accrual;
                    opening_balance = asList.Opening_Balance == null ? 0 : asList.Opening_Balance;
                    curMth_Actual = asList.CurMth_Actual == null ? 0 : asList.CurMth_Actual;
                    // total = ytd_actual - curMth_Actual_Excluding_Accrual + opening_balance;
                    total = ytd_actual - curMth_Actual + opening_balance;
                }

                if (ac.RowNo >= 88)
                {
                    if (total >= 0)
                        ws.Cell(ac.RowNo, 7).Value = total;
                    else
                        ws.Cell(ac.RowNo, 8).Value = -total;
                }
                if (curMth_Actual_Excluding_Accrual >= 0)
                    ws.Cell(ac.RowNo, 9).Value = curMth_Actual_Excluding_Accrual;
                else
                    ws.Cell(ac.RowNo, 10).Value = -curMth_Actual_Excluding_Accrual;
            }
            wb.Save();
        }

        private void create_NYK_T00101(string filename, string VslCode, int Year, int Month)
        {

            string ExcelTemplate = _env.WebRootPath + "/App_Data/NYK-T00101.xlsx";
            System.IO.File.Copy(ExcelTemplate, filename, true);

            XLWorkbook wb = new XLWorkbook(filename);
            var ws = wb.Worksheet(1);
            var vslList = dbBNReport.WAL_Vessels.Where(x => x.Vessel_Code == VslCode).FirstOrDefault();
            string strVesselName = vslList.Vessel_Name + " (" + vslList.Vessel_Code + ")";
            ws.Cell(3, 3).Value = strVesselName;
            ws.Cell(4, 3).Value = vslList.Owner_Name;
            ws.Cell(3, 9).Value = vslList.Owner_Vessel_Code;
            ws.Cell(4, 9).Value = vslList.Owner_Owner_Code;
            ws.Cell(3, 12).FormulaA1 = "=text(" + Year + ",\"0000\")";
            ws.Cell(4, 12).Value = DateTime.Today.ToString("yyyy/MM/dd"); 

            DateTime calendarMonth = new DateTime(Year, Month, 1);
            var acList = dbWallemRpt.TmplAccountCells.Where(x => x.TmplFileName == "T00101").ToList();
            foreach (var ac in acList)
            {
                var asList = dbBNReport.WAL_ActualSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).FirstOrDefault();
                var budgetLine = dbBNReport.WAL_BudgetSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).FirstOrDefault();
                var actualByPeriodLine = dbBNReport.WAL_ActualByPeriodSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).FirstOrDefault();

                decimal? prior_year_accrual = 0;
                decimal? accrual_for_costanalysis = 0;
                decimal? fy_budget = 0;
                decimal? ytd_budget = 0;
                decimal? actual_excluding_accrual_01 = 0;
                decimal? actual_excluding_accrual_02 = 0;
                decimal? actual_excluding_accrual_03 = 0;
                if (asList != null)
                {
                    prior_year_accrual = asList.Prior_Year_Accrual;
                    accrual_for_costanalysis = asList.Accrual_for_CostAnalysis;
                }
                if(budgetLine != null)
                {
                    fy_budget = budgetLine.FY_Budget;
                    ytd_budget = budgetLine.YTD_Budget;
                }
                if(actualByPeriodLine != null)
                {
                    actual_excluding_accrual_01 = actualByPeriodLine.Actual_Excluding_Accrual_01;
                    actual_excluding_accrual_02 = actualByPeriodLine.Actual_Excluding_Accrual_02;
                    actual_excluding_accrual_03 = actualByPeriodLine.Actual_Excluding_Accrual_03;
                }

                ws.Cell(ac.RowNo, 5).Value = fy_budget;
                ws.Cell(ac.RowNo, 6).Value = ytd_budget;
                ws.Cell(ac.RowNo, 8).Value = prior_year_accrual;
                ws.Cell(ac.RowNo, 9).Value = actual_excluding_accrual_01;
                ws.Cell(ac.RowNo, 10).Value = actual_excluding_accrual_02;
                ws.Cell(ac.RowNo, 11).Value = actual_excluding_accrual_03;
                ws.Cell(ac.RowNo, 13).Value = accrual_for_costanalysis;
            }
            wb.Save();
        }

        private void create_NYK_T00201(string filename, string VslCode, int Year, int Month)
        {

            string ExcelTemplate = _env.WebRootPath + "/App_Data/NYK-T00201.xlsx";
            System.IO.File.Copy(ExcelTemplate, filename, true);

            XLWorkbook wb = new XLWorkbook(filename);
            var ws = wb.Worksheet(1);
            var vslList = dbBNReport.WAL_Vessels.Where(x => x.Vessel_Code == VslCode).FirstOrDefault();
            string strVesselName = vslList.Vessel_Name + " (" + vslList.Vessel_Code + ")";
            ws.Cell(3, 3).Value = strVesselName;
            ws.Cell(4, 3).Value = vslList.Owner_Name;
            ws.Cell(3, 9).Value = vslList.Owner_Vessel_Code;
            ws.Cell(4, 9).Value = vslList.Owner_Owner_Code;
            ws.Cell(3, 12).FormulaA1 = "=text(" + Year + ",\"0000\")";
            ws.Cell(4, 12).Value = DateTime.Today.ToString("yyyy/MM/dd");

            DateTime calendarMonth = new DateTime(Year, Month, 1);
            var acList = dbWallemRpt.TmplAccountCells.Where(x => x.TmplFileName == "T00201").ToList();
            foreach (var ac in acList)
            {
                var asList = dbBNReport.WAL_ActualSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).FirstOrDefault();
                var budgetLine = dbBNReport.WAL_BudgetSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).FirstOrDefault();
                var actualByPeriodLine = dbBNReport.WAL_ActualByPeriodSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).FirstOrDefault();

                decimal? accrual_for_costanalysis = 0;
                decimal? ytd_budget = 0;
                decimal? actual_excluding_accrual_01 = 0;
                decimal? actual_excluding_accrual_02 = 0;
                decimal? actual_excluding_accrual_03 = 0;
                decimal? actual_excluding_accrual_04 = 0;
                decimal? actual_excluding_accrual_05 = 0;
                decimal? actual_excluding_accrual_06 = 0;
                decimal? YTD_actual_exclude_accrual = 0;
                decimal? YTD_accrual = 0;
                decimal? curqtr_accrual = 0;
                decimal? QTD_opening_balance = 0;
                decimal? prior_year_accrual = 0;
                if (asList != null)
                {
                    YTD_actual_exclude_accrual = asList.YTD_Actual_Excluding_Accrual;
                    YTD_accrual = asList.YTD_Accrual;
                    curqtr_accrual = asList.CurQtr_Accrual;
                    accrual_for_costanalysis = asList.Accrual_for_CostAnalysis;
                    QTD_opening_balance = asList.QTD_Opening_Balance;
                    prior_year_accrual = asList.Prior_Year_Accrual;
                }
                if (budgetLine != null)
                {
                    ytd_budget = budgetLine.YTD_Budget;
                }
                if (actualByPeriodLine != null)
                {
                    actual_excluding_accrual_01 = actualByPeriodLine.Actual_Excluding_Accrual_01;
                    actual_excluding_accrual_02 = actualByPeriodLine.Actual_Excluding_Accrual_02;
                    actual_excluding_accrual_03 = actualByPeriodLine.Actual_Excluding_Accrual_03;
                    actual_excluding_accrual_04 = actualByPeriodLine.Actual_Excluding_Accrual_04;
                    actual_excluding_accrual_05 = actualByPeriodLine.Actual_Excluding_Accrual_05;
                    actual_excluding_accrual_06 = actualByPeriodLine.Actual_Excluding_Accrual_06;
                }

                ws.Cell(ac.RowNo, 5).Value = ytd_budget;
                ws.Cell(ac.RowNo, 6).Value = QTD_opening_balance; // F column 
                ws.Cell(ac.RowNo, 8).Value = (QTD_opening_balance - prior_year_accrual - actual_excluding_accrual_01 - actual_excluding_accrual_02 - actual_excluding_accrual_03) * -1; // H column
                ws.Cell(ac.RowNo, 9).Value = actual_excluding_accrual_04;  // I column
                ws.Cell(ac.RowNo, 10).Value = actual_excluding_accrual_05; // J column
                ws.Cell(ac.RowNo, 11).Value = actual_excluding_accrual_06; // K column
                ws.Cell(ac.RowNo, 13).Value = accrual_for_costanalysis; // M column
            }
            wb.Save();
        }

        private void create_NYK_T00301(string filename, string VslCode, int Year, int Month)
        {

            string ExcelTemplate = _env.WebRootPath + "/App_Data/NYK-T00301.xlsx";
            System.IO.File.Copy(ExcelTemplate, filename, true);

            XLWorkbook wb = new XLWorkbook(filename);
            var ws = wb.Worksheet(1);
            var vslList = dbBNReport.WAL_Vessels.Where(x => x.Vessel_Code == VslCode).FirstOrDefault();
            string strVesselName = vslList.Vessel_Name + " (" + vslList.Vessel_Code + ")";
            ws.Cell(3, 3).Value = strVesselName;
            ws.Cell(4, 3).Value = vslList.Owner_Name;
            ws.Cell(3, 9).Value = vslList.Owner_Vessel_Code;
            ws.Cell(4, 9).Value = vslList.Owner_Owner_Code;
            ws.Cell(3, 12).FormulaA1 = "=text(" + Year + ",\"0000\")";
            ws.Cell(4, 12).Value = DateTime.Today.ToString("yyyy/MM/dd");

            DateTime calendarMonth = new DateTime(Year, Month, 1);
            var acList = dbWallemRpt.TmplAccountCells.Where(x => x.TmplFileName == "T00301").ToList();
            foreach (var ac in acList)
            {
                var asList = dbBNReport.WAL_ActualSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).FirstOrDefault();
                var budgetLine = dbBNReport.WAL_BudgetSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).FirstOrDefault();
                var actualByPeriodLine = dbBNReport.WAL_ActualByPeriodSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).FirstOrDefault();

                decimal? accrual_for_costanalysis = 0;
                decimal? ytd_budget = 0;
                decimal? actual_excluding_accrual_01 = 0;
                decimal? actual_excluding_accrual_02 = 0;
                decimal? actual_excluding_accrual_03 = 0;
                decimal? actual_excluding_accrual_04 = 0;
                decimal? actual_excluding_accrual_05 = 0;
                decimal? actual_excluding_accrual_06 = 0;
                decimal? actual_excluding_accrual_07 = 0;
                decimal? actual_excluding_accrual_08 = 0;
                decimal? actual_excluding_accrual_09 = 0;
                decimal? YTD_actual_exclude_accrual = 0;
                decimal? YTD_accrual = 0;
                decimal? curqtr_accrual = 0;
                decimal? QTD_opening_balance = 0;
                decimal? prior_year_accrual = 0;
                if (asList != null)
                {
                    YTD_actual_exclude_accrual = asList.YTD_Actual_Excluding_Accrual;
                    YTD_accrual = asList.YTD_Accrual;
                    curqtr_accrual = asList.CurQtr_Accrual;
                    accrual_for_costanalysis = asList.Accrual_for_CostAnalysis;
                    QTD_opening_balance = asList.QTD_Opening_Balance;
                    prior_year_accrual = asList.Prior_Year_Accrual;
                }
                if (budgetLine != null)
                {
                    ytd_budget = budgetLine.YTD_Budget;
                }
                if (actualByPeriodLine != null)
                {
                    actual_excluding_accrual_01 = actualByPeriodLine.Actual_Excluding_Accrual_01;
                    actual_excluding_accrual_02 = actualByPeriodLine.Actual_Excluding_Accrual_02;
                    actual_excluding_accrual_03 = actualByPeriodLine.Actual_Excluding_Accrual_03;
                    actual_excluding_accrual_04 = actualByPeriodLine.Actual_Excluding_Accrual_04;
                    actual_excluding_accrual_05 = actualByPeriodLine.Actual_Excluding_Accrual_05;
                    actual_excluding_accrual_06 = actualByPeriodLine.Actual_Excluding_Accrual_06;
                    actual_excluding_accrual_07 = actualByPeriodLine.Actual_Excluding_Accrual_07;
                    actual_excluding_accrual_08 = actualByPeriodLine.Actual_Excluding_Accrual_08;
                    actual_excluding_accrual_09 = actualByPeriodLine.Actual_Excluding_Accrual_09;
                }

                ws.Cell(ac.RowNo, 5).Value = ytd_budget; // E column
                ws.Cell(ac.RowNo, 6).Value = QTD_opening_balance; // F column
                ws.Cell(ac.RowNo, 8).Value = (QTD_opening_balance - prior_year_accrual - actual_excluding_accrual_01 - actual_excluding_accrual_02 - actual_excluding_accrual_03 - actual_excluding_accrual_04 - actual_excluding_accrual_05 - actual_excluding_accrual_06 ) * -1; // H column
                ws.Cell(ac.RowNo, 9).Value = actual_excluding_accrual_07;  // I column
                ws.Cell(ac.RowNo, 10).Value = actual_excluding_accrual_08; // J column
                ws.Cell(ac.RowNo, 11).Value = actual_excluding_accrual_09; // K column
                ws.Cell(ac.RowNo, 13).Value = accrual_for_costanalysis; // M column

            }
            wb.Save();
        }

        private void create_NYK_T00401(string filename, string VslCode, int Year, int Month)
        {

            string ExcelTemplate = _env.WebRootPath + "/App_Data/NYK-T00401.xlsx";
            System.IO.File.Copy(ExcelTemplate, filename, true);

            XLWorkbook wb = new XLWorkbook(filename);
            var ws = wb.Worksheet(1);
            var vslList = dbBNReport.WAL_Vessels.Where(x => x.Vessel_Code == VslCode).FirstOrDefault();
            string strVesselName = vslList.Vessel_Name + " (" + vslList.Vessel_Code + ")";
            ws.Cell(3, 3).Value = strVesselName;
            ws.Cell(4, 3).Value = vslList.Owner_Name;
            ws.Cell(3, 9).Value = vslList.Owner_Vessel_Code;
            ws.Cell(4, 9).Value = vslList.Owner_Owner_Code;
            ws.Cell(3, 12).FormulaA1 = "=text(" + Year + ",\"0000\")";
            ws.Cell(4, 12).Value = DateTime.Today.ToString("yyyy/MM/dd");

            DateTime calendarMonth = new DateTime(Year, Month, 1);
            var acList = dbWallemRpt.TmplAccountCells.Where(x => x.TmplFileName == "T00401").ToList();
            foreach (var ac in acList)
            {
                var asList = dbBNReport.WAL_ActualSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).FirstOrDefault();
                var budgetLine = dbBNReport.WAL_BudgetSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).FirstOrDefault();
                var actualByPeriodLine = dbBNReport.WAL_ActualByPeriodSummaries.Where(x => x.Calendar_Month == calendarMonth && x.Vessel_Code == VslCode && x.Account_Code == ac.AccountCode).FirstOrDefault();

                decimal? accrual_for_costanalysis = 0;
                decimal? ytd_budget = 0;
                decimal? actual_excluding_accrual_01 = 0;
                decimal? actual_excluding_accrual_02 = 0;
                decimal? actual_excluding_accrual_03 = 0;
                decimal? actual_excluding_accrual_04 = 0;
                decimal? actual_excluding_accrual_05 = 0;
                decimal? actual_excluding_accrual_06 = 0;
                decimal? actual_excluding_accrual_07 = 0;
                decimal? actual_excluding_accrual_08 = 0;
                decimal? actual_excluding_accrual_09 = 0;

                decimal? actual_excluding_accrual_10 = 0;
                decimal? actual_excluding_accrual_11 = 0;
                decimal? actual_excluding_accrual_12 = 0;
                decimal? YTD_actual_exclude_accrual = 0;
                decimal? YTD_accrual = 0;
                decimal? curqtr_accrual = 0;
                decimal? QTD_Opening_Balance = 0;
                decimal? Prior_Year_Accrual = 0;
                if (asList != null)
                {
                    YTD_actual_exclude_accrual = asList.YTD_Actual_Excluding_Accrual;
                    YTD_accrual = asList.YTD_Accrual;
                    curqtr_accrual = asList.CurQtr_Accrual;
                    accrual_for_costanalysis = asList.Accrual_for_CostAnalysis;
                    QTD_Opening_Balance = asList.QTD_Opening_Balance;
                    Prior_Year_Accrual = asList.Prior_Year_Accrual;
                }
                if (budgetLine != null)
                {
                    ytd_budget = budgetLine.YTD_Budget;
                }
                if (actualByPeriodLine != null)
                {
                    actual_excluding_accrual_01 = actualByPeriodLine.Actual_Excluding_Accrual_01;
                    actual_excluding_accrual_02 = actualByPeriodLine.Actual_Excluding_Accrual_02;
                    actual_excluding_accrual_03 = actualByPeriodLine.Actual_Excluding_Accrual_03;
                    actual_excluding_accrual_04 = actualByPeriodLine.Actual_Excluding_Accrual_04;
                    actual_excluding_accrual_05 = actualByPeriodLine.Actual_Excluding_Accrual_05;
                    actual_excluding_accrual_06 = actualByPeriodLine.Actual_Excluding_Accrual_06;
                    actual_excluding_accrual_07 = actualByPeriodLine.Actual_Excluding_Accrual_07;
                    actual_excluding_accrual_08 = actualByPeriodLine.Actual_Excluding_Accrual_08;
                    actual_excluding_accrual_09 = actualByPeriodLine.Actual_Excluding_Accrual_09;

                    actual_excluding_accrual_10 = actualByPeriodLine.Actual_Excluding_Accrual_10;
                    actual_excluding_accrual_11 = actualByPeriodLine.Actual_Excluding_Accrual_11;
                    actual_excluding_accrual_12 = actualByPeriodLine.Actual_Excluding_Accrual_12;
                }

                ws.Cell(ac.RowNo, 5).Value = ytd_budget; // E Column
                ws.Cell(ac.RowNo, 6).Value = QTD_Opening_Balance; // F Column
                ws.Cell(ac.RowNo, 8).Value = (QTD_Opening_Balance - Prior_Year_Accrual 
                    - actual_excluding_accrual_01
                    - actual_excluding_accrual_02
                    - actual_excluding_accrual_03
                    - actual_excluding_accrual_04
                    - actual_excluding_accrual_05
                    - actual_excluding_accrual_06
                    - actual_excluding_accrual_07
                    - actual_excluding_accrual_08
                    - actual_excluding_accrual_09
                    ) * -1; // H Column
                ws.Cell(ac.RowNo, 9).Value = actual_excluding_accrual_10;  // I column
                ws.Cell(ac.RowNo, 10).Value = actual_excluding_accrual_11; // J column
                ws.Cell(ac.RowNo, 11).Value = actual_excluding_accrual_12; // K column
                ws.Cell(ac.RowNo, 13).Value = accrual_for_costanalysis; // M column
            }
            wb.Save();
        }

        private void create_Interocean_FCA(string filename, List<string> VesselList, int Year, int Month)
        {

            //string ExcelTemplate = _env.WebRootPath + "/App_Data/Interocean_Fleet_Cost_Analysis.xlsx";
            string ExcelTemplate = _env.WebRootPath + "/App_Data/Interocean_Fleet_Cost_Analysis.xlsx";
            System.IO.File.Copy(ExcelTemplate, filename, true);

            XLWorkbook wb = new XLWorkbook(filename);
                        
            string vesselxml = "<Vs>";
            foreach (string ve in VesselList)
            {
                vesselxml = vesselxml + "<V>" + ve + "</V>";
            }
            vesselxml = vesselxml + "</Vs>";
            var ws = wb.Worksheet("Data");
            var reportmonth = new DateTime(Year, Month, 1);
            dbBNReport.Database.SetCommandTimeout(3600); // 1 hour
            var itemList = dbBNReport.FCAs.FromSqlRaw("exec sp_Interocean_Fleet_CostAnalysis_2 {0}", reportmonth).ToList();            
            int RowNo = 2;
            var earlyMonth = new DateTime();
            foreach (var item in itemList)
            {

                ws.Cell(RowNo, 1).Value = item.Lookup_Key;
                ws.Cell(RowNo, 2).Value = item.Amount;
                ws.Cell(RowNo, 3).Value = item.YTD;
                ws.Cell(RowNo, 4).Value = item.CM;
                earlyMonth = item.EarlyMonth;

                RowNo++;
            }             

            ws = wb.Worksheet("Data");
            ws.Cell(1, 19).Value = earlyMonth.ToString("yyyy-MM-dd"); // column S
            var reportMonthLastDay = reportmonth.AddMonths(1).AddDays(-1);
            ws.Cell(1, 20).Value = reportMonthLastDay.ToString("yyyy-MM-dd"); // column T

            wb.Worksheet("Data").Hide();

            // convert cell formula to value            


            var vesselWSList = dbWallemRpt.InteroceanCostAnalysises.Where(x => x.DataType == "VesselWS" && VesselList.Contains(x.VesselCode)).ToList();
            foreach (var ve in vesselWSList)
            {
                ws = wb.Worksheet(ve.DataValue);
                var rangeList = dbWallemRpt.InteroceanCostAnalysises.Where(x => x.DataType == "ConvertToValue").ToList();
                foreach (var r in rangeList)
                {
                    var tmpRange = ws.Range(r.DataValue);
                    for (int i = 1; i <= tmpRange.RowCount(); i++)
                    {
                        for (int j = 1; j <= tmpRange.ColumnCount(); j++)
                        {
                            var tmpCell = tmpRange.Cell(i, j);
                            bool exFound = false;
                            try
                            {
                                var tmpValue = tmpCell.Value;

                            }
                            catch (Exception ex)
                            {
                                exFound = true;
                                _logger.LogInformation("cell value not available - " + ve.DataValue + " " + r.DataValue + " " + i + ", " + j);
                            }
                            if (exFound == false)
                                tmpRange.Cell(i, j).Value = tmpCell.Value;
                            else
                                tmpRange.Cell(i, j).Value = 0;
                        }
                    }
                }
                ws.Protect("interocean" + System.DateTime.Today.Year);
            }
            

            /*
            var staticWSList = dbWallemRpt.ALSeerCostAnalysises.Where(x => x.DataType == "StaticWS").ToList();
            foreach (var staticWS in staticWSList)
            {
                ws = wb.Worksheet(staticWS.DataValue);
                ws.Protect("interocean" + System.DateTime.Today.Year);
            }
            */

            wb.Save();
        }


        private void create_AL_Seer_CAP(string filename, List<string> VesselList, int Year, int Month)
        {

            string ExcelTemplate = _env.WebRootPath + "/App_Data/AL_Seer_Cost_Analysis_and_Prognosis.xlsx";
            System.IO.File.Copy(ExcelTemplate, filename, true);

            XLWorkbook wb = new XLWorkbook(filename);
            string vesselxml = "<Vs>";
            foreach (string ve in VesselList)
            {
                vesselxml = vesselxml + "<V>" + ve + "</V>";
            }
            vesselxml = vesselxml + "</Vs>";
            var ws = wb.Worksheet("Actual");
            var actualList = dbBNReport.CAPActuals.FromSqlRaw("exec sp_AL_Seer_CAP_Actual {0}, {1}", vesselxml, Year).ToList();
            int RowNo = 2;
            foreach(var actual in actualList)
            {
                ws.Cell(RowNo, 1).Value = actual.MATCH;
                ws.Cell(RowNo, 2).Value = actual.JAN;
                ws.Cell(RowNo, 3).Value = actual.FEB;
                ws.Cell(RowNo, 4).Value = actual.MAR;
                ws.Cell(RowNo, 5).Value = actual.APR;
                ws.Cell(RowNo, 6).Value = actual.MAY;
                ws.Cell(RowNo, 7).Value = actual.JUN;
                ws.Cell(RowNo, 8).Value = actual.JUL;
                ws.Cell(RowNo, 9).Value = actual.AUG;
                ws.Cell(RowNo, 10).Value = actual.SEP;
                ws.Cell(RowNo, 11).Value = actual.OCT;
                ws.Cell(RowNo, 12).Value = actual.NOV;
                ws.Cell(RowNo, 13).Value = actual.DEC;
                RowNo++;
            }
            ws = wb.Worksheet("Budget");
            var budgetList = dbBNReport.CAPBudgets.FromSqlRaw("exec sp_AL_Seer_CAP_Budget {0}, {1}", vesselxml, Year + "-" + Month + "-01").ToList();
            RowNo = 2;
            foreach (var budget in budgetList)
            {
                ws.Cell(RowNo, 1).Value = budget.MATCH;
                ws.Cell(RowNo, 2).Value = budget.YTD_Budget;
                ws.Cell(RowNo, 3).Value = budget.FY_Budget;
                RowNo++;
            }
            ws = wb.Worksheet("Running Days");
            var runningDaysList = dbBNReport.CAPRunningDays.FromSqlRaw("exec sp_AL_Seer_CAP_Running_Days {0}, {1}", vesselxml, Year ).ToList();
            RowNo = 2;
            foreach ( var runningDays in runningDaysList)
            {
                ws.Cell(RowNo, 1).Value = runningDays.VC_Month;
                ws.Cell(RowNo, 2).Value = runningDays.CurMth_Running_Days;
                RowNo++;
            }
            ws = wb.Worksheet("YTD Variance");
            var ytdVarianceList = dbBNReport.CAPYTDVariances.FromSqlRaw("exec sp_AL_Seer_CAP_YTD_Variance {0}, {1}", vesselxml, Year).ToList();
            RowNo = 2;
            foreach (var ytdVariance in ytdVarianceList)
            {
                ws.Cell(RowNo, 1).Value = ytdVariance.VC_Month;
                ws.Cell(RowNo, 2).Value = ytdVariance.YTD_Actual;
                ws.Cell(RowNo, 3).Value = ytdVariance.YTD_Budget;
                ws.Cell(RowNo, 4).Value = ytdVariance.YTDVariance;
                ws.Cell(RowNo, 5).Value = ytdVariance.YTDVariance_Percent;
                RowNo++;
            }
            ws = wb.Worksheet("Summary");
            ws.Cell(1, 3).Value = Year + "-" + Month + "-01";
            wb.Worksheet("Actual").Hide();
            wb.Worksheet("Budget").Hide();
            wb.Worksheet("Running Days").Hide();
            wb.Worksheet("YTD Variance").Hide();
            
            // convert cell formula to value
            
            
            var vesselWSList = dbWallemRpt.ALSeerCostAnalysises.Where(x => x.DataType == "VesselWS" && VesselList.Contains(x.VesselCode)).ToList();
            foreach(var ve in vesselWSList)
            {
                ws = wb.Worksheet(ve.DataValue);
                var rangeList = dbWallemRpt.ALSeerCostAnalysises.Where(x => x.DataType == "ConvertToValue").ToList();
                foreach (var r in rangeList)
                {
                    var tmpRange = ws.Range(r.DataValue);
                    for (int i = 1; i <= tmpRange.RowCount(); i++)
                    {
                        for (int j = 1; j <= tmpRange.ColumnCount(); j++)
                        {
                            var tmpCell = tmpRange.Cell(i, j);
                            bool exFound = false;
                            try
                            {
                                var tmpValue = tmpCell.Value;
                                
                            }catch(Exception ex)
                            {
                                exFound = true;
                                _logger.LogInformation("cell value not available - " + ve.DataValue + " " + r.DataValue + " " + i + ", " + j);
                            }
                            if(exFound == false)
                                tmpRange.Cell(i, j).Value = tmpCell.Value;
                            else
                                tmpRange.Cell(i, j).Value = 0;
                        }
                    }
                }
                ws.Protect("alseer" + System.DateTime.Today.Year);
             }            

            var staticWSList = dbWallemRpt.ALSeerCostAnalysises.Where(x => x.DataType == "StaticWS").ToList();
            foreach(var staticWS in staticWSList)
            {
                ws = wb.Worksheet(staticWS.DataValue);
                ws.Protect("alseer" + System.DateTime.Today.Year);
            }
            
            wb.Save();
        }

        /*
        private string getUserName()
        {
            string strUserName = "";
            using (var client = new HttpClient())
            {
                var userinfo_url = ConfigurationManager.AppSettings["userinfo_url"];
                client.BaseAddress = new Uri(userinfo_url);
                //HTTP GET
                var initial = "";
                if (System.Web.HttpContext.Current.Session["tkusername"] != null)
                {
                    initial = System.Web.HttpContext.Current.Session["tkusername"].ToString().ToUpper();
                }
                var responseTask = client.GetAsync(initial + "?json=true");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var readTask = result.Content.re<UserInfo>();
                        readTask.Wait();

                        var userInfo = readTask.Result;
                        strUserName = userInfo.FullName;
                    }
                }
            }
            return strUserName;
        }
        */

        /*
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        */


    }
}
