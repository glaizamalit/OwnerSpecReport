using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Net.Mail;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using OwnerSpecReport.Models;
using Microsoft.EntityFrameworkCore;

namespace OwnerSpecReport
{
    public class SsoAuth : ActionFilterAttribute, IActionFilter
    {

        private readonly ILogger<SsoAuth> logger;
        private readonly IConfiguration Configuration;
        private IHttpContextAccessor _httpContextAccessor;
        private static ISso SsoObject;

        private WallemRptModelDbContext _WallemRpt;

        public SsoAuth(IConfiguration configuration, ILogger<SsoAuth> sys_logger, IHttpContextAccessor httpContextAccessor, ISso in_Sso, WallemRptModelDbContext WallemRpt)
        {
            Configuration = configuration;
            logger = sys_logger;
            _httpContextAccessor = httpContextAccessor;
            SsoObject = in_Sso;
            _WallemRpt = WallemRpt;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (SsoObject.IsLogged())
            {
                //Get the rights
                if (!IsAuthorized())
                {
                    _httpContextAccessor.HttpContext.Response.Redirect("/Unauthorized");
                }
            }
            else
            {

                try
                {
                    string token = _httpContextAccessor.HttpContext.Request.Query["tkid"];
                    string isAllow = _httpContextAccessor.HttpContext.Request.Query["tkallow"];
                    bool isAllowed = isAllow == null ? false : Boolean.Parse(isAllow);
                    bool isAuthen = false;

                    if (string.IsNullOrEmpty(token))
                    {
                        logger.LogDebug("### Token is null, redirect to SSO, for " +  filterContext.HttpContext.Request.Method + " request ");

                        _httpContextAccessor.HttpContext.Response.Redirect(SsoObject.SsoLoginurl());
                    }
                    else
                    {
                        // send token to IdP (SAML Request)
                        logger.LogDebug("Token is not null, send SAML request to IdP SSO ");

                        var data = SsoObject.PostToken(token, isAllowed);
                        //SPHelper.CheckSSOViaService(ref token, ref isAuthen, isAllow);
                        if (data != null)
                        {
                            // registerLink

                            _httpContextAccessor.HttpContext.Session.SetString("tkusername", data[18].Value);
                            _httpContextAccessor.HttpContext.Session.SetString("tkid", data[16].Value);
                            var strFullName = getUserFullName(data[18].Value);
                            _httpContextAccessor.HttpContext.Session.SetString("UserFullName", strFullName);

                            isAuthen = true;
                        }
                        logger.LogDebug("Called CheckSSO method, result: " + isAuthen);
                        if (isAuthen)
                        {
                            string usn = _httpContextAccessor.HttpContext.Session.GetString("tkusername");

                            logger.LogInformation(usn + " - Authenticated.");
                            
                            //TODO: Do we still need to get the token???

                            if (!IsAuthorized())
                            {
                                _httpContextAccessor.HttpContext.Response.Redirect("~/Unauthorized");
                            }

                            this.OnActionExecuting(filterContext);
                        }
                        else
                        {
                            logger.LogDebug("Problem with SSO to verify token");
                            _httpContextAccessor.HttpContext.Response.Redirect(SsoObject.SsoLoginurl());
                        }
                    }
                }
                catch (Exception ex)
                {
                    string usn = _httpContextAccessor.HttpContext.Session.GetString("tkusername");
                    SendMail(usn);
                    logger.LogError("Error " + ex.Message);
                    logger.LogError("Error " + ex.InnerException);

                    // throw ex;
                }
            }

        }

        /*
        public Guid getid(string usn)
        {
            Guid userID = new Guid();
            if (usn != null)
            {

                try
                {
                    var connectionString = ConfigurationManager.ConnectionStrings["VRMdb"].ConnectionString;
                    string queryString = "select userID from tblusers where musrLoginID like '%" + usn + "%'";
                    using (var connection = new SqlConnection(connectionString))
                    {
                        var command = new SqlCommand(queryString, connection);
                        connection.Open();
                        var reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                userID = reader.GetGuid(0);
                            }
                        }
                        connection.Close();
                    }
                    return userID;
                }
                catch (Exception e)
                {
                    SendMail(usn);
                    return userID;
                }
            }
            else
            {
                SendMail(usn);
                userID = Guid.Empty;
                return userID;
            }

        }
        */

        public string getIP()
        {
            string myIP = "";
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress addr in localIPs)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {

                    myIP = addr.ToString();
                }
            }
            return myIP;

        }

        public void SendMail(string username)
        {
            try
            {
                /*
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(ConfigurationManager.AppSettings["SMTPFrom"]);
                mail.To.Add(ConfigurationManager.AppSettings["SMTPTo"]);
                mail.CC.Add(ConfigurationManager.AppSettings["SMTPCC"]);
                mail.Subject = ConfigurationManager.AppSettings["SMTPSubjectNoAccount"] + " " + DateTime.Now.ToString("yyyy-MM-dd");
                mail.Body = ConfigurationManager.AppSettings["SMTPGreetings"] + "\n\n" + "User with network login ID “" + username.ToUpper() + "” " + ConfigurationManager.AppSettings["SMTPMessageNoAccount"];
                SmtpClient smtp = new SmtpClient(ConfigurationManager.AppSettings["SMTPHost"]);
                smtp.Send(mail);
                */
                // send email disabled
            }
            catch (Exception ep)
            {
                Console.WriteLine("Failed to send email with the following error:");
                Console.WriteLine(ep.Message);
            }
        }

        public bool IsAuthorized()
        {
               
            var isAuthorized = _httpContextAccessor.HttpContext.Session.GetString("IsAuthorized");

            if (isAuthorized == null)
            {
                string UserInitial = _httpContextAccessor.HttpContext.Session.GetString("tkusername");
                // string strCmd = "select 'apfk' as Initial";
                string strCmd = "exec OwnerSpecReport_IsGroupMember '"+ UserInitial +"'";
                var ResultList = _WallemRpt.IsGroupMemberResults.FromSqlRaw(strCmd).ToList();                
                if (ResultList.Count() == 0)
                {
                    return false;
                }
                else
                {
                    _httpContextAccessor.HttpContext.Session.SetString("IsAuthorized", "true");
                    return true;
                }
            }else
            {
                return true;
            }
            
            /*
            _httpContextAccessor.HttpContext.Session.SetString("IsAuthorized","true");
            return true;
            */
            
        }

        private string getUserFullName(string initial)
        {
            string strUserName = "";
            using (var client = new HttpClient())
            {
                var userinfo_url = Configuration["UserInfo:service_url"];
                client.BaseAddress = new Uri(userinfo_url);
                //HTTP GET
                var responseTask = client.GetAsync(initial + "?json=true");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        var readTask = result.Content.ReadAsAsync<UserInfo>();
                        readTask.Wait();

                        var userInfo = readTask.Result;
                        strUserName = userInfo.FullName;
                    }
                }
            }
            return strUserName;
        }

    }
}