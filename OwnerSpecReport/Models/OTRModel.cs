using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OwnerSpecReport.Models
{

    /*
    public class VRMdbDbContext : DbContext
    {
        public VRMdbDbContext(DbContextOptions<VRMdbDbContext> options)
            : base(options)
        {
        }
                
        public DbSet<IsGroupMemberResult> IsGroupMemberResults { get; set; }
    }
    */

    public class WallemRptModelDbContext : DbContext
    {
        public WallemRptModelDbContext(DbContextOptions<WallemRptModelDbContext> options)
            : base(options)
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
        public DbSet<TmplAccountCell> TmplAccountCells { get; set; }
        public DbSet<TmplExceptCell> TmplExceptCells { get; set; }

        public DbSet<IsGroupMemberResult> IsGroupMemberResults { get; set; }

        public DbSet<ALSeerCostAnalysis> ALSeerCostAnalysises { get; set; }
        public DbSet<InteroceanCostAnalysis> InteroceanCostAnalysises { get; set; }
    }

    [Keyless]
    public class IsGroupMemberResult
    {
        public string Initial { get; set; }
    }
    
    public class BNReportDbContext : DbContext
    {
        // Your context has been configured to use a 'VARModels' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'VARApi.Models.VARModels' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'VARModels' 
        // connection string in the application configuration file.
        public BNReportDbContext(DbContextOptions<BNReportDbContext> options)
              : base(options)
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
        public DbSet<WAL_Vessel> WAL_Vessels { get; set; }
        public DbSet<WAL_ActualSummary> WAL_ActualSummaries { get; set; }
        public DbSet<WAL_BudgetSummary> WAL_BudgetSummaries { get; set; }

        public DbSet<WAL_ActualByPeriodSummary> WAL_ActualByPeriodSummaries { get; set; }
        public DbSet<CAPActual> CAPActuals { get; set; }
        public DbSet<CAPBudget> CAPBudgets { get; set; }
        public DbSet<CAPRunningDays> CAPRunningDays { get; set; }
        public DbSet<CAPYTDVariance> CAPYTDVariances { get;set; }
        public DbSet<WAL_MANUALACCR> WAL_MANUALACCRs { get; set; }
        public DbSet<WAL_RealMarine_Billing> WAL_RealMarine_Billings { get; set; }
        public DbSet<FCA> FCAs { get; set; }
    }

    public class WallemProDbContext : DbContext
    {
        // Your context has been configured to use a 'VARModels' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'VARApi.Models.VARModels' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'VARModels' 
        // connection string in the application configuration file.
        public WallemProDbContext(DbContextOptions<WallemProDbContext> options)
              : base(options)
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
        
    }

    public class BIWallemDbContext : DbContext
    {
        // Your context has been configured to use a 'VARModels' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'VARApi.Models.VARModels' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'VARModels' 
        // connection string in the application configuration file.
        public BIWallemDbContext(DbContextOptions<BIWallemDbContext> options)
              : base(options)
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
        public DbSet<RealMarine_Auto> RealMarine_Autos { get; set; }
        public DbSet<RealMarine_Journal> RealMarine_Journals { get; set; }

        public DbSet<RealMarine_Eyeshare> RealMarine_Eyeshares { get; set; }

        public DbSet<RealMarineListResult> RealMarineListResults { get; set;}

    }

    public class FormModel
    {
        [Required]
        public int Year { get; set; }
        [Required]
        public int Month { get; set; }
        [Required]
        public string ReportType { get; set; }
        [Required]
        public string VesselGroup { get; set; }

        [Required]
        [DisplayName("Reports")]
        public List<string> ReportList { get; set; }
        [Required]
        [DisplayName("Vessels")]
        public List<string> VesselList { get; set; }

        public List<SelectListItem> MonthList { get; } = new List<SelectListItem>
        {
                new SelectListItem{ Text="January", Value = "1" },
                new SelectListItem{ Text="February", Value = "2" },
                new SelectListItem{ Text="March", Value = "3"},
                new SelectListItem{ Text="April", Value = "4"},
                new SelectListItem{ Text="May", Value = "5"},
                new SelectListItem{ Text="June", Value = "6"},
                new SelectListItem{ Text="July", Value = "7"},
                new SelectListItem{ Text="August", Value = "8"},
                new SelectListItem{ Text="September", Value = "9"},
                new SelectListItem{ Text="October", Value = "10"},
                new SelectListItem{ Text="November", Value = "11"},
                new SelectListItem{ Text="December", Value = "12"}
        };

        public List<SelectListItem> ReportTypeList { get; } = new List<SelectListItem>
        {
                new SelectListItem{ Text="NYK Excel", Value = "NYK Excel" },
                new SelectListItem{ Text="AL Seer Excel", Value = "AL Seer Excel" },
                new SelectListItem{ Text="Interocean Excel", Value = "Interocean Excel" }
                // new SelectListItem{ Text="Genco Excel", Value = "Genco Excel" }                
        };

        public List<SelectListItem> VesselGroupList { get; set; }
    }

    [Table("WAL_Vessel")]
    public class WAL_Vessel
    {
        [Key]
        public string Vessel_Code { get; set; }

        public string Report_Format { get; set; }

        public string Vessel_Name { get; set; }

        public string Base_Currency { get; set; }

        public string Owner_Name { get; set; }

        public string Owner_Vessel_Code { get; set; }

        public string Owner_Owner_Code { get; set; }

        public string Vessel_Group { get; set; }

        public string Accrual_Basis { get; set; }

        public decimal? Monthly_Funding { get; set; }

        public string SMC { get; set; }

    }

    [Table("WAL_ActualSummary")]
    public class WAL_ActualSummary
    {
        public string Vessel_Code { get; set; }

        public DateTime Calendar_Month { get; set; }

        public string Account_Code { get; set; }

        public decimal? CurMth_Actual { get; set; }

        public decimal? CurMth_Accrual { get; set; }

        public decimal? Prior_Month_Actual { get; set; }

        public decimal? Prior_Year_Accrual { get; set; }

        public decimal? CurQtr_Actual { get; set; }

        public decimal? CurMth_Actual_Excluding_Accrual { get; set; }

        public decimal? YTD_Actual { get; set; }

        public decimal? YTD_Actual_Excluding_Accrual { get; set; }

        public decimal? CurMth_Actual_for_OwnerStatement { get; set; }

        public int? YTD_Running_Days { get; set; }

        public int? CurMth_Running_Days { get; set; }

        public decimal? Accrual_for_CostAnalysis { get; set; }

        public decimal? YTD_Actual_Daily { get; set; }

        public decimal? Monthly_Average { get; set; }

        public decimal? CurMth_Variance { get; set; }

        public decimal? YTD_Variance { get; set; }

        public decimal? YTD_Variance_Percent { get; set; }

        public decimal? Opening_Balance { get; set; }

        public decimal? Opening_Actual { get; set; }

        public decimal? QTD_Opening_Balance { get; set; }

        public DateTime UpdateDateTime { get; set; }

        [Key]
        public long ID { get; set; }

        public decimal? CurQtr_Accrual { get; set; }

        public decimal? YTD_Accrual { get; set; }
    }

    [Table("WAL_BudgetSummary")]
    public class WAL_BudgetSummary
    {
        public string Vessel_Code { get; set; }

        public DateTime Calendar_Month { get; set; }

        public string Account_Code { get; set; }

        public decimal? CurMth_Budget { get; set; }

        public decimal? YTD_Budget { get; set; }

        public decimal? FY_Budget { get; set; }

        public decimal? Remaining_Year_Budget { get; set; }

        public decimal? FY_Prognosis { get; set; }

        public decimal? YTD_Budget_Daily { get; set; }

        public decimal? FY_Budget_Daily { get; set; }

        public decimal? FY_Prognosis_Daily { get; set; }

        public DateTime UpdateDateTime { get; set; }

        [Key]
        public long ID { get; set; }

    }

    [Table("TmplAccountCell")]
    public class TmplAccountCell
    {
        [Key]
        public int ID { get; set; }

        public string TmplFileName { get; set; }

        public string AccountCode { get; set; }

        public int RowNo { get; set; }

    }


    [Table("TmplExceptCell")]
    public class TmplExceptCell
    {
        public string TmplFileName { get; set; }

        public string Cell { get; set; }

        public string VersionYrMth { get; set; }

        [Key]
        public int ID { get; set; }

    }

    [Table("WAL_ActualByPeriodSummary")]
    public class WAL_ActualByPeriodSummary
    {
        public string Vessel_Code { get; set; }

        public DateTime Calendar_Month { get; set; }

        public string Account_Code { get; set; }

        public decimal Actual_01 { get; set; }

        public decimal Actual_02 { get; set; }

        public decimal Actual_03 { get; set; }

        public decimal Actual_04 { get; set; }

        public decimal Actual_05 { get; set; }

        public decimal Actual_06 { get; set; }

        public decimal Actual_07 { get; set; }

        public decimal Actual_08 { get; set; }

        public decimal Actual_09 { get; set; }

        public decimal Actual_10 { get; set; }

        public decimal Actual_11 { get; set; }

        public decimal Actual_12 { get; set; }

        public decimal Actual_Excluding_Accrual_01 { get; set; }

        public decimal Actual_Excluding_Accrual_02 { get; set; }

        public decimal Actual_Excluding_Accrual_03 { get; set; }

        public decimal Actual_Excluding_Accrual_04 { get; set; }

        public decimal Actual_Excluding_Accrual_05 { get; set; }

        public decimal Actual_Excluding_Accrual_06 { get; set; }

        public decimal Actual_Excluding_Accrual_07 { get; set; }

        public decimal Actual_Excluding_Accrual_08 { get; set; }

        public decimal Actual_Excluding_Accrual_09 { get; set; }

        public decimal Actual_Excluding_Accrual_10 { get; set; }

        public decimal Actual_Excluding_Accrual_11 { get; set; }

        public decimal Actual_Excluding_Accrual_12 { get; set; }

        public decimal Actual_OwnerStatement_01 { get; set; }

        public decimal Actual_OwnerStatement_02 { get; set; }

        public decimal Actual_OwnerStatement_03 { get; set; }

        public decimal Actual_OwnerStatement_04 { get; set; }

        public decimal Actual_OwnerStatement_05 { get; set; }

        public decimal Actual_OwnerStatement_06 { get; set; }

        public decimal Actual_OwnerStatement_07 { get; set; }

        public decimal Actual_OwnerStatement_08 { get; set; }

        public decimal Actual_OwnerStatement_09 { get; set; }

        public decimal Actual_OwnerStatement_10 { get; set; }

        public decimal Actual_OwnerStatement_11 { get; set; }

        public decimal Actual_OwnerStatement_12 { get; set; }

        public DateTime UpdateDateTime { get; set; }

        [Key]
        public long ID { get; set; }

    }


    public class UserInfo
    {
        public string Initial { get; set; }
        public string FullName { get; set; }
        public string Surname { get; set; }
        public string GivenName { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
    }

    [Keyless]
    public class CAPActual
    {
        public string MATCH { get; set; }
        public decimal JAN { get; set; }
        public decimal FEB { get; set; }
        public decimal MAR { get; set; }
        public decimal APR { get; set; }
        public decimal MAY { get; set; }
        public decimal JUN { get; set; }
        public decimal JUL { get; set; }
        public decimal AUG { get; set; }
        public decimal SEP { get; set; }
        public decimal OCT { get; set; }
        public decimal NOV { get; set; }
        public decimal DEC { get; set; }
    }

    [Keyless]
    public class CAPRunningDays
    {
        public string VC_Month { get; set; }
        public int CurMth_Running_Days { get; set; }
    }

    [Keyless]
    public class CAPBudget
    {
        public string MATCH { get; set; }
        public decimal YTD_Budget { get; set; }
        public decimal FY_Budget { get; set; }
    }

    [Keyless]
    public class CAPYTDVariance
    {
        public string VC_Month { get; set; }
        public decimal YTD_Actual { get; set; }
        public decimal YTD_Budget { get; set; }
        public decimal YTDVariance { get; set; }
        public decimal YTDVariance_Percent { get; set; }
    }

    [Keyless]
    public class FCA 
    {
        public string Lookup_Key { get; set; }
        public decimal Amount { get; set; } 

        public decimal YTD { get; set; }
        public decimal CM { get; set; }
        public DateTime EarlyMonth { get; set; }
    }

    [Table("ALSeerCostAnalysis")]
    public class ALSeerCostAnalysis
    {
        [Key]
        public int ID { get; set; }

        public string DataType { get; set; }

        public string DataValue { get; set; }

        public string VesselCode { get; set; }

    }

    [Table("InteroceanCostAnalysis")]
    public class InteroceanCostAnalysis
    {
        [Key]
        public int ID { get; set; }

        public string DataType { get; set; }

        public string DataValue { get; set; }

        public string VesselCode { get; set; }

    }


    [Table("WAL_MANUALACCR")]
    public class WAL_MANUALACCR
    {
        [Key]
        public int ID { get; set; }
        public string Vessel_Code { get; set; }
        public DateTime Calendar_Month { get; set; }
        public string Voucher_Number { get; set; }
        public string PO_Number { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDt { get; set; }
    }


    public class ManualAccrualClass
    {
        [Key]
        public int ID { get; set; }
        public string Vessel_Name { get; set; }
        public string Vessel_Code { get; set; }
        public string Calendar_Month { get; set; }
        public string Calendar_MonthString { get; set; }
        public string Voucher_Number { get; set; }
        public string PO_Number { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDt { get; set; }
    }

    [Table("WAL_RealMarine_Billing")]
    public class WAL_RealMarine_Billing
    {
        [Key]
        public int ID { get; set; }

        public string Vessel_Code { get; set; }

        public string Charge_Account_Code { get; set; }

        public decimal Amount { get; set; }

        public string Created_By { get; set; }

        public DateTime? Created_Date { get; set; }

        public string Updated_By { get; set; }

        public DateTime? Updated_Date { get; set; }

    }

    public class RealMarine_Charge_VM
    {
        [Key]
        public int ID { get; set; }

        public string Vessel_Code { get; set; }

        public string Vessel_Name { get; set; }

        public string Charge_Account_Code { get; set; }

        public decimal Amount { get; set; }

        public string Created_By { get; set; }

        public DateTime? Created_Date { get; set; }

        public string Updated_By { get; set; }

        public DateTime? Updated_Date { get; set; }

    }

    [Table("RealMarine_Auto")]
    public class RealMarine_Auto
    {
        [Key]
        public long ID { get; set; }

        public string Purchase_Order { get; set; }

        public string Requisition { get; set; }

        public string Vessel { get; set; }

        public string CONO { get; set; }

        public DateTime? Date { get; set; }

        public string Vendor { get; set; }

        public string Source { get; set; }

        public string Vessel_Name { get; set; }

        public string Account { get; set; }

        public decimal? Amount { get; set; }

        public string Vessel_Group { get; set; }

        public string Invoice_Ref { get; set; }

        public string Created_By { get; set; }

        public DateTime? Created_Date { get; set; }

        public bool? Is_NYKVessel { get; set; }

    }

    [Keyless]
    public class RealMarine_Journal
    {
        public DateTime Created_Date { get; set; }
        public string Account_Type { get; set; }
        public string Account { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; }
        public string Rate_Type { get; set; }
        public int Exch_Rate {get; set;}
        public decimal? Debit { get; set; }
        public int Credit { get; set; }
        public string DIM3 { get; set; }
        public string VC_Name { get; set; }
        public string Offset_Type { get; set; }
        public string Offset_Account { get; set; }
    }

    [Keyless]
    public class RealMarine_Eyeshare
    {
        public string Vessel { get; set; }
        public string Invoice_Ref { get; set; }
        public string Account { get; set; }
        public decimal? Sum_of_Amount { get; set; }        
    }

    [Keyless]
    public class RealMarineListResult
    {
        public string Vessel_Code { get; set; }

        public string SMC { get; set; }
    }

}