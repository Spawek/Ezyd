using System.Configuration;

namespace CSMvc3FacebookApp.Controllers
{
    using System.Web.Mvc;
    using Facebook;
    using Facebook.Web;
    using Facebook.Web.Mvc;
    using System.Net;
    using System.Text;
    using System;

    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.IO;
    using System.Text;
    using System.Runtime.Serialization.Json;
    using System.Web.Security;
    using System.Text.RegularExpressions;
    using System.Security.Cryptography;




    [HandleError]
    public class HomeController : Controller
    {
        public FacebookSession FacebookSession
        {
            get { return (new CanvasAuthorizer().Session); }
        }

        
        public ActionResult Index()
        {
            
            return View();
        }

        //this is the statically typed representation of the JSON object that will get returned from: https://graph.facebook.com/me
        public class FacebookUser
        {
            public long id { get; set; } //yes. the user id is of type long...dont use int
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string name { get; set; }
            public string email { get; set; }

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index(string signed_request)
        {
            //when the user gets on the landing page, simply return the signed_request 
            //this is just a starting point btw
            return Content(signed_request);
        }

        //this controller action will be called when Facebook redirects
        [HttpGet]
        [ActionName("handshake")]
        public ActionResult Handshake(string code)
        {
            //after authentication, Facebook will redirect to this controller action with a QueryString parameter called "code" (this is Facebook's Session key)

            //example uri: http://www.borrowedgames.com/facebook/handshake/?code=2.DQUGad7_kFVGqKTeGUqQTQ__.3600.1273809600-1756053625|dil1rmAUjgbViM_GQutw-PEgPIg.

            //this is your Facebook App ID
            string clientId = "241822175888190";

            //this is your Secret Key
            string clientSecret = "a03f56d45568b6ebc9eb6c4503090e00";

            //we have to request an access token from the following Uri
            string url = "https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}";

            //your redirect uri must be EXACTLY the same Uri that caused the initial authentication handshake
            string redirectUri = "http://www.borrowedgames.com/facebook/handshake/";

            //Create a webrequest to perform the request against the Uri
            WebRequest request = WebRequest.Create(string.Format(url, clientId, redirectUri, clientSecret, code));

            //read out the response as a utf-8 encoding and parse out the access_token
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader streamReader = new StreamReader(stream, encode);
            string accessToken = streamReader.ReadToEnd().Replace("access_token=", "");
            streamReader.Close();
            response.Close();

            //set the access token to some session variable so it can be used through out the session
            Session["FacebookAccessToken"] = accessToken;

            //now that we have an access token, query the Graph Api for the JSON representation of the User
            url = "https://graph.facebook.com/me?access_token={0}";

            //create the request to https://graph.facebook.com/me
            request = WebRequest.Create(string.Format(url, accessToken));

            //Get the response
            response = request.GetResponse();

            //Get the response stream
            stream = response.GetResponseStream();

            //Take our statically typed representation of the JSON User and deserialize the response stream
            //using the DataContractJsonSerializer
            DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(FacebookUser));
            FacebookUser user = new FacebookUser();
            user = dataContractJsonSerializer.ReadObject(stream) as FacebookUser;

            //close the stream
            response.Close();

            //capture the UserId
            Session["FacebookUserId"] = user.id;

            //Set the forms authentication auth cookie
            FormsAuthentication.SetAuthCookie(user.email, false);

            //redirect to home page so that user can start using your application      
            return RedirectToAction("Home", "User");
        }

        /* new one
        [CanvasAuthorize(Permissions = "user_about_me")]
        public ActionResult About()
        {
            CanvasAuthorizer auth = new CanvasAuthorizer();
            FacebookWebClient client = new FacebookWebClient(auth.FacebookWebRequest.Session.AccessToken);

            dynamic result = client.Get("me");

            ViewData["Firstname"] = result.first_name;
            ViewData["Lastname"] = result.last_name;


            return View();
        }
        */

        //old one
        [CanvasAuthorize(Permissions = "user_about_me")]
        public ActionResult About()
        {
            var fbApp = new FacebookClient(this.FacebookSession.AccessToken);

            dynamic result = fbApp.Get("me");

            ViewData["Firstname"] = result.first_name;
            ViewData["Lastname"] = result.last_name;


            return View();
        }

        /*
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // YOU DONT NEED ANY OF THIS IN YOUR APPLICATION
            // THIS METHOD JUST CHECKS TO SEE IF YOU HAVE SETUP
            // THE SAMPLE. IF THE SAMPLE IS NOT SETUP YOU WILL
            // BE SENT TO THE GETTING STARTED PAGE.

            base.OnActionExecuting(filterContext);

            bool isSetup = false;
            var settings = ConfigurationManager.GetSection("facebookSettings");
            if (settings != null)
            {
                var current = settings as IFacebookApplication;
                if (current.AppId != "241822175888190" &&
                    current.AppSecret != "a03f56d45568b6ebc9eb6c4503090e00" &&
                    current.CanvasUrl != "http://mojLocalHost:5000/")
                {
                    isSetup = true;
                }
            }

            if (!isSetup)
            {
                filterContext.Result = View("GettingStarted");
            }
        }
        */
    }
}
