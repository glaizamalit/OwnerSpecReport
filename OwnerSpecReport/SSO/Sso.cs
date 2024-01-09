using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace OwnerSpecReport
{
    public interface ISso
    {
        string SsoLoginurl();
        List<XElement> PostToken(string token, bool isAllowed);
        bool IsLogged();
    }

    public class Sso : ISso
    {
        private readonly ILogger<Sso> logger;
        private readonly IConfiguration Configuration;
        private IHttpContextAccessor _httpContextAccessor;        

        public Sso(IConfiguration configuration, ILogger<Sso> sys_logger, IHttpContextAccessor httpContextAccessor)
        {
            Configuration = configuration;
            logger = sys_logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private string localLoginUrl()
        {
            string _localLoginUrl = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase + "/";

            return _localLoginUrl;
        }

        public string SsoLogoutUrl()
        {
            string url = Configuration["SSO:logout_url"];
            return url;
        }

        public string SsoLoginurl()
        {

            //SSO login url example
            //http://sso.wallem.com/login?tkfrom=http://wis-timesheet.herokuapp.com/users/sign_in*

            string url = Configuration["SSO:login_url"]+"?"+"tkfrom="+ localLoginUrl();
                     
            return url;
        }

        public List<XElement> PostToken(string token, bool isAllowed)
        {
            string decryptedToken = decryptToken(token);
            //if available_server{
            string response_data = ssoData(decryptedToken, isAllowed.ToString(), availableServer());
            if (response_data != null && response_data != "null")
            {
                var nodes = decryptResponse(response_data);            

                return nodes;
            }else
            {
                return null;
            }
           
        }

        private string decryptToken(string token)
        {
            //Example:
            //def decrypt_token(str)
            //  token_key = "aaid17"
            //  str[token_key.length..- 1].reverse
            //end
            //print decrypt_token("abcdefghijklmnopqrstuvwxyz")
            //output: zyxwvutsrqponmlkjihg 

            string tokenKey = Configuration["SSO:token_key"].ToString();
            string subStr = token.Substring(tokenKey.Length);
            string decryptedToken =new string(subStr.ToCharArray().Reverse().ToArray());
            return decryptedToken.ToString();
        }

        public string SsoConfig()
        {
            return "";
        }

        private string availableServer()
        {
            return GetOnlineServer();
        }

        private string ssoData(string token, string isAllowed, string serverUrl)
        {
            // var issuer = HttpContext.Current.Request.Url.AbsoluteUri;//.Authority; 
            var issuer = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase;
            string itemIssuant = DateTime.UtcNow.ToString("u");
            NameValueCollection data = new NameValueCollection()
                                {
                                    { "ID", token },
                                        { "Issuer",  issuer },
                                        { "Version", "2.0" },
                                        { "AssertionConsumerServiceIndex", "0" },
                                        { "IssueInstant", DateTime.UtcNow.ToString("u")},
                                        { "Format", "urn:oasis:names:tc:SAML:2.0:nameid-format:transient" },                                        
                                        { "AllowCreate", isAllowed}
                                };
            WebClient client = new WebClient();
            try
            {
               // LogManager.Log.Debug("Post JSON string to IDP");
                //ping first, if success -- go on, 
                //if failed: use the backup IdP...
                var ssoServer = availableServer();
                //LogManager.Log.Debug("ssoServer: " + ssoServer);
                var byteArray = client.UploadValues(new Uri(ssoServer), "POST", data);
                //LogManager.Log.Debug("JSON string: ");
               // LogManager.Log.Debug("Post JSON string to IDP: DONE.");
                string m = Encoding.UTF8.GetString(byteArray);
                return m;
            }
            catch (WebException ex)
            {
                return "Error SSO: " + ex.Message;
            }           
        }

        protected System.Collections.Generic.List<XElement> decryptResponse(string jsonData)
        {
            //update Oct 9, remove invalid characters in base64string
            string encodedString = Regex.Replace(jsonData, @"[^0-9a-zA-Z=+\/]", "");
            //LogManager.Log.Debug("### Before decode: " + encodedString);
            byte[] decodedBytes = Convert.FromBase64String(encodedString);
            string decodedText = Encoding.UTF8.GetString(decodedBytes);
           // LogManager.Log.Debug("### Decode succeed " + decodedText);

            var xDoc = XDocument.Parse(decodedText);
            var nodes = (from x in xDoc.Root.Descendants()
                         select x).ToList();
            //LogManager.Log.Debug("### Extracted data succeed");
            return nodes;
        }

        /// <summary>
        /// Get online server
        /// </summary>
        /// <returns></returns>
        public string GetOnlineServer()
        {
            return GetOnlineServer(Configuration["SSO:master_server"], Configuration["SSO:backup_server"]);
        }
        /// <summary>
        /// Get online server
        /// </summary>
        /// <returns></returns>
        protected string GetOnlineServer(string a, string b)
        {
            //LogManager.Log.Debug("GetOnlineServer(string a, string b) - b: " + b);
            if (IsOnline(a))
            {
              //  LogManager.Log.Debug("GetOnlineServer(string a, string b) - " + a + " is ONLINE");
                return a;//;.Replace("http:","https:");
            }
            else
            {
               // LogManager.Log.Debug("GetOnlineServer(string a, string b) - " + a + " is OFFLINE. Try using " + b);
                if (IsOnline(b))
                {
                 //   LogManager.Log.Debug("GetOnlineServer(string a, string b) - " + b + " is ONLINE");
                    return b;//.Replace("http:","https:");
                }
                else
                {
                   // LogManager.Log.Debug("GetOnlineServer(string a, string b) - All servers are DOWN.");
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Check if server is online
        /// </summary>
        /// <param name="destinateUrl"></param>
        /// <returns></returns>
        protected bool IsOnline(string destinateUrl)
        {
            try
            {
                WebRequest request = WebRequest.Create(destinateUrl);

                request.Method = "GET";
                // trust all certificates sent from server
                //ServicePointManager.ServerCertificateValidationCallback +=
                //    (sender, certificate, chain, sslPolicyErrors) => true;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                //Uri url = new Uri(destinateUrl);
                //string pingurl = string.Format("{0}", url.Host);
                //string host = pingurl;
                //Ping p = new Ping();
                //try
                //{
                //    PingReply reply = p.Send(pingurl.ToString(), 3000);
                //    if (reply.Status == IPStatus.Success)
                //        return true;
                //}
                //catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection could not be established due to: \n" + ex.Message);
            }
            finally
            {
            }
            return false;
        }

        public bool IsLogged()
        {
            if (_httpContextAccessor.HttpContext.Session.GetString("tkusername") != null)
            {
               
                {
                    // LogManager.Log.Debug("### Session[tkusername] is not NULL.");
                    logger.LogDebug("### Session[tkusername] is not NULL.");
                    if (_httpContextAccessor.HttpContext.Session.GetString("tkid") != null)
                    {
                        logger.LogDebug("### Session[tkid] is not NULL, sending to server");
                        string server = GetOnlineServer();
                        if (!string.IsNullOrEmpty(server))
                        {
                            WebClient wc = new WebClient();
                            var byteArray = wc.DownloadData(server + "/" + _httpContextAccessor.HttpContext.Session.GetString("tkid").ToString());
                            logger.LogDebug("### Is token alive? ");

                            if (byteArray.Length != 4)
                            {
                                logger.LogDebug("### YES");
                                return true;
                            }
                            else
                            {
                                logger.LogDebug("### NO");
                                return false;
                            }
                        }
                        else
                        {
                            logger.LogDebug("### Servers is not active, return false");
                            return false;
                        }
                    }
                    else
                    {
                        logger.LogDebug("### Session[tkid] is null ");
                        return false;
                    }
                }
            }
            else
            {
                logger.LogDebug("### Session[tkusername] is NULL.");
                return false;
            }
        }

    }

    
}