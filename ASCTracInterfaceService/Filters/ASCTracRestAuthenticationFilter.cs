using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace ASCTracInterfaceService.Filters
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ASCTracRestAuthenticationFilter : AuthorizationFilterAttribute
    {

        /// <summary>  
        /// Public default Constructor  
        /// </summary>  
        public ASCTracRestAuthenticationFilter() { }

        private readonly bool _isActive = true;

        private ascLibrary.ascDBUtils myDBUtils = new ascLibrary.ascDBUtils();
        private bool fInit = false;
        private bool fUsingAuthentication = false;

        private bool InitAuthenticate()
        {
            if (!fInit)
            {
                try
                {
                    string tmp = string.Empty;
                    var currDT = string.Empty;
                    try
                    {
                        myDBUtils.BuildConnectString("AliasASCTrac");
                        if (myDBUtils.ReadFieldFromDB("select CFGDATA from CFGSETTINGS WHERE CFGFIELD = 'GWInterfaceUseAuthentication'", "", ref tmp))
                            fUsingAuthentication = tmp.Equals("T");
                    }
                    catch (Exception EX1)
                    {
                        ascLibrary.ascUtils.ascWriteLog("ASCTracInterface", "Exception at BuildConnectString: " + EX1.ToString(), false);
                        myDBUtils.myConnString = "packet size=4096;user id=app_user;Password='WeH73w';data source=asc-cin-app01;persist security info=False;initial catalog=ASCTRAC904Dev";
                        if (myDBUtils.ReadFieldFromDB("select CFGDATA from CFGSETTINGS WHERE CFGFIELD = 'GWInterfaceUseAuthentication'", "", ref tmp))
                            fUsingAuthentication = tmp.Equals("T");
                    }
                    myDBUtils.ReadFieldFromDB("SELECT GETDATE()", "", ref currDT);
                    fInit = true;
                }
                catch (Exception ex)
                {
                    ascLibrary.ascUtils.ascWriteLog("ASCTracInterface", "Exception during Init Authenticate" + ex.ToString(), false);
                }
            }
            return (fInit);
        }


        /// <summary>  
        /// parameter isActive explicitly enables/disables this filetr.  
        /// </summary>  
        /// <param name="isActive"></param>  
        public ASCTracRestAuthenticationFilter(bool isActive)
        {
            _isActive = isActive;
        }

        /// <summary>  
        /// Checks Bearer authentication request  
        /// </summary>  
        /// <param name="filterContext"></param>  
        public override void OnAuthorization(HttpActionContext filterContext)
        {
            if (!_isActive) return;

            InitAuthenticate();

            var identity = FetchAuthHeader(filterContext);
            if (identity == null)
            {
                if (fUsingAuthentication)
                {
                    ChallengeAuthRequest(filterContext);
                }
                return;
            }
            var genericPrincipal = new GenericPrincipal(identity, null);
            Thread.CurrentPrincipal = genericPrincipal;
            if (!OnAuthorizeUser(identity.Token, filterContext))
            {
                ChallengeAuthRequest(filterContext);
                return;
            }
            base.OnAuthorization(filterContext);
        }

        /// <summary>  
        /// Virtual method.Can be overriden with the custom Authorization.  
        /// </summary>  
        /// <param name="user"></param>  
        /// <param name="psswd"></param>  
        /// <param name="filterContext"></param>  
        /// <returns></returns>  
        protected virtual bool OnAuthorizeUser(string user, string psswd, HttpActionContext filterContext)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(psswd)) return false;
            return true;
        }
        /// <summary>  
        /// Virtual method.Can be overriden with the custom Authorization.  
        /// </summary>  
        /// <param name="token"></param>  
        /// <param name="filterContext"></param>  
        /// <returns></returns>  
        protected virtual bool OnAuthorizeUser(string token, HttpActionContext filterContext)
        {
            if (string.IsNullOrEmpty(token) ) return false;
            return true;
        }

        /// <summary>  
        /// Checks for autrhorization header in the request and parses it, creates user credentials and returns as BasicAuthenticationIdentity  
        /// </summary>  
        /// <param name="filterContext"></param>  
        protected virtual BasicAuthenticationIdentity FetchAuthHeader(HttpActionContext filterContext)
        {
            string authHeaderValue = null;
            var authRequest = filterContext.Request.Headers.Authorization;
            if (authRequest != null && !String.IsNullOrEmpty(authRequest.Scheme) && authRequest.Scheme == "Bearer") authHeaderValue = authRequest.Parameter;
            if (string.IsNullOrEmpty(authHeaderValue)) return null;
           // authHeaderValue = authHeaderValue; // Encoding.Default.GetString(Convert.FromBase64String(authHeaderValue));
            var credentials = authHeaderValue.Split(':');
            return credentials.Length < 2 ? new BasicAuthenticationIdentity(credentials[0], String.Empty) : new BasicAuthenticationIdentity(credentials[0], credentials[1]);
        }


        /// <summary>  
        /// Send the Authentication Challenge request  
        /// </summary>  
        /// <param name="filterContext"></param>  
        private static void ChallengeAuthRequest(HttpActionContext filterContext)
        {
            var dnsHost = filterContext.Request.RequestUri.DnsSafeHost;
            filterContext.Response = filterContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            filterContext.Response.Headers.Add("WWW-Authenticate", string.Format("Bearer realm=\"{0}\"", dnsHost));
        }
    }
}