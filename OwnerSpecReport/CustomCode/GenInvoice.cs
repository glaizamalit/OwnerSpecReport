using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceReference1;
using System.IO;
using System.ServiceModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;
using OwnerSpecReport.Models;

namespace OwnerSpecReport
{
    public interface IGenInvoice
    {
        void SetPeriod(string Year, string Month);
        void SetSMC(string strSMC);
        bool Run();        

    }

    class GenInvoice : IGenInvoice
    {
        static string ReportExecution2005EndPointUrl;
        static string SsrsServiceAccountActiveDirectoryUserName;
        static string SsrsServiceAccountActiveDirectoryPassword;
        static string SsrsServiceAccountActiveDirectoryDomain;
        static string ReportPath;
        static string ReportWidth = "8.5in";
        static string ReportHeight = "11in";
        static string ReportFormat = "PDF";  // Other options include WORDOPENXML and EXCELOPENXML
        static string OutputFolder;
        static string VesselCode;
        static string SMC;
        static int ProcessYear;
        static int ProcessMonth;
        static DateTime CalendarMonth;
        const string HistoryId = null;

        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly BIWallemDbContext _BIWallemDb;
        string MailTo;
        string MailCC;
        string SMTPServer;
        static List<string> InvoiceList = new List<string>();


        public GenInvoice(ILogger<GenInvoice> logger, IConfiguration config, BIWallemDbContext BIWallemDb)
        {
            _logger = logger;
            _config = config;
            _BIWallemDb = BIWallemDb;
        }       
        
        public void SetPeriod(string Year, string Month)
        {
            ProcessYear = int.Parse(Year);
            ProcessMonth = int.Parse(Month);
        }

        public void SetSMC(string strSMC)
        {
            SMC = strSMC;
        }
        
        public bool Run()
        {
            // _logger.LogInformation("Application Started at {dateTime}", DateTime.UtcNow);

            bool rtn = false;

            try
            {

                CalendarMonth = new DateTime(ProcessYear, ProcessMonth, 1);
                // Console.WriteLine("Processing {0}-{1}-{2}", ProcessYear, ProcessMonth, SMC);

                // var builder = new ConfigurationBuilder()
                //                    .SetBasePath(Directory.GetCurrentDirectory())
                //                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);


                ReportExecution2005EndPointUrl = _config.GetSection("SSRS").GetSection("ServiceURL").Value;
                SsrsServiceAccountActiveDirectoryUserName = _config.GetSection("SSRS").GetSection("LoginUserName").Value;
                SsrsServiceAccountActiveDirectoryPassword = _config.GetSection("SSRS").GetSection("LoginPassword").Value;
                SsrsServiceAccountActiveDirectoryDomain = _config.GetSection("SSRS").GetSection("LoginDomain").Value;
                ReportPath = _config.GetSection("SSRS").GetSection("ReportPath").Value;
                OutputFolder = _config.GetSection("Output").GetSection("Folder").Value;
                MailTo = _config.GetSection("Output").GetSection("MailTo").Value;
                MailCC = _config.GetSection("Output").GetSection("MailCC").Value;
                SMTPServer = _config.GetSection("Output").GetSection("SMTPServer").Value;

                // transfer records to RealMarine table for PDF generation
                _BIWallemDb.Database.ExecuteSqlRaw("exec sp_RealMarine_TransferPDF {0}", SMC);

                string cmd = "";
                string OriginalSMC = SMC;
                if (SMC == "NYK")
                {
                    cmd = "SELECT distinct [Vessel] Vessel_Code, [CONO] SMC FROM [BIWallem].[dbo].[RealMarine] where cono = 'W10' and Source = 'BNP' and Is_NYKVessel = 1 order by cono, Vessel";
                }
                else
                {
                    cmd = "SELECT distinct [Vessel] Vessel_Code, [CONO] SMC FROM [BIWallem].[dbo].[RealMarine] where cono = '" + SMC + "' and Source = 'BNP' and Is_NYKVessel = 0 order by cono, Vessel";
                }

                InvoiceList = new List<string>();

                var rList = _BIWallemDb.RealMarineListResults.FromSqlRaw(cmd);
                foreach (var r in rList)
                {
                    VesselCode = r.Vessel_Code;
                    SMC = r.SMC;
                    RunReport().Wait();
                }

                // email invoice

                var smtpClient = new SmtpClient(SMTPServer);

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(SMTPServer);
                mail.From = new MailAddress("no-reply@wallem.com");
                string[] arrTo = MailTo.Split(";");
                foreach (string s in arrTo) { 
                    mail.To.Add(s);
                }
                string[] arrCC = MailCC.Split(";");
                foreach (string cc in arrCC)
                {
                    mail.CC.Add(cc);
                }
                mail.Subject = "RealMarine Invoice for " + OriginalSMC;
                mail.Body = "Please see attached invoices.\n\n* This is system generated message.\n\n";

                foreach (string Invoice in InvoiceList) {
                    Attachment attachment = new Attachment(OutputFolder + "\\" + Invoice);
                    mail.Attachments.Add(attachment);
                }                
                SmtpServer.Send(mail);

                mail.Dispose(); // unlock attachment

                rtn = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                rtn = false;
            }

            return(rtn);
        }


        private static async Task RunReport()
        {
            ReportExecutionServiceSoapClient rs = CreateClient();
            var trustedHeader = new TrustedUserHeader();

            LoadReportResponse loadReponse = await LoadReport(rs, trustedHeader);

            await AddParametersToTheReport(rs, loadReponse.ExecutionHeader, trustedHeader);

            RenderResponse response = await RenderReportByteArrayAsync(loadReponse.ExecutionHeader, trustedHeader, rs, ReportFormat, ReportWidth, ReportHeight);

            SaveResultToFile(response.Result, "RealMarine_Invoice_Details_" + ProcessYear + ProcessMonth.ToString("00") + "_" + SMC + "_" + VesselCode + ".PDF");
        }

        private static async Task<LoadReportResponse> LoadReport(ReportExecutionServiceSoapClient rs, TrustedUserHeader trustedHeader)
        {
            // Get the report and set the execution header.
            // Failure to set the execution header will result in this error: "The session identifier is missing. A session identifier is required for this operation."
            // See https://social.msdn.microsoft.com/Forums/sqlserver/en-US/17199edb-5c63-4815-8f86-917f09809504/executionheadervalue-missing-from-reportexecutionservicesoapclient
            LoadReportResponse loadReponse = await rs.LoadReportAsync(trustedHeader, ReportPath, HistoryId);

            return loadReponse;
        }

        private static async Task<SetExecutionParametersResponse> AddParametersToTheReport(ReportExecutionServiceSoapClient rs, ExecutionHeader executionHeader, TrustedUserHeader trustedHeader)
        {
            // Add parameters to the report
            var reportParameters = new List<ParameterValue>();
            reportParameters.Add(new ParameterValue() { Name = "Vessel", Value = VesselCode });
            reportParameters.Add(new ParameterValue() { Name = "COMPANY", Value = SMC });
            
            SetExecutionParametersResponse setParamsResponse = await rs.SetExecutionParametersAsync(executionHeader, trustedHeader, reportParameters.ToArray(), "en-US");

            return setParamsResponse;
        }

        private static async Task<RenderResponse> RenderReportByteArrayAsync(ExecutionHeader execHeader, TrustedUserHeader trustedHeader,
           ReportExecutionServiceSoapClient rs, string format, string width, string height)
        {
            string deviceInfo = String.Format("<DeviceInfo><PageHeight>{0}</PageHeight><PageWidth>{1}</PageWidth><PrintDpiX>300</PrintDpiX><PrintDpiY>300</PrintDpiY></DeviceInfo>", height, width);

            var renderRequest = new RenderRequest(execHeader, trustedHeader, format, deviceInfo);

            //get report bytes
            RenderResponse response = await rs.RenderAsync(renderRequest);
            return response;
        }


        private static ReportExecutionServiceSoapClient CreateClient()
        {
            var rsBinding = new BasicHttpBinding();
            // rsBinding.Security.Mode = BasicHttpSecurityMode.Transport; // "Transport" is for https
            rsBinding.Security.Mode = BasicHttpSecurityMode.None;
            rsBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;

            // So we can download reports bigger than 64 KBytes
            // See https://stackoverflow.com/questions/884235/wcf-how-to-increase-message-size-quota
            rsBinding.MaxBufferPoolSize = 20000000;
            rsBinding.MaxBufferSize = 20000000;
            rsBinding.MaxReceivedMessageSize = 20000000;

            var rsEndpointAddress = new EndpointAddress(ReportExecution2005EndPointUrl);
            var rsClient = new ReportExecutionServiceSoapClient(rsBinding, rsEndpointAddress);

            // Set user name and password
            rsClient.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            rsClient.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                SsrsServiceAccountActiveDirectoryUserName,
                SsrsServiceAccountActiveDirectoryPassword,
                SsrsServiceAccountActiveDirectoryDomain);

            return rsClient;
        }

        private static void SaveResultToFile(byte[] result, string fileName)
        {
            string DestFolder = OutputFolder + "\\";
            using (var fs = File.Create(DestFolder + "\\" + fileName))
            {
                // var sw = new StreamWriter(fs);
                fs.Write(result);
            }            

            InvoiceList.Add(fileName);
        }

    }


}
