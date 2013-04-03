using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ezyd.Models;
using ezyd.Models.ezyd;
using Facebook;
using Facebook.Web;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;
using System.Text;
using System.Web.Security;
using System.Web.Helpers;
using ezyd.Models.stringHelper;
using ezyd.Models.helpers;


namespace ezyd.Controllers
{
    [HandleError]
    public class FriendsController : Controller
    {
        public FacebookSession FacebookSession
        {
            get { return (new CanvasAuthorizer().Session); }
        }

        public class FacebookUser
        {
            public long id { get; set; } //yes. the user id is of type long...dont use int
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string name { get; set; }
            public string email { get; set; }
        }

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

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FriendListDisplay(int tries = 0)
        {
            try
            {
                var fbApp = new FacebookClient(Session["accessToken"].ToString());
                dynamic result = fbApp.Get("me");

                UInt64 UId = Convert.ToUInt64(result.id);
                ViewData["UserID"] = UId;

                var DB = new EzydInstantDB();
                var reader = DB.SqlQuery("SELECT `senderID`, `receiverID` " +
                    "FROM `friends` " +
                    "WHERE " +
                        " `receiverAccepted` = 1 AND " +
                        "(`senderID` = " + result.id +
                        " OR `receiverID` = " + result.id + " )"
                );

                var friendList = new List<UInt64>();

                while (reader.Read())
                {
                    if (reader[0] == UId)
                        friendList.Add((UInt64)reader[1]);
                    else
                        friendList.Add((UInt64)reader[0]);
                }
                ViewData["friendList"] = friendList;
            }
            catch (NullReferenceException)
            {
                if (tries < 5) //5 tries
                {
                    return FriendListDisplay(tries + 1);
                }
                return View("uNotLogged");
            }
            catch (ArgumentNullException)
            {
                if (tries < 5)
                {
                    return FriendListDisplay(tries + 1);
                }
                return View("uNotLogged");
            }

            return View();
        }

        public ActionResult SendFriendInvitations(int tries = 0)
        {
            try
            {
                var fbApp = new FacebookClient(Session["accessToken"].ToString());
            }
            catch (NullReferenceException)
            {
                if (tries < 5) //5 tries
                {
                    return FriendListDisplay(tries + 1);
                }
                return View("uNotLogged");
            }
            catch (ArgumentNullException)
            {
                if (tries < 5)
                {
                    return FriendListDisplay(tries + 1);
                }
                return View("uNotLogged");
            }

            return View();
        }

        [AjaxOnly]
        [HttpPost]
        public ActionResult SendFriendInv(string FId)
        {
            try
            {
                BadCharsRemover.RemoveBadChars(ref FId);

                var DB = new EzydInstantDB();

                var fbApp = new FacebookClient(Session["accessToken"].ToString());
                dynamic result = fbApp.Get("me");

                DB.sendFriendInv(result.id, FId);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    ok = 0,
                    msg = e.Message
                });
            }

            return Json(new
            {
                ok = 1
            });
        }

        public ActionResult AcceptFriendInvitations(int tries = 0)
        {
            try
            {
                var fbApp = new FacebookClient(Session["accessToken"].ToString());
                dynamic result = fbApp.Get("me");

                UInt64 UId = Convert.ToUInt64(result.id);
                ViewData["UserID"] = UId;

                var DB = new EzydInstantDB();
                var reader = DB.SqlQuery("SELECT `senderID` " +
                    "FROM `friends` " +
                    "WHERE " +
                        "`receiverAccepted` = 0 " +
                        "AND " +
                        " `receiverID` = " + result.id
                );

                var friendInvs = new List<UInt64>();

                while (reader.Read())
                    friendInvs.Add(reader[0]);

                ViewData["friendInvs"] = friendInvs;

            }
            catch (NullReferenceException)
            {
                if (tries < 5) //5 tries
                {
                    return FriendListDisplay(tries + 1);
                }
                return View("uNotLogged");
            }
            catch (ArgumentNullException)
            {
                if (tries < 5)
                {
                    return FriendListDisplay(tries + 1);
                }
                return View("uNotLogged");
            }

            return View();
        }

        [AjaxOnly]
        [HttpPost]
        public ActionResult AcceptFriendInv(string FId)
        {
            try
            {
                BadCharsRemover.RemoveBadChars(ref FId);

                var DB = new EzydInstantDB();

                var fbApp = new FacebookClient(Session["accessToken"].ToString());
                dynamic result = fbApp.Get("me");

                DB.acceptFriendInv(result.id, FId);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    ok = 0,
                    msg = e.Message
                });
            }

            return Json(new
            {
                ok = 1
            });

        }
    }
}
